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
using WDT_AS2.Models;

namespace WDT_AS2.Controllers
{
    [AuthorizeCustomer]
    public class CustomerController : Controller
    {
        private readonly McbaContext _context;

        // ReSharper disable once PossibleInvalidOperationException
        private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

        public CustomerController(McbaContext context) => _context = context;

        public async Task<IActionResult> Index() => View(await _context.Customers.FindAsync(CustomerID));

        public async Task<IActionResult> Deposit(int id) => View(await _context.Accounts.FindAsync(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);

            if(amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
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
                    TransactionTimeUtc = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int id) => View(await _context.Accounts.FindAsync(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);
            var customer = await _context.Customers.FindAsync(account.CustomerID);
            var transactionCount = _context.Transactions.Where(x => x.TransactionType == TransactionType.Transfer || x.TransactionType == TransactionType.Withdraw).Count();
            decimal fee = 0;
            int chAmount = 0;

            if (transactionCount > 4)
                fee = 0.1m;
            if (account.AccountType == AccountType.Checking)
                chAmount = 200;
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if ((amount + fee + chAmount) > account.Balance)
                ModelState.AddModelError(nameof(amount), "Insufficient Funds.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (!ModelState.IsValid)
            {
                ViewBag.Amount = amount;
                return View(account);
            }

            // Note this code could be moved out of the controller, e.g., into the Model.
            account.Balance -= (amount + fee);
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Withdraw,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                });

            if (transactionCount > 4)
            {
                account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.ServiceCharge,
                    Amount = fee,
                    TransactionTimeUtc = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Transfer(int id)
        {
            var accList = _context.Accounts.Where(x => x.AccountNumber != id).Select(x => x.AccountNumber).ToList();
            ViewBag.AccountList = new SelectList(accList, "AccountNumber");
            return View(await _context.Accounts.FindAsync(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, int AccountNumber, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(id);
            var transferAccount = await _context.Accounts.FindAsync(AccountNumber);
            var customer = await _context.Customers.FindAsync(account.CustomerID);
            var transactionCount = _context.Transactions.Where(x => x.TransactionType == TransactionType.Transfer || x.TransactionType == TransactionType.Withdraw).Count();
            decimal fee = 0;
            int chAmount = 0;

            if (transactionCount > 4)
                fee = 0.2m;
            if (account.AccountType == AccountType.Checking)
                chAmount = 200;
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if ((amount + fee + chAmount) > account.Balance)
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
            if (transactionCount > 4)
            {
                account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.ServiceCharge,
                    Amount = fee,
                    TransactionTimeUtc = DateTime.Now
                });
            }

            account.Balance -= (amount+fee);
            account.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Transfer,
                    DestinationAccountNumber = AccountNumber,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                });

            transferAccount.Balance += amount;
            transferAccount.Transactions.Add(
                new Transaction
                {
                    TransactionType = TransactionType.Transfer,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Statements() 
        {
            var accList = await _context.Accounts.Where(x => x.CustomerID == CustomerID).Select(x => x.AccountNumber).ToListAsync();
            ViewBag.AccList = new SelectList(accList, "AccountNumber");
            return View();
        }


        public async Task<IActionResult> AccountStatement(int MyAccountNumber, int? page = 1)
        {
            var customer = await _context.Customers.FindAsync(CustomerID);
            ViewBag.Customer = customer;

            var account = await _context.Accounts.FindAsync(MyAccountNumber);
            ViewBag.Account = account;

            int pageSize = 4;
            var transactionListPaged = await _context.Transactions.Where(x => x.AccountNumber == MyAccountNumber).ToPagedListAsync(page, pageSize);
            
            return View(transactionListPaged);
        }

        public async Task<IActionResult> BillPay()
        {
            var payeeList = await _context.Payees.Select(x => x.PayeeID).ToListAsync();
            ViewBag.PayeeList = new SelectList(payeeList, "AccountNumber");
            var accList = await _context.Accounts.Where(x => x.CustomerID == CustomerID).Select(x => x.AccountNumber).ToListAsync();
            ViewBag.AccList = new SelectList(accList, "AccountNumber");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BillPay(int MyAccountNumber, int PayeeAccountNumber, decimal amount, DateTime PickedDate, Period Period)
        {
            var account = await _context.Accounts.FindAsync(MyAccountNumber);
            var payeeAccount = await _context.Payees.FindAsync(PayeeAccountNumber);
            int chAmount = 0;

            if (account.AccountType == AccountType.Checking)
                chAmount = 200;
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (amount > (account.Balance + chAmount))
                ModelState.AddModelError(nameof(amount), "Insufficeint Funds.");
            if (!ModelState.IsValid)
            {
                ViewBag.Amount = amount;
                return View(account);
            }

            account.BillPays.Add(
                new BillPay
                {
                    PayeeID = PayeeAccountNumber,
                    Amount = amount,
                    Status = Status.Pending,
                    ScheduleDate = PickedDate,
                    Period = Period,
                    ModifyDate = DateTime.Now
                });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ScheduledPayments(int? page = 1, Status? filter = null)
        {
            var customer = await _context.Customers.FindAsync(CustomerID);
            ViewBag.Customer = customer;

            // all billpays
            var query = from b in _context.BillPays
                        join a in _context.Accounts
                            on b.AccountNumber equals a.AccountNumber
                        join p in _context.Payees
                            on b.PayeeID equals p.PayeeID
                        where (a.CustomerID == CustomerID)
                        orderby b.ScheduleDate descending
                        select new ScheduledPaymentsViewModel
                        {
                            BillPayID = b.BillPayID,
                            PayeeName = p.PayeeName,
                            Amount = b.Amount,
                            Status = b.Status,
                            ScheduleDate = b.ScheduleDate,
                            Period = b.Period
                        };

            switch(filter)
            {
                case Status.Pending:
                    query = query.Where(b => b.Status == Status.Pending);
                    ViewBag.TableFilter = Status.Pending;
                    break;
                case Status.Complete:
                    query = query.Where(b => b.Status == Status.Complete);
                    ViewBag.TableFilter = Status.Complete;
                    break;
                case Status.Failed:
                    query = query.Where(b => b.Status == Status.Failed);
                    ViewBag.TableFilter = Status.Failed;
                    break;
                default:
                    break;
            }

            int pageSize = 4;
            var billPayListPaged = await query.ToPagedListAsync(page, pageSize);

            return View(billPayListPaged);
        }

        public async Task<IActionResult> ModifyBillPay(int id) => View(await _context.BillPays.FindAsync(id));
        
        [HttpPost]
        public async Task<IActionResult> ModifyBillPay(int BillPayID, int AccountNumber, decimal Amount, DateTime ScheduleDate, Period Period)
        {
            var billpay = await _context.BillPays.FindAsync(BillPayID);
            var account = await _context.Accounts.FindAsync(AccountNumber);
            int chAmount = 0;

            if (account.AccountType == AccountType.Checking)
                chAmount = 200;
            if (Amount <= 0)
                ModelState.AddModelError(nameof(Amount), "Amount must be positive.");
            if (Amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(Amount), "Amount cannot have more than 2 decimal places.");
            if (Amount > (account.Balance + chAmount))
                ModelState.AddModelError(nameof(Amount), "Insufficient Funds.");
            if (DateTime.Compare(DateTime.Now, ScheduleDate) > 0)
                ModelState.AddModelError(nameof(ScheduleDate), "Select a time in the future.");
            if (!ModelState.IsValid)
            {
                ViewBag.Amount = Amount;
                ViewBag.ScheduleDate = ScheduleDate;
                return View(billpay);
            }

            billpay.Amount = Amount;
            billpay.ScheduleDate = ScheduleDate;
            billpay.Period = Period;
            billpay.ModifyDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DeleteBillPay(int id) => View(await _context.BillPays.FindAsync(id));


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int BillPayID)
        {
            var billpay = await _context.BillPays.FindAsync(BillPayID);
            _context.BillPays.Remove(billpay);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ScheduledPayments));
        }
    }
}
