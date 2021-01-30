using System;
using System.Threading;
using System.Threading.Tasks;
using AdminWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AdminWebAPI.Models;
using AdminWebAPI.Utilities;


namespace AdminWebAPI.BackgroundServices
{
    public class UnlockBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<UnlockBackgroundService> _logger;

        public UnlockBackgroundService(IServiceProvider services, ILogger<UnlockBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await DoWork(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<McbaContext>();

            var customers = await context.Customers.ToListAsync(cancellationToken);

            foreach(var customer in customers)
            {
                if (customer.AccountStatus == AccountStatus.Locked)
                {
                    context.Customers.Update(Unlock(customer));
                }
            }
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Account Unlock Background Service is work complete.");
        }

        private Customer Unlock(Customer customer)
        {
            customer = customer with { AccountStatus = AccountStatus.UnLocked };
            return customer;

        }
    }
}
