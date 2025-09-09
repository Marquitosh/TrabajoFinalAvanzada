using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class RolesController : Controller
    {
        // Esta acción mostrará la vista Index.cshtml
        public IActionResult Index()
        {
            return View();
        }

        // Esta acción mostrará la vista Create.cshtml
        public IActionResult Create()
        {
            return View();
        }

        // Esta acción mostrará la vista Edit.cshtml
        public IActionResult Edit()
        {
            return View();
        }
    }
}