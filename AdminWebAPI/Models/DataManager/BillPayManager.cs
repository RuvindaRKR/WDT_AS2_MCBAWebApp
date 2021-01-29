using System.Collections.Generic;
using System.Linq;
using AdminWebAPI.Data;
using AdminWebAPI.Models.Repository;

namespace AdminWebAPI.Models.DataManager
{
    public class BillPayManager : IDataRepository<BillPay, int>
    {
        private readonly McbaContext _context;

        public BillPayManager(McbaContext context)
        {
            _context = context;
        }

        public BillPay Get(int id)
        {
            return _context.BillPays.Find(id);
        }

        public IEnumerable<BillPay> GetAll()
        {
            return _context.BillPays.ToList();
        }

        public int Add(BillPay billpay)
        {
            _context.BillPays.Add(billpay);
            _context.SaveChanges();

            return billpay.BillPayID;
        }

        public int Delete(int id)
        {
            _context.BillPays.Remove(_context.BillPays.Find(id));
            _context.SaveChanges();

            return id;
        }

        public int Update(int id, BillPay billpay)
        {
            _context.Update(billpay);
            _context.SaveChanges();
            
            return id;
        }
    }
}
