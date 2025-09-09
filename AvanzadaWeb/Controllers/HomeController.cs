using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
