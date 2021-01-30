using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminWebApp.Models;
using Newtonsoft.Json;
using X.PagedList;
using AdminWebApp.Utilities;

namespace AdminWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private HttpClient Client => _clientFactory.CreateClient("api");

        public AdminController(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

        public async Task<IActionResult> Index(int? page = 1)
        {
            // make request to api
            var response = await Client.GetAsync("api/customers");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            // Storing the response details received from web api.
            var result = await response.Content.ReadAsStringAsync();

            // Deserializing the response received from web api and storing into a list.
            var customers = JsonConvert.DeserializeObject<List<Customer>>(result);

            int pageSize = 4;
            var customerListPaged = await customers.ToPagedListAsync((int)page, pageSize);

            return View(customerListPaged);
        }

        public async Task<IActionResult> Transactions(int? id, int? page = 1, DateTime? d1 = null, DateTime? d2 = null, string? SearchString = null)
        {
            // Start by gathering info needed to populate customer filter ############### CURRENTLY UNUSED
            var customersResponse = await Client.GetAsync($"api/customers");
            if (!customersResponse.IsSuccessStatusCode)
                throw new Exception();
            var customersResult = await customersResponse.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<Customer>>(customersResult);
            ViewBag.Customers = customers;

            // Here we start gathering info we need to generate tables
            var transactionsResponse = await Client.GetAsync($"api/transactions");
            if (!transactionsResponse.IsSuccessStatusCode)
                throw new Exception();
            var transactionsResult = await transactionsResponse.Content.ReadAsStringAsync();
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsResult);
            List<Transaction> sortedTransactions = transactions.OrderByDescending(t => t.TransactionTimeUtc).ToList();
            // at this point we have all transactions sorted in sortedTransactions

            // check search string
            if (!int.TryParse(SearchString, out var searchID))
            {
                ModelState.AddModelError(nameof(SearchString), "Invalid Input");
                ViewBag.SearchString = SearchString;
            }

            // check customer filter ############### CURRENTLY UNUSED
            List<int> accountNumbers = new();
            if (id.HasValue)
            {
                var accountsResponse = await Client.GetAsync($"api/accounts/customer/{id}");
                if (!accountsResponse.IsSuccessStatusCode)
                    throw new Exception();
                var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);
                foreach (var account in accounts)
                {
                    accountNumbers.Add(account.AccountNumber);
                }
            }

            // now we remove unneccessary transactions from list
            foreach (var transaction in sortedTransactions.ToList())
            {
                // this removes transactions that dont match search string
                if (!String.IsNullOrEmpty(SearchString))
                {
                    if (transaction.TransactionID != searchID)
                        sortedTransactions.Remove(transaction);
                }

                // this removes transactions that dont meet the date filters
                if (d1.HasValue && d2.HasValue)
                {
                    if (DateTime.Compare((DateTime)d1, transaction.TransactionTimeUtc) > 0 && DateTime.Compare((DateTime)d2, transaction.TransactionTimeUtc) < 0)
                        sortedTransactions.Remove(transaction);
                }
                else if (d1.HasValue)
                {
                    if (DateTime.Compare((DateTime)d1, transaction.TransactionTimeUtc) > 0)
                        sortedTransactions.Remove(transaction);
                }
                else if (d2.HasValue)
                {
                    if (DateTime.Compare((DateTime)d2, transaction.TransactionTimeUtc) < 0)
                        sortedTransactions.Remove(transaction);
                }

                // filter based on customer ############### CURRENTLY UNUSED
                if (id.HasValue)
                {
                    if (!accountNumbers.Exists(x => x == transaction.AccountNumber))
                    {
                        sortedTransactions.Remove(transaction);
                    }
                }
            }

            // finally page it
            int pageSize = 10;
            var transactionsListPaged = await sortedTransactions.ToPagedListAsync((int)page, pageSize);

            return View(transactionsListPaged);
        }

        public async Task<IActionResult> BillPays(int? id, int? page = 1, DateTime? d1 = null, DateTime? d2 = null, string? SearchString = null)
        {
            // Start by gathering info needed to populate customer filter ############### CURRENTLY UNUSED
            var customersResponse = await Client.GetAsync($"api/customers");
            if (!customersResponse.IsSuccessStatusCode)
                throw new Exception();
            var customersResult = await customersResponse.Content.ReadAsStringAsync();
            var customers = JsonConvert.DeserializeObject<List<Customer>>(customersResult);
            ViewBag.Customers = customers;

            // Here we start gathering info we need to generate tables
            var billpaysResponse = await Client.GetAsync($"api/billpays");
            if (!billpaysResponse.IsSuccessStatusCode)
                throw new Exception();
            var billpaysResult = await billpaysResponse.Content.ReadAsStringAsync();
            var billpays = JsonConvert.DeserializeObject<List<BillPay>>(billpaysResult);
            List<BillPay> sortedBillpays = billpays.OrderByDescending(t => t.ScheduleDate).ToList();
            // at this point we have all billpays sorted in sortedBillpays

            // check search string
            if (!int.TryParse(SearchString, out var searchID))
            {
                ModelState.AddModelError(nameof(SearchString), "Invalid Input");
                ViewBag.SearchString = SearchString;
            }

            // check customer filter ############### CURRENTLY UNUSED
            List<int> accountNumbers = new();
            if (id.HasValue)
            {
                var accountsResponse = await Client.GetAsync($"api/accounts/customer/{id}");
                if (!accountsResponse.IsSuccessStatusCode)
                    throw new Exception();
                var accountsResult = await accountsResponse.Content.ReadAsStringAsync();
                var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResult);
                foreach (var account in accounts)
                {
                    accountNumbers.Add(account.AccountNumber);
                }
            }

            // now we remove unneccessary transactions from list
            foreach (var billpay in sortedBillpays.ToList())
            {
                // this removes billpays that dont match search string
                if (!String.IsNullOrEmpty(SearchString))
                {
                    if (billpay.BillPayID != searchID)
                        sortedBillpays.Remove(billpay);
                }

                // this removes billpays that dont meet the date filters
                if (d1.HasValue && d2.HasValue)
                {
                    if (DateTime.Compare((DateTime)d1, billpay.ScheduleDate) > 0 && DateTime.Compare((DateTime)d2, billpay.ScheduleDate) < 0)
                        sortedBillpays.Remove(billpay);
                }
                else if (d1.HasValue)
                {
                    if (DateTime.Compare((DateTime)d1, billpay.ScheduleDate) > 0)
                        sortedBillpays.Remove(billpay);
                }
                else if (d2.HasValue)
                {
                    if (DateTime.Compare((DateTime)d2, billpay.ScheduleDate) < 0)
                        sortedBillpays.Remove(billpay);
                }

                // filter based on customer ############### CURRENTLY UNUSED
                if (id.HasValue)
                {
                    if (!accountNumbers.Exists(x => x == billpay.AccountNumber))
                    {
                        sortedBillpays.Remove(billpay);
                    }
                }
            }

            // finally page it
            int pageSize = 10;
            var billpaysListPaged = await sortedBillpays.ToPagedListAsync((int)page, pageSize);

            return View(billpaysListPaged);
        }
    }
}
