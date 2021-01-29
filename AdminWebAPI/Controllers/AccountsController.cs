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
    public class AccountsController : ControllerBase
    {
        private readonly AccountManager _repo;

        public AccountsController(AccountManager repo)
        {
            _repo = repo;
        }

        // GET: /Accounts
        [HttpGet]
        public IEnumerable<Account> Get()
        {
            return _repo.GetAll();
        }

        //GET: /Accounts/1
        [HttpGet("{id}")]
        public Account Get(int id)
        {
            return _repo.Get(id);
        }

        // POST /Accounts
        [HttpPost]
        public void Post([FromBody] Account account)
        {
            _repo.Add(account);
        }

        // PUT /Accounts
        [HttpPut]
        public void Put([FromBody] Account account)
        {
            _repo.Update(account.AccountNumber, account);
        }

        // DELETE /Accounts/1
        [HttpDelete("{id}")]
        public long Delete(int id)
        {
            return _repo.Delete(id);
        }
    }
}
