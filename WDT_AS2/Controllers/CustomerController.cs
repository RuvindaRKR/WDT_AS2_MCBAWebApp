using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WDT_AS2.Data;
using WDT_AS2.Models;
using WDT_AS2.Utilities;
using WDT_AS2.Filters;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public ArrayList GetAllAccountNumbers(int id)
        {
            var accounts = _context.Accounts.ToList();
            
            ArrayList _accountNumbers = new ArrayList();
            foreach (var customer in _context.Customers)
            {
                foreach (var account in customer.Accounts)
                {
                    if(account.AccountNumber != id)
                        _accountNumbers.Add(account.AccountNumber);
                }
            }

            return _accountNumbers;
        }

        public async Task<IActionResult> Transfer(int id)
        {
            //List<Account> accList = _context.Accounts.ToList();
            //ViewBag.AccountList = new SelectList(accList, "AccountNumber");
            ViewBag.AccountList = new SelectList(GetAllAccountNumbers(id), "AccountNumber");
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
                ModelState.AddModelError(nameof(amount), "Insufficeint Funds.");
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

        public async Task<IActionResult> Statement()
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
    }
}
