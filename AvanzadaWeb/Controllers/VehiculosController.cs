using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly IApiService _apiService;

        public VehiculosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _apiService.GetAsync<List<VehiculoViewModel>>("vehiculos");
            return View(vehiculos);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(VehiculoViewModel vehiculo)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PostAsync<VehiculoViewModel>("vehiculos", vehiculo);
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vehiculo = await _apiService.GetAsync<VehiculoViewModel>($"vehiculos/{id}");
            return View(vehiculo);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, VehiculoViewModel vehiculo)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PutAsync<VehiculoViewModel>($"vehiculos/{id}", vehiculo);
                return RedirectToAction(nameof(Index));
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteAsync($"vehiculos/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}