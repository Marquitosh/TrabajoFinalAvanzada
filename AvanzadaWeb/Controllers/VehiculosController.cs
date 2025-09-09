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

        // MÉTODO ORIGINAL RESTAURADO PARA USAR LA BASE DE DATOS
        public async Task<IActionResult> Index()
        {
            try
            {
                var vehiculos = await _apiService.GetAsync<List<VehiculoViewModel>>("vehiculos");
                return View("MyVehicles", vehiculos);
            }
            catch (Exception ex)
            {
                // Si hay error con la API, mostrar vista vacía
                ViewBag.Error = "Error al cargar los vehículos: " + ex.Message;
                return View("MyVehicles", new List<VehiculoViewModel>());
            }
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
                try
                {
                    await _apiService.PostAsync<VehiculoViewModel>("vehiculos", vehiculo);
                    TempData["Success"] = "Vehículo registrado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al registrar el vehículo: " + ex.Message);
                }
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var vehiculo = await _apiService.GetAsync<VehiculoViewModel>($"vehiculos/{id}");
                return View(vehiculo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el vehículo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, VehiculoViewModel vehiculo)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PutAsync<VehiculoViewModel>($"vehiculos/{id}", vehiculo);
                    TempData["Success"] = "Vehículo actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el vehículo: " + ex.Message);
                }
            }
            return View(vehiculo);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"vehiculos/{id}");
                TempData["Success"] = "Vehículo eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el vehículo: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}