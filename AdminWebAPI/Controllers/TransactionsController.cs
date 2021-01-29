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
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionManager _repo;

        public TransactionsController(TransactionManager repo)
        {
            _repo = repo;
        }

        // GET: /Transactions
        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            return _repo.GetAll();
        }

        //GET: /Transactions/1
        [HttpGet("{id}")]
        public Transaction Get(int id)
        {
            return _repo.Get(id);
        }

        // POST /Transactions
        [HttpPost]
        public void Post([FromBody] Transaction transaction)
        {
            _repo.Add(transaction);
        }

        // PUT /Transactions
        [HttpPut]
        public void Put([FromBody] Transaction transaction)
        {
            _repo.Update(transaction.TransactionID, transaction);
        }

        // DELETE /Transactions/1
        [HttpDelete("{id}")]
        public long Delete(int id)
        {
            return _repo.Delete(id);
        }
    }
}
