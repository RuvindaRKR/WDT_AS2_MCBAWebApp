using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminWebApp.Models;

namespace AdminWebApp.Controllers
{
    [Route("/Mcba/SecureLogin")]
    public class LoginController : Controller
    {
        public IActionResult Login(string loginID, string password)
        {
            if (string.IsNullOrEmpty(loginID))
            {
                return View();
            }

            if (loginID != "admin" || password != "admin")
            {
                ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
                return View(new Login { LoginID = loginID });
            }

            // Login administrator.
            HttpContext.Session.SetInt32("AdminID", 1);

            return RedirectToAction("Index", "Admin");
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
