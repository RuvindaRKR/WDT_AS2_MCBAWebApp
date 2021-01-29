using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminWebAPI.Models;
using AdminWebAPI.Models.DataManager;

namespace AdminWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly CustomerManager _repo;

        public AdminController(CustomerManager repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return _repo.GetAll();
        }

        [HttpGet("{id}")]
        public Customer Get(int id)
        {
            return _repo.Get(id);
        }
    }
}
