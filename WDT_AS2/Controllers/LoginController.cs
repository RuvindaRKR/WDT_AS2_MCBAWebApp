using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WDT_AS2.Data;
using WDT_AS2.Models;
using SimpleHashing;
using WDT_AS2.Utilities;

namespace WDT_AS2.Controllers
{
    [Route("/Mcba/SecureLogin")]
    public class LoginController : Controller
    {
        private readonly McbaContext _context;

        public LoginController(McbaContext context) => _context = context;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string loginID, string password)
        {
            var login = await _context.Logins.FindAsync(loginID);
            var customer = await _context.Customers.FindAsync(login.CustomerID);

            if(login == null || !PBKDF2.Verify(login.PasswordHash, password))
            { 
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View(new Login { LoginID = loginID });
            }
            if(customer.AccountStatus == AccountStatus.Locked)
            {
                ModelState.AddModelError("LoginFailed", "Login failed, your account is locked.");
                return View(new Login { LoginID = loginID });
            }

            // Login customer.
            HttpContext.Session.SetInt32(nameof(Customer.CustomerID), login.CustomerID);
            HttpContext.Session.SetString(nameof(Customer.CustomerName), login.Customer.CustomerName);

            return RedirectToAction("Index", "Customer");
        }

        [Route("LogoutNow")]
        public IActionResult Logout()
        {
            // Logout customer.
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
