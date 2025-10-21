using AvanzadaAPI.Models;
using AvanzadaWeb.Models;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehiculoViewModel model)
        {
            try
            {
                Console.WriteLine("=== INICIO Create Vehiculo ===");

                // REMOVER campos que no se envían en el formulario
                ModelState.Remove("UsuarioNombre");
                ModelState.Remove("CombustibleDescripcion");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState no es válido:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($" - {error.ErrorMessage}");
                    }
                    return View(model);
                }

                // Obtener el ID del usuario desde la sesión
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                {
                    return RedirectToAction("Login", "Account");
                }

                var sessionUser = System.Text.Json.JsonSerializer.Deserialize<SessionUser>(userJson);

                // Crear el objeto para enviar a la API
                var vehiculoData = new
                {
                    IDUsuario = sessionUser.IDUsuario,
                    Marca = model.Marca,
                    Modelo = model.Modelo,
                    Year = model.Year,
                    Patente = model.Patente?.ToUpper(),
                    IDCombustible = model.IDCombustible,
                    Observaciones = model.Observaciones
                };

                Console.WriteLine($"Enviando vehículo a API: {System.Text.Json.JsonSerializer.Serialize(vehiculoData)}");

                // Llamar a la API
                var response = await _apiService.PostAsync<Vehiculo>("vehiculos", vehiculoData);

                TempData["SuccessMessage"] = "Vehículo registrado correctamente";
                return RedirectToAction("MyVehicles", "Usuarios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en Create Vehiculo: {ex.Message}");
                TempData["ErrorMessage"] = "Error al registrar el vehículo: " + ex.Message;
                return View(model);
            }
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
            return RedirectToAction("MyVehicles", "Usuarios");
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Obtener el vehículo de la API
                var vehiculo = await _apiService.GetAsync<VehiculoViewModel>($"vehiculos/{id}");

                if (vehiculo == null)
                {
                    TempData["ErrorMessage"] = "Vehículo no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Cargar los tipos de combustible para el dropdown
                await CargarTiposCombustible();

                return View(vehiculo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el vehículo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehiculoViewModel model)
        {
            try
            {
                if (id != model.IDVehiculo)
                {
                    TempData["ErrorMessage"] = "ID del vehículo no coincide";
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    await CargarTiposCombustible();
                    return View(model);
                }

                var vehiculoData = new
                {
                    IDVehiculo = model.IDVehiculo,
                    Marca = model.Marca,
                    Modelo = model.Modelo,
                    Year = model.Year,
                    Patente = model.Patente?.ToUpper(),
                    IDCombustible = model.IDCombustible,
                    Observaciones = model.Observaciones
                };

                await _apiService.PutAsync<VehiculoViewModel>($"vehiculos/{id}", vehiculoData);

                TempData["SuccessMessage"] = "Vehículo actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el vehículo: " + ex.Message;
                await CargarTiposCombustible();
                return View(model);
            }
        }

        // Método auxiliar para cargar tipos de combustible
        private async Task CargarTiposCombustible()
        {
            try
            {
                var tiposCombustible = await _apiService.GetAsync<List<AvanzadaWeb.Models.TipoCombustible>>("tiposcombustible");
                ViewBag.TiposCombustible = tiposCombustible ?? new List<AvanzadaWeb.Models.TipoCombustible>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando tipos de combustible: {ex.Message}");
                ViewBag.TiposCombustible = new List<AvanzadaWeb.Models.TipoCombustible>();
            }
        }
    }
}