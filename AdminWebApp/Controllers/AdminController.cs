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
    }
}
