//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using MCBAWebApplication.Data;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using MCBAWebApplication.Models;
//using MCBAWebApplication.Utilities;


//namespace MCBAWebApplication.BackgroundServices
//{
//    public class BillPayBackgroundService : BackgroundService
//    {
//        private readonly IServiceProvider _services;
//        private readonly ILogger<BillPayBackgroundService> _logger;

//        public BillPayBackgroundService(IServiceProvider services, ILogger<BillPayBackgroundService> logger)
//        {
//            _services = services;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//        {
//            while (!cancellationToken.IsCancellationRequested)
//            {
//                await DoWork(cancellationToken);
//                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
//            }
//        }

//        private async Task DoWork(CancellationToken cancellationToken)
//        {
//            using var scope = _services.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<McbaContext>();

//            var billPays = await context.BillPays.ToListAsync(cancellationToken);

//            foreach(var BillPay in billPays)
//            {
//                if (BillPay.Status == Status.Pending)
//                {
//                    if (DateTime.Compare(DateTime.Now, BillPay.ScheduleDate) >= 0)
//                    {
//                        var account = await context.Accounts.FindAsync(BillPay.AccountNumber);
//                        int chAmount = 0;

//                        if (account.AccountType == AccountType.Checking)
//                            chAmount = 200;
//                        if (account.Balance < (BillPay.Amount + chAmount))
//                            BillPay.Status = Status.Failed;
//                        else
//                        {
//                            BillPay.Status = Status.Complete;
//                            account.Balance -= BillPay.Amount;
//                            account.Transactions.Add(
//                                new Transaction
//                                {
//                                    TransactionType = TransactionType.BillPay,
//                                    DestinationAccountNumber = BillPay.PayeeID,
//                                    Amount = BillPay.Amount,
//                                    TransactionTimeUtc = DateTime.UtcNow
//                                });
//                        }
//                    } 
//                }
//            }

//            await context.SaveChangesAsync(cancellationToken);

//            _logger.LogInformation("Bill Pay Background Service is work complete.");
//        }
//    }
//}
