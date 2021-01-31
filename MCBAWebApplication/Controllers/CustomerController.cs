using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MCBAWebApplication.ViewModels;
using MCBAWebApplication.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using MCBAWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Text;

namespace MCBAWebApplication.Controllers
{
    [Authorize(Roles = "customer")]
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private HttpClient Client => _clientFactory.CreateClient("api");
        private UserManager<ApplicationUser> _userManager;

        public CustomerController(IHttpClientFactory clientFactory, UserManager<ApplicationUser> userManager)
        {
            _clientFactory = clientFactory;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //  find current user's CustomerID
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve customer object
            var customerResponse = await Client.GetAsync($"api/customers/{customerid}");
            if (!customerResponse.IsSuccessStatusCode)
                throw new Exception();
            var customerResult = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(customerResult);
            ViewBag.CustomerName = customer.CustomerName;

            //  retrieve accounts
            var accountsResponse = await Client.GetAsync($"api/accounts");
            if (!accountsResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

            //  remove unnecessary accounts 
            foreach (var account in accounts.ToList())
            {
                if (account.CustomerID != customerid)
                {
                    accounts.Remove(account);
                }
            }

            return View(accounts);
        }

        public async Task<IActionResult> Deposit(int id)
        {
            //  retrieve account
            var accountResponse = await Client.GetAsync($"api/accounts/{id}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, decimal amount)
        {
            //  retrieve account
            var accountResponse = await Client.GetAsync($"api/accounts/{id}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);

            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if (!ModelState.IsValid)
            {
                ViewBag.Amount = amount;
                return View(account);
            }

            //  update account balance
            account.Balance += amount;
            var content = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            var response = Client.PutAsync("api/accounts", content).Result;
            if (response.IsSuccessStatusCode)
            {
                //  add transaction to database
                Transaction transaction =
                new Transaction
                {
                    TransactionType = TransactionType.Deposit,
                    AccountNumber = account.AccountNumber,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                };

                var tcontent = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
                var tresponse = Client.PostAsync("api/transactions", tcontent).Result;

                if (tresponse.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(account);
        }

        public async Task<IActionResult> Withdraw(int id)
        {
            var accountResponse = await Client.GetAsync($"api/accounts/{id}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, decimal amount)
        {
            // retrieve customer id
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve account
            var accountResponse = await Client.GetAsync($"api/accounts/{id}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);

            //  retrieve customer object
            var customerResponse = await Client.GetAsync($"api/customers/{customerid}");
            if (!customerResponse.IsSuccessStatusCode)
                throw new Exception();
            var customerResult = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(customerResult);

            // retrieve transactions
            var transactionsResponse = await Client.GetAsync($"api/transactions");
            if (!transactionsResponse.IsSuccessStatusCode)
                throw new Exception();
            var transactionsResult = await transactionsResponse.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsResult);

            //  check transaction count for fees
            var transactionCount = 0;
            foreach (var transaction in transactions.ToList())
            {
                if (transaction.AccountNumber == account.AccountNumber)
                {
                    if (transaction.TransactionType == TransactionType.Transfer || transaction.TransactionType == TransactionType.Withdraw)
                    {
                        transactionCount++;
                    }
                }
            }
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

            //  update account balance
            account.Balance -= (amount + fee);
            var content = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            var response = Client.PutAsync("api/accounts", content).Result;
            if (response.IsSuccessStatusCode)
            {
                //  add transaction to database
                Transaction transaction =
                new Transaction
                {
                    TransactionType = TransactionType.Withdraw,
                    AccountNumber = account.AccountNumber,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                };

                var tcontent = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
                var tresponse = Client.PostAsync("api/transactions", tcontent).Result;

                if (transactionCount > 4)
                {
                    Transaction feeTransaction =
                    new Transaction
                    {
                        TransactionType = TransactionType.ServiceCharge,
                        AccountNumber = account.AccountNumber,
                        Amount = fee,
                        TransactionTimeUtc = DateTime.Now
                    };

                    var ftcontent = new StringContent(JsonConvert.SerializeObject(feeTransaction), Encoding.UTF8, "application/json");
                    var ftresponse = Client.PostAsync("api/transactions", ftcontent).Result;
                }

                if (tresponse.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(account);
        }

        public async Task<IActionResult> Transfer(int id)
        {
            //  find current user's CustomerID
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve accounts
            var accountsResponse = await Client.GetAsync($"api/accounts");
            if (!accountsResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

            //  copy account ids of accounts other than the users' to a list 
            var accList = new List<int>();
            foreach (var item in accounts.ToList())
            {
                if (item.CustomerID != customerid)
                {
                    accList.Add(item.AccountNumber);
                }
            }

            ViewBag.AccountList = new SelectList(accList, "AccountNumber");

            //  retrieve account
            var accountResponse = await Client.GetAsync($"api/accounts/{id}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, int AccountNumber, decimal amount)
        {
            //  find current user's CustomerID
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve customer object
            var customerResponse = await Client.GetAsync($"api/customers/{customerid}");
            if (!customerResponse.IsSuccessStatusCode)
                throw new Exception();
            var customerResult = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(customerResult);

            //  retrieve accounts
            var accountsResponse = await Client.GetAsync($"api/accounts");
            if (!accountsResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

            var payerAccount = new Account();
            var payeeAccount = new Account();

            //  remove unnecessary accounts 
            foreach (var account in accounts.ToList())
            {
                if (account.AccountNumber == id)
                {
                    payerAccount = account;
                }
                if (account.AccountNumber == AccountNumber)
                {
                    payeeAccount = account;
                }

            }

            // retrieve transactions
            var transactionsResponse = await Client.GetAsync($"api/transactions");
            if (!transactionsResponse.IsSuccessStatusCode)
                throw new Exception();
            var transactionsResult = await transactionsResponse.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsResult);

            //  check transaction count for fees
            var transactionCount = 0;
            foreach (var transaction in transactions.ToList())
            {
                if (transaction.AccountNumber == payerAccount.AccountNumber)
                {
                    if (transaction.TransactionType == TransactionType.Transfer || transaction.TransactionType == TransactionType.Withdraw)
                    {
                        transactionCount++;
                    }
                }
            }
            decimal fee = 0;
            int chAmount = 0;

            if (transactionCount > 4)
                fee = 0.2m;
            if (payerAccount.AccountType == AccountType.Checking)
                chAmount = 200;
            if (amount <= 0)
                ModelState.AddModelError(nameof(amount), "Amount must be positive.");
            if (amount.HasMoreThanTwoDecimalPlaces())
                ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
            if ((amount + fee + chAmount) > payerAccount.Balance)
                ModelState.AddModelError(nameof(amount), "Insufficient Funds.");
            if (payeeAccount == null)
                ModelState.AddModelError(nameof(AccountNumber), "Invalid Account ID.");
            if (!ModelState.IsValid)
            {
                ViewBag.AccountNumber = AccountNumber;
                ViewBag.Amount = amount;
                return View(payerAccount);
            }

            //  update account balance
            payerAccount.Balance -= (amount + fee);
            var content = new StringContent(JsonConvert.SerializeObject(payerAccount), Encoding.UTF8, "application/json");
            var response = Client.PutAsync("api/accounts", content).Result;
            payeeAccount.Balance += (amount + fee);
            var payeeContent = new StringContent(JsonConvert.SerializeObject(payeeAccount), Encoding.UTF8, "application/json");
            var payeeResponse = Client.PutAsync("api/accounts", payeeContent).Result;

            if (response.IsSuccessStatusCode)
            {
                //  add transaction to database
                Transaction transaction =
                new Transaction
                {
                    TransactionType = TransactionType.Transfer,
                    AccountNumber = payerAccount.AccountNumber,
                    DestinationAccountNumber = payeeAccount.AccountNumber,
                    Amount = amount,
                    TransactionTimeUtc = DateTime.Now
                };

                var tcontent = new StringContent(JsonConvert.SerializeObject(transaction), Encoding.UTF8, "application/json");
                var tresponse = Client.PostAsync("api/transactions", tcontent).Result;

                if (transactionCount > 4)
                {
                    Transaction feeTransaction =
                    new Transaction
                    {
                        TransactionType = TransactionType.ServiceCharge,
                        AccountNumber = payerAccount.AccountNumber,
                        Amount = fee,
                        TransactionTimeUtc = DateTime.Now
                    };

                    var ftcontent = new StringContent(JsonConvert.SerializeObject(feeTransaction), Encoding.UTF8, "application/json");
                    var ftresponse = Client.PostAsync("api/transactions", ftcontent).Result;
                }
                if (tresponse.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(payerAccount);
        }

        public async Task<IActionResult> Statements()
        {
            //  find current user's CustomerID
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve accounts
            var accountsResponse = await Client.GetAsync($"api/accounts");
            if (!accountsResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

            //  copy account ids of accounts other than the users' to a list 
            var accList = new List<int>();
            foreach (var item in accounts.ToList())
            {
                if (item.CustomerID == customerid)
                {
                    accList.Add(item.AccountNumber);
                }
            }

            ViewBag.AccList = new SelectList(accList, "AccountNumber");
            return View();
        }


        public async Task<IActionResult> AccountStatement(int MyAccountNumber, int? page = 1)
        {
            //  find current user's CustomerID
            var name = User.Identity.Name;
            var applicationUser = await _userManager.FindByNameAsync(name);
            var customerid = applicationUser.CustomerID;

            //  retrieve customer object
            var customerResponse = await Client.GetAsync($"api/customers/{customerid}");
            if (!customerResponse.IsSuccessStatusCode)
                throw new Exception();
            var customerResult = await customerResponse.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(customerResult);
            ViewBag.Customer = customer;

            //  retrieve account
            var accountResponse = await Client.GetAsync($"api/accounts/{MyAccountNumber}");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var accountResult = await accountResponse.Content.ReadAsStringAsync();
            var account = JsonConvert.DeserializeObject<Account>(accountResult);
            ViewBag.Account = account;

            //  retrieve transactions
            var transactionsResponse = await Client.GetAsync($"api/transactions");
            if (!accountResponse.IsSuccessStatusCode)
                throw new Exception();
            var transactionsResult = await transactionsResponse.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsResult);

            //  remove irrelevant transactions
            foreach (var transaction in transactions.ToList())
            {
                if (transaction.AccountNumber != MyAccountNumber)
                {
                    transactions.Remove(transaction);
                }
            }

            int pageSize = 4;
            var transactionListPaged = await transactions.ToPagedListAsync((int)page, pageSize);

            return View(transactionListPaged);
        }

        //public async Task<IActionResult> BillPay()
        //{
        //    var payeeList = await _context.Payees.Select(x => x.PayeeID).ToListAsync();
        //    ViewBag.PayeeList = new SelectList(payeeList, "AccountNumber");
        //    var accList = await _context.Accounts.Where(x => x.CustomerID == CustomerID).Select(x => x.AccountNumber).ToListAsync();
        //    ViewBag.AccList = new SelectList(accList, "AccountNumber");
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> BillPay(int MyAccountNumber, int PayeeAccountNumber, decimal amount, DateTime PickedDate, Period Period)
        //{
        //    var account = await _context.Accounts.FindAsync(MyAccountNumber);
        //    var payeeAccount = await _context.Payees.FindAsync(PayeeAccountNumber);
        //    int chAmount = 0;

        //    if (account.AccountType == AccountType.Checking)
        //        chAmount = 200;
        //    if (amount <= 0)
        //        ModelState.AddModelError(nameof(amount), "Amount must be positive.");
        //    if (amount.HasMoreThanTwoDecimalPlaces())
        //        ModelState.AddModelError(nameof(amount), "Amount cannot have more than 2 decimal places.");
        //    if (amount > (account.Balance + chAmount))
        //        ModelState.AddModelError(nameof(amount), "Insufficeint Funds.");
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Amount = amount;
        //        return View(account);
        //    }

        //    account.BillPays.Add(
        //        new BillPay
        //        {
        //            PayeeID = PayeeAccountNumber,
        //            Amount = amount,
        //            Status = Status.Pending,
        //            ScheduleDate = PickedDate,
        //            Period = Period,
        //            ModifyDate = DateTime.Now
        //        });

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}

        //public async Task<IActionResult> ScheduledPayments(int? page = 1, Status? filter = null)
        //{
        //    var customer = await _context.Customers.FindAsync(CustomerID);
        //    ViewBag.Customer = customer;

        //    // all billpays
        //    var query = from b in _context.BillPays
        //                join a in _context.Accounts
        //                    on b.AccountNumber equals a.AccountNumber
        //                join p in _context.Payees
        //                    on b.PayeeID equals p.PayeeID
        //                where (a.CustomerID == CustomerID)
        //                orderby b.ScheduleDate descending
        //                select new ScheduledPaymentsViewModel
        //                {
        //                    BillPayID = b.BillPayID,
        //                    PayeeName = p.PayeeName,
        //                    Amount = b.Amount,
        //                    Status = b.Status,
        //                    ScheduleDate = b.ScheduleDate,
        //                    Period = b.Period
        //                };

        //    switch (filter)
        //    {
        //        case Status.Pending:
        //            query = query.Where(b => b.Status == Status.Pending);
        //            ViewBag.TableFilter = Status.Pending;
        //            break;
        //        case Status.Complete:
        //            query = query.Where(b => b.Status == Status.Complete);
        //            ViewBag.TableFilter = Status.Complete;
        //            break;
        //        case Status.Failed:
        //            query = query.Where(b => b.Status == Status.Failed);
        //            ViewBag.TableFilter = Status.Failed;
        //            break;
        //        default:
        //            break;
        //    }

        //    int pageSize = 4;
        //    var billPayListPaged = await query.ToPagedListAsync(page, pageSize);

        //    return View(billPayListPaged);
        //}

        //public async Task<IActionResult> ModifyBillPay(int id) => View(await _context.BillPays.FindAsync(id));

        //[HttpPost]
        //public async Task<IActionResult> ModifyBillPay(int BillPayID, int AccountNumber, decimal Amount, DateTime ScheduleDate, Period Period)
        //{
        //    var billpay = await _context.BillPays.FindAsync(BillPayID);
        //    var account = await _context.Accounts.FindAsync(AccountNumber);
        //    int chAmount = 0;

        //    if (account.AccountType == AccountType.Checking)
        //        chAmount = 200;
        //    if (Amount <= 0)
        //        ModelState.AddModelError(nameof(Amount), "Amount must be positive.");
        //    if (Amount.HasMoreThanTwoDecimalPlaces())
        //        ModelState.AddModelError(nameof(Amount), "Amount cannot have more than 2 decimal places.");
        //    if (Amount > (account.Balance + chAmount))
        //        ModelState.AddModelError(nameof(Amount), "Insufficient Funds.");
        //    if (DateTime.Compare(DateTime.Now, ScheduleDate) > 0)
        //        ModelState.AddModelError(nameof(ScheduleDate), "Select a time in the future.");
        //    if (!ModelState.IsValid)
        //    {
        //        ViewBag.Amount = Amount;
        //        ViewBag.ScheduleDate = ScheduleDate;
        //        return View(billpay);
        //    }

        //    billpay.Amount = Amount;
        //    billpay.ScheduleDate = ScheduleDate;
        //    billpay.Period = Period;
        //    billpay.ModifyDate = DateTime.Now;

        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}


        //public async Task<IActionResult> DeleteBillPay(int id) => View(await _context.BillPays.FindAsync(id));


        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int BillPayID)
        //{
        //    var billpay = await _context.BillPays.FindAsync(BillPayID);
        //    _context.BillPays.Remove(billpay);
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(ScheduledPayments));
        //}
    }
}
