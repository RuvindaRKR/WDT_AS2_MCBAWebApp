using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using AdminWebAPI.Models;
using AdminWebAPI.Utilities;

namespace AdminWebAPI.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<McbaContext>();
            const string initialDeposit = "Initial deposit";
            const string format = "dd/MM/yyyy hh:mm:ss tt";

            // Look for customers.
            if (context.Customers.Any())
                return; // DB has already been seeded.

            context.Customers.AddRange(
                new Customer
                {
                    CustomerID = 2100,
                    CustomerName = "Matthew Bolger",
                    TFN = "12345678901",
                    Address = "123 Fake Street",
                    City = "Melbourne",
                    State = "VIC",
                    PostCode = "3000",
                    Phone = "0434409681"
                },
                new Customer
                {
                    CustomerID = 2200,
                    CustomerName = "Rodney Cocker",
                    TFN = "09876543210",
                    Address = "456 Real Road",
                    City = "Melbourne",
                    State = "NSW",
                    PostCode = "3005",
                    Phone = "0434419681"
                },
                new Customer
                {
                    CustomerID = 2300,
                    CustomerName = "Shekhar Kalra"
                });

            context.Payees.AddRange(
                new Payee
                {
                    PayeeID = 6100,
                    PayeeName = "Ruvinda R",
                    Address = "987 Fake Street",
                    City = "Melbourne",
                    State = "VIC",
                    PostCode = "2000",
                    Phone = "0434409121"
                },
                new Payee
                {
                    PayeeID = 6200,
                    PayeeName = "Luke G",
                    Address = "765 Real Road",
                    City = "Melbourne",
                    State = "NSW",
                    PostCode = "2005",
                    Phone = "0434454685"
                },
                new Payee
                {
                    PayeeID = 6300,
                    PayeeName = "Falca"
                });

            context.Logins.AddRange(
                new Login
                {
                    LoginID = "12345678",
                    CustomerID = 2100,
                    PasswordHash = "YBNbEL4Lk8yMEWxiKkGBeoILHTU7WZ9n8jJSy8TNx0DAzNEFVsIVNRktiQV+I8d2",
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null)
                },
                new Login
                {
                    LoginID = "38074569",
                    CustomerID = 2200,
                    PasswordHash = "EehwB3qMkWImf/fQPlhcka6pBMZBLlPWyiDW6NLkAh4ZFu2KNDQKONxElNsg7V04",
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:50:00 PM", format, null)
                },
                new Login
                {
                    LoginID = "17963428",
                    CustomerID = 2300,
                    PasswordHash = "LuiVJWbY4A3y1SilhMU5P00K54cGEvClx5Y+xWHq7VpyIUe5fe7m+WeI0iwid7GE",
                    ModifyDate = DateTime.ParseExact("08/05/2020 10:00:00 PM", format, null)
                });

            context.Accounts.AddRange(
                new Account
                {
                    AccountNumber = 4100,
                    AccountType = AccountType.Saving,
                    CustomerID = 2100,
                    Balance = 500,
                    ModifyDate = DateTime.ParseExact("07/03/2020 10:00:00 AM", format, null)
                },
                new Account
                {
                    AccountNumber = 4101,
                    AccountType = AccountType.Checking,
                    CustomerID = 2100,
                    Balance = 500,
                    ModifyDate = DateTime.ParseExact("08/04/2020 09:00:00 PM", format, null)
                },
                new Account
                {
                    AccountNumber = 4200,
                    AccountType = AccountType.Saving,
                    CustomerID = 2200,
                    Balance = 500.95m,
                    ModifyDate = DateTime.ParseExact("08/05/2020 08:50:00 PM", format, null)
                },
                new Account
                {
                    AccountNumber = 4300,
                    AccountType = AccountType.Checking,
                    CustomerID = 2300,
                    Balance = 1250.50m,
                    ModifyDate = DateTime.ParseExact("08/05/2020 08:40:00 PM", format, null)
                });


            context.Transactions.AddRange(
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4100,
                    DestinationAccountNumber = 4101,
                    Amount = 100,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4100,
                    DestinationAccountNumber = 4200,
                    Amount = 100,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("09/06/2020 09:00:00 AM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4100,
                    Amount = 100,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("09/06/2020 01:00:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4100,
                    Amount = 100,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("09/06/2020 03:00:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4100,
                    Amount = 100,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("10/06/2020 11:00:00 AM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4101,
                    Amount = 500,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("08/06/2020 08:30:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4200,
                    Amount = 500,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("08/06/2020 09:00:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4200,
                    Amount = 0.95m,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("08/06/2020 09:00:00 PM", format, null)
                },
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = 4300,
                    Amount = 1250.50m,
                    Comment = initialDeposit,
                    TransactionTimeUtc = DateTime.ParseExact("08/06/2020 10:00:00 PM", format, null)
                });

            context.BillPays.AddRange(
                new BillPay
                {
                    AccountNumber = 4100,
                    PayeeID = 6100,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Failed
                },
                new BillPay
                {
                    AccountNumber = 4100,
                    PayeeID = 6100,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Complete
                },
                new BillPay
                {
                    AccountNumber = 4100,
                    PayeeID = 6100,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("10/06/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Failed
                },
                new BillPay
                {
                    AccountNumber = 4101,
                    PayeeID = 6200,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("12/12/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Complete
                },
                new BillPay
                {
                    AccountNumber = 4200,
                    PayeeID = 6200,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("14/11/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Failed
                },
                new BillPay
                {
                    AccountNumber = 4300,
                    PayeeID = 6200,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("16/10/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Complete
                },
                new BillPay
                {
                    AccountNumber = 4300,
                    PayeeID = 6300,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("18/07/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Failed
                },
                new BillPay
                {
                    AccountNumber = 4200,
                    PayeeID = 6300,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("20/08/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Complete
                },
                new BillPay
                {
                    AccountNumber = 4101,
                    PayeeID = 6300,
                    Amount = 100,
                    ScheduleDate = DateTime.ParseExact("25/09/2020 08:00:00 PM", format, null),
                    Period = Period.OnceOff,
                    ModifyDate = DateTime.ParseExact("08/06/2020 08:00:00 PM", format, null),
                    Status = Status.Failed
                }) ;

            context.SaveChanges();
        }
    }
}
