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
        //    //  find current user's CustomerID
        //    var name = User.Identity.Name;
        //    var applicationUser = await _userManager.FindByNameAsync(name);
        //    var customerid = applicationUser.CustomerID;

        //    //  retrieve payees
        //    var payeesResponse = await Client.GetAsync($"api/payees");
        //    if (!payeesResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var payeesResult = await payeesResponse.Content.ReadAsStringAsync();
        //    var payees = JsonConvert.DeserializeObject<List<Payee>>(payeesResult);

        //    //  copy account ids of accounts other than the users' to a list 
        //    var payeeList = new List<int>();
        //    foreach (var item in payees.ToList())
        //    {
        //        payeeList.Add(item.PayeeID);
        //    }
        //    ViewBag.PayeeList = new SelectList(payeeList, "PayeeID");

        //    //  retrieve accounts
        //    var accountsResponse = await Client.GetAsync($"api/accounts");
        //    if (!accountsResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
        //    var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

        //    //  add users accounts' account numbers to list 
        //    var accList = new List<int>();
        //    foreach (var account in accounts.ToList())
        //    {
        //        if (account.CustomerID == customerid)
        //        {
        //            accList.Add(account.AccountNumber);
        //        }
        //    }
        //    ViewBag.AccList = new SelectList(accList, "AccountNumber");
            
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> BillPay(int MyAccountNumber, int PayeeAccountNumber, decimal amount, DateTime PickedDate, Period Period)
        //{
        //    //  retrieve customers account
        //    var accountResponse = await Client.GetAsync($"api/accounts/{MyAccountNumber}");
        //    if (!accountResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var accountResult = await accountResponse.Content.ReadAsStringAsync();
        //    var account = JsonConvert.DeserializeObject<Account>(accountResult);

        //    //  retrieve payees account
        //    var payeeResponse = await Client.GetAsync($"api/payees/{PayeeAccountNumber}");
        //    if (!payeeResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var payeeResult = await payeeResponse.Content.ReadAsStringAsync();
        //    var payee = JsonConvert.DeserializeObject<Payee>(payeeResult);

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

        //    //  add billpay to database
        //    BillPay billpay =
        //    new BillPay
        //    {
        //        AccountNumber = account.AccountNumber,
        //        PayeeID = PayeeAccountNumber,
        //        Amount = amount,
        //        Status = Status.Pending,
        //        ScheduleDate = PickedDate,
        //        Period = Period,
        //        ModifyDate = DateTime.Now
        //    };

        //    var content = new StringContent(JsonConvert.SerializeObject(billpay), Encoding.UTF8, "application/json");
        //    var response = Client.PostAsync("api/billpays", content).Result;

        //    if (response.IsSuccessStatusCode)
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(account);
        //}

        //public async Task<IActionResult> ScheduledPayments(int? page = 1, Status? filter = null)
        //{
        //    //  find current user's CustomerID
        //    var name = User.Identity.Name;
        //    var applicationUser = await _userManager.FindByNameAsync(name);
        //    var customerid = applicationUser.CustomerID;

        //    //  retrieve customer object
        //    var customerResponse = await Client.GetAsync($"api/customers/{customerid}");
        //    if (!customerResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var customerResult = await customerResponse.Content.ReadAsStringAsync();
        //    var customer = JsonConvert.DeserializeObject<Customer>(customerResult);
        //    ViewBag.Customer = customer;

        //    //  retrieve accounts
        //    var accountsResponse = await Client.GetAsync($"api/accounts");
        //    if (!accountsResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
        //    var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);

        //    //  add users accounts' account numbers to list 
        //    var accList = new List<int>();
        //    foreach (var account in accounts.ToList())
        //    {
        //        if (account.CustomerID == customerid)
        //        {
        //            accList.Add(account.AccountNumber);
        //        }
        //    }

        //    //  retrieve billpays
        //    var billpaysResponse = await Client.GetAsync($"api/accounts");
        //    if (!billpaysResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var billpaysResult = await billpaysResponse.Content.ReadAsStringAsync();
        //    var billpays = JsonConvert.DeserializeObject<List<BillPay>>(billpaysResult);

        //    //  retrieve payees
        //    var payeesResponse = await Client.GetAsync($"api/payees");
        //    if (!payeesResponse.IsSuccessStatusCode)
        //        throw new Exception();
        //    var payeesResult = await payeesResponse.Content.ReadAsStringAsync();
        //    var payees = JsonConvert.DeserializeObject<List<Payee>>(payeesResult);

        //    var scheduledPayments = new List<ScheduledPaymentsViewModel>();
        //    foreach (var b in billpays)
        //    {
        //        foreach (var p in payees)
        //        {
        //            if (accList.Exists(x => x == b.AccountNumber) && b.PayeeID == p.PayeeID)
        //            {
        //                scheduledPayments.Add(
        //                    new ScheduledPaymentsViewModel
        //                    {
        //                        BillPayID = b.BillPayID,
        //                        PayeeName = p.PayeeName,
        //                        Amount = b.Amount,
        //                        Status = b.Status,
        //                        ScheduleDate = b.ScheduleDate,
        //                        Period = b.Period
        //                    });
        //            }
        //        }
        //    }

        //    switch (filter)
        //    {
        //        case Status.Pending:
        //            foreach (var scheduledPayment in scheduledPayments.ToList())
        //            {
        //                if (scheduledPayment.Status != Status.Pending)
        //                {
        //                    scheduledPayments.Remove(scheduledPayment);
        //                }
        //            }
        //            ViewBag.TableFilter = Status.Pending;
        //            break;
        //        case Status.Complete:
        //            foreach (var scheduledPayment in scheduledPayments.ToList())
        //            {
        //                if (scheduledPayment.Status != Status.Complete)
        //                {
        //                    scheduledPayments.Remove(scheduledPayment);
        //                }
        //            }
        //            ViewBag.TableFilter = Status.Complete;
        //            break;
        //        case Status.Failed:
        //            foreach (var scheduledPayment in scheduledPayments.ToList())
        //            {
        //                if (scheduledPayment.Status != Status.Failed)
        //                {
        //                    scheduledPayments.Remove(scheduledPayment);
        //                }
        //            }
        //            ViewBag.TableFilter = Status.Failed;
        //            break;
        //        default:
        //            break;
        //    }

        //    int pageSize = 4;
        //    var billPayListPaged = await scheduledPayments.ToPagedListAsync((int)page, pageSize);

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
