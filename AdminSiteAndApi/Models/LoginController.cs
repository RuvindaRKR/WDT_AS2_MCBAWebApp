using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WDT_AS2.Data;
using WDT_AS2.Models;
using SimpleHashing;

namespace WDT_AS2.Models
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
            if(login == null || !PBKDF2.Verify(login.PasswordHash, password))
            { 
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
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

        [HttpGet("admin")]
        public IActionResult Admin() => View();

        [HttpGet("admin")]
        public async Task<IActionResult> Admin(string loginID, string password)
        {
            if (loginID != "admin" || password != "admin")
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View(new Login { LoginID = loginID });
            }

            // Login administrator.
            HttpContext.Session.SetInt32("AdminID", 1);

            return RedirectToAction("Index", "Admin");
        }
    }
}
