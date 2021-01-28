using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WDT_AS2.Data;
using WDT_AS2.ViewModels;
using WDT_AS2.Utilities;
using WDT_AS2.Filters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;

namespace WDT_AS2.Models
{
    [AuthorizeCustomer]
    public class CustomerController : Controller
    {
        private readonly McbaContext _context;

        // ReSharper disable once PossibleInvalidOperationException
        private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        public CustomerController(McbaContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            // Lazy loading.
            // The Customer.Accounts property will be lazy loaded upon demand.
            var customer = await _context.Customers.FindAsync(CustomerID);

            // OR
            // Eager loading.
            //var customer = await _context.Customers.Include(x => x.Accounts).
            //    FirstOrDefaultAsync(x => x.CustomerID == _customerID);

            return View(customer);
        }

        public async Task<IActionResult> Deposit(int id) => View(await _context.Accounts.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if(amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if(amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if(!ModelState.IsValid)
            {
                ViewBag.Amount = amount;
                return View(account);
            }

            // Note this code could be moved out of the controller, e.g., into the Model.
            account.Balance += amount;
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int id) => View(await _context.Accounts.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);
            var customer = await _context.Customers.FindAsync(account.CustomerID);

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (!ModelState.IsValid)
            {
                ViewBag.Amount = amount;
                return View(account);
            }

            // Note this code could be moved out of the controller, e.g., into the Model.
            account.Balance -= amount;
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Withdraw,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            if (GetTransactionCount(customer) > 4)
            {
                account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.ServiceCharge,
                    Amount = 0.1m,
                    TransactionTimeUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public int GetTransactionCount(Customer customer)
        {
            int count = 0;

            foreach(var account in customer.Accounts)
            {
                foreach (var transaction in account.Transactions)
                {
                    if ((transaction.TransactionType == TransactionType.Transfer || transaction.TransactionType == TransactionType.Withdraw) && transaction.DestinationAccountNumber != transaction.AccountNumber)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public async Task<IActionResult> Transfer(int id)
        {
            var accList = _context.Accounts.Where(x => x.AccountNumber != id).Select(x => x.AccountNumber).ToList();
            ViewBag.AccountList = new SelectList(accList, "AccountNumber");
            return View(await _context.Accounts.FindAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(int id, int AccountNumber, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);
            var transferAccount = await _context.Accounts.FindAsync(AccountNumber);
            var customer = await _context.Customers.FindAsync(account.CustomerID);

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (amount > account.Balance)
                ModelState.AddModelError(nameof(amount), "Insufficient Funds.");
            if (transferAccount == null)
                ModelState.AddModelError(nameof(AccountNumber), "Invalid Account ID.");
            if (!ModelState.IsValid)
            {
                ViewBag.AccountNumber = AccountNumber;
                ViewBag.Amount = amount;
                return View(account);
            }

            // Note this code could be moved out of the controller, e.g., into the Model.
            account.Balance -= amount;
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Transfer,
                    DestinationAccountNumber = AccountNumber,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            transferAccount.Balance += amount;
            transferAccount.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Transfer,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            if (GetTransactionCount(customer) > 4)
            {
                account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.ServiceCharge,
                    Amount = 0.2m,
                    TransactionTimeUtc = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Statements()
        {
            // Lazy loading.
            // The Customer.Accounts property will be lazy loaded upon demand.
            var customer = await _context.Customers.FindAsync(CustomerID);

            // OR
            // Eager loading.
            //var customer = await _context.Customers.Include(x => x.Accounts).
            //    FirstOrDefaultAsync(x => x.CustomerID == _customerID);

            return View(customer);
        }

        public async Task<IActionResult> AccountStatement(int id, int? page = 1)
        {
            var customer = await _context.Customers.FindAsync(CustomerID);
            ViewBag.Customer = customer;

            var account = await _context.Accounts.FindAsync(id);
            ViewBag.Account = account;


            int pageSize = 4;
            var transactionListPaged = await _context.Transactions.Where(x => x.AccountNumber == id).ToPagedListAsync(page, pageSize);
            
            return View(transactionListPaged);
        }

        public async Task<IActionResult> BillPay(int id)
        {
            var accList = _context.Payees.Select(x => x.PayeeID).ToList();
            ViewBag.PayeeList = new SelectList(accList, "AccountNumber");
            return View(await _context.Accounts.FindAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> BillPay(int id, int AccountNumber, decimal amount, DateTime PickedDate, Period Period)
        {
            var account = await _context.Accounts.FindAsync(id);
            var payeeAccount = await _context.Payees.FindAsync(AccountNumber);

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (amount > account.Balance)
                ModelState.AddModelError(nameof(amount), "Insufficeint Funds.");
            if (payeeAccount == null)
                ModelState.AddModelError(nameof(AccountNumber), "Invalid Account ID.");
            if (!ModelState.IsValid)
            {
                ViewBag.AccountNumber = AccountNumber;
                ViewBag.Amount = amount;
                return View(account);
            }

            // Note this code could be moved out of the controller, e.g., into the Model.
            account.Balance -= amount;
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.BillPay,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.UtcNow
                });

            account.BillPays.Add(
                new BillPay
                {
                    PayeeID = AccountNumber,
                    Amount = amount,
                    ScheduleDate = PickedDate,
                    Period = Period,
                    ModifyDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ScheduledPayments(int? page = 1)
        {
            var customer = await _context.Customers.FindAsync(CustomerID);
            ViewBag.Customer = customer;

            var query =  from b in _context.BillPays
                         join a in _context.Accounts
                             on b.AccountNumber equals a.AccountNumber
                         join p in _context.Payees
                             on b.PayeeID equals p.PayeeID
                         where (a.CustomerID == CustomerID)
                         select new ScheduledPaymentsViewModel
                         {
                             PayeeName = p.PayeeName,
                             Amount = b.Amount,
                             ScheduleDate = b.ScheduleDate,
                             Period = b.Period
                         };

            int pageSize = 4;
            var billPayListPaged = await query.ToPagedListAsync(page, pageSize);

            return View(billPayListPaged);
        }
    }
}
