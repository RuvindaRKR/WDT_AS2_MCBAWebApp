using System;
using System.Threading;
using System.Threading.Tasks;
using WDT_AS2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WDT_AS2.Models;
using WDT_AS2.Utilities;
using System.Collections.Generic;
using WDT_AS2.Controllers;
using MimeKit;
using MailKit.Net.Smtp;
using SharedUtils;
using System.IO;
using System.Linq;

namespace WDT_AS2.BackgroundServices
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<EmailBackgroundService> _logger;
        private DateTime _lastRunTime;

        public EmailBackgroundService(IServiceProvider services, ILogger<EmailBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
            _lastRunTime = DateTime.Now;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await DoWork(cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(3), cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<McbaContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var customers = await context.Customers.ToListAsync(cancellationToken);
            var transactions = await context.Transactions.ToListAsync(cancellationToken);
            

            foreach (var customer in customers)
            {
                decimal changedBalance = 0;
                decimal totatAccountBalance = 0;
                List<Transaction> transactionsList = new();

                foreach (var account in customer.Accounts)
                {
                    totatAccountBalance += account.Balance;
                    foreach (var transaction in account.Transactions)
                    {
                        if (DateTime.Compare(_lastRunTime, transaction.TransactionTimeUtc) < 0)
                        {
                            if(transaction.TransactionType == TransactionType.Deposit || (transaction.TransactionType == TransactionType.Transfer && transaction.DestinationAccountNumber ==null))
                                changedBalance += transaction.Amount;
                            if(transaction.TransactionType == TransactionType.Withdraw || transaction.TransactionType == TransactionType.BillPay || transaction.TransactionType == TransactionType.ServiceCharge || transaction.TransactionType == TransactionType.Transfer)
                                changedBalance -= transaction.Amount;

                            transactionsList.Add(transaction);
                        }
 
                    }
                }

                if(transactionsList.Capacity != 0)
                {
                    var assembly = typeof(CustomerController).Assembly;
                    Stream resource = assembly.GetManifestResourceStream("WDT_AS2.EmailTemplates.Customerreport.cshtml");
                    string text = string.Empty;
                    using (var reader = new StreamReader(resource))
                    {
                        text = await reader.ReadToEndAsync();
                    }
                    var body = await emailService.BuildBody(text, new
                    {
                        Name = "Transactions Statement MCBA",
                        Balance = totatAccountBalance,
                        Amount = changedBalance,
                        Data = transactionsList,
                    });
                    await emailService.SendEmail(new List<EmailRecipient>
                        {
                            new EmailRecipient
                            {
                                Email = "S3804158@student.rmit.edu.au",
                                Name = "Transactions Statement MCBA"
                            }
                        }, body, "MCBA");
                }
            }
            _lastRunTime = DateTime.Now;


            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email Background Service is work complete.");
        }
    }
}
