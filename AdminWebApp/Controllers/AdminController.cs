using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdminWebApp.Models;
using Newtonsoft.Json;
using X.PagedList;


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

        public async Task<IActionResult> CustomerTransactions(int id, int? page = 1)
        {
            // get customer details and put in viewbag
            var response = await Client.GetAsync($"api/customers/{id}");
            if (!response.IsSuccessStatusCode)
                throw new Exception();
            var result = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<Customer>(result);
            ViewBag.Customer = customer;

            // make request to api for accounts of the specified customer
            var response2 = await Client.GetAsync($"api/accounts");
            if (!response2.IsSuccessStatusCode)
                throw new Exception();
            var result2 = await response2.Content.ReadAsStringAsync();
            var accounts = JsonConvert.DeserializeObject<List<Account>>(result2);

            // for each account, take each transaction and add it to the transactions list
            List<Transaction> transactions = new();

            foreach (var account in accounts)
            {
                if(account.CustomerID == id)
                {
                    var response3 = await Client.GetAsync($"api/transactions");
                    if (!response3.IsSuccessStatusCode)
                        throw new Exception();
                    var result3 = await response3.Content.ReadAsStringAsync();
                    var transactionsForAccount = JsonConvert.DeserializeObject<List<Transaction>>(result3);
                    foreach (var entry in transactionsForAccount)
                    {
                        if(entry.AccountNumber == account.AccountNumber)
                            transactions.Add(entry);
                    }
                }
                
            }
            // sort the transactions list so that they are ordered by transaction time
            List<Transaction> sortedTransactions = transactions.OrderBy(t => t.TransactionTimeUtc).ToList();

            int pageSize = 4;
            var transactionsListPaged = await sortedTransactions.ToPagedListAsync((int)page, pageSize);

            return View(transactionsListPaged);
        }
    }
}
