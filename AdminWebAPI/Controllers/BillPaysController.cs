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
    public class BillPaysController : ControllerBase
    {
        private readonly BillPayManager _repo;

        public BillPaysController(BillPayManager repo)
        {
            _repo = repo;
        }

        // GET: /BillPays
        [HttpGet]
        public IEnumerable<BillPay> Get()
        {
            return _repo.GetAll();
        }

        //GET: /BillPays/1
        [HttpGet("{id}")]
        public BillPay Get(int id)
        {
            return _repo.Get(id);
        }

        // POST /BillPays
        [HttpPost]
        public void Post([FromBody] BillPay billpay)
        {
            _repo.Add(billpay);
        }

        // PUT /BillPays
        [HttpPut]
        public void Put([FromBody] BillPay billpay)
        {
            _repo.Update(billpay.BillPayID, billpay);
        }

        // DELETE /BillPays/1
        [HttpDelete("{id}")]
        public long Delete(int id)
        {
            return _repo.Delete(id);
        }
    }
}
