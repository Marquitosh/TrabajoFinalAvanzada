using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly IApiService _apiService;

        public ServiciosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var servicios = await _apiService.GetAsync<List<ServicioViewModel>>("servicios");
                return View(servicios);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los servicios: " + ex.Message;
                return View(new List<ServicioViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServicioViewModel servicio)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PostAsync<ServicioViewModel>("servicios", servicio);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el servicio: " + ex.Message);
                }
            }
            return View(servicio);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var servicio = await _apiService.GetAsync<ServicioViewModel>($"servicios/{id}");
                if (servicio == null)
                {
                    return NotFound();
                }
                return View(servicio);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar el servicio: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServicioViewModel servicio)
        {
            if (id != servicio.IDServicio)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PutAsync<ServicioViewModel>($"servicios/{id}", servicio);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el servicio: " + ex.Message);
                }
            }
            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"servicios/{id}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al eliminar el servicio: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}