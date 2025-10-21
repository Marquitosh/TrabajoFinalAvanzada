using AvanzadaWeb.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AuthorizeRole("Cliente", "Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
