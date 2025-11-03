using AvanzadaAPI.Models;
using AvanzadaWeb.Models;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AvanzadaWeb.Controllers
{
    public class VehiculosController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<VehiculosController> _logger;

        public VehiculosController(IApiService apiService, ILogger<VehiculosController> logger)
        {
            _apiService = apiService;
            _logger = logger;
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

        // GET: Vehiculos/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new VehiculoViewModel();

            try
            {
                // Tareas en paralelo para cargar dropdowns
                var marcasTask = _apiService.GetAsync<List<MarcaViewModel>>("marcas");
                var combustiblesTask = _apiService.GetAsync<List<TipoCombustibleViewModel>>("tiposcombustible");

                await Task.WhenAll(marcasTask, combustiblesTask);

                var marcas = await marcasTask;
                var combustibles = await combustiblesTask;

                // Cargar Marcas
                viewModel.MarcasList = marcas?
                    .Select(m => new SelectListItem { Value = m.IDMarca.ToString(), Text = m.Nombre })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();

                // Cargar Combustibles
                viewModel.CombustiblesList = combustibles?
                    .Select(c => new SelectListItem { Value = c.IdCombustible.ToString(), Text = c.Descripcion }) // Asumiendo IdCombustible y Descripcion
                    .OrderBy(c => c.Text)
                    .ToList() ?? new List<SelectListItem>();

                // El dropdown de Modelos se deja vacío, se carga con JS
                viewModel.ModelosList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Primero seleccione una marca..." }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para crear vehículo.");
                TempData["ErrorMessage"] = "Error al cargar datos: " + ex.Message;
            }

            return View(viewModel);
        }

        // POST: Vehiculos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehiculoViewModel vehiculo)
        {
            var user = HttpContext.Session.GetString("User");
            if (user == null) return RedirectToAction("Login", "Account");
            var sessionUser = System.Text.Json.JsonSerializer.Deserialize<SessionUser>(user);

            vehiculo.UsuarioNombre = sessionUser.Nombre;

            // Removemos las listas del ModelState, no son parte del POST
            ModelState.Remove("MarcasList");
            ModelState.Remove("ModelosList");
            ModelState.Remove("CombustiblesList");
            ModelState.Remove("MarcaNombre");
            ModelState.Remove("ModeloNombre");
            ModelState.Remove("CombustibleNombre");
            ModelState.Remove("Patente"); // Quitamos la validación regex de C# para confiar en la de JS

            if (ModelState.IsValid)
            {
                try
                {
                    // El ApiService enviará el ViewModel con IDMarca, IDModelo, etc.
                    // La API (VehiculosController) debe estar esperando un DTO con estas propiedades int
                    await _apiService.PostAsync<VehiculoViewModel>($"usuarios/{sessionUser.IDUsuario}/vehiculos", vehiculo);

                    TempData["SuccessMessage"] = "Vehículo registrado exitosamente.";
                    return RedirectToAction("MyVehicles", "Usuarios");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar vehículo.");
                    TempData["ErrorMessage"] = "Error al guardar: " + ex.Message;
                }
            }

            // Si llegamos aquí, hubo un error, recargamos los dropdowns
            try
            {
                var marcas = await _apiService.GetAsync<List<MarcaViewModel>>("marcas");
                var combustibles = await _apiService.GetAsync<List<TipoCombustibleViewModel>>("tiposcombustible");

                vehiculo.MarcasList = marcas?
                    .Select(m => new SelectListItem { Value = m.IDMarca.ToString(), Text = m.Nombre })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();

                vehiculo.CombustiblesList = combustibles?
                    .Select(c => new SelectListItem { Value = c.IdCombustible.ToString(), Text = c.Descripcion })
                    .OrderBy(c => c.Text)
                    .ToList() ?? new List<SelectListItem>();

                // Si ya había seleccionado una marca, recargar los modelos
                if (vehiculo.IDMarca > 0)
                {
                    var modelos = await _apiService.GetAsync<List<ModeloViewModel>>($"modelos/marca/{vehiculo.IDMarca}");
                    vehiculo.ModelosList = modelos?
                        .Select(m => new SelectListItem { Value = m.IDModelo.ToString(), Text = m.Nombre })
                        .OrderBy(m => m.Text)
                        .ToList() ?? new List<SelectListItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recargando dropdowns en POST fallido.");
                TempData["ErrorMessage"] = "Error al recargar datos: " + ex.Message;
            }

            return View(vehiculo);
        }

        // --- ACCIÓN NUEVA PARA JAVASCRIPT ---

        // GET: /Vehiculos/GetModelosPorMarca?idMarca=5
        [HttpGet]
        public async Task<JsonResult> GetModelosPorMarca(int idMarca)
        {
            if (idMarca == 0)
            {
                return Json(new List<ModeloViewModel>());
            }

            try
            {
                var modelos = await _apiService.GetAsync<List<ModeloViewModel>>($"modelos/marca/{idMarca}");
                return Json(modelos ?? new List<ModeloViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetModelosPorMarca (AJAX)");
                return Json(new { error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"vehiculos/{id}");
                return Ok(new { message = "Vehículo eliminado exitosamente." });
            }
            catch (Exception ex) // Capturar la excepción de ApiService
            {
                _logger.LogError(ex, "Error al intentar eliminar vehículo ID {Id}", id);

                if (ex.Message.Contains("Conflict") || (ex.InnerException != null && ex.InnerException.Message.Contains("Conflict")))
                {
                    // Error 409: Devolver Conflict (409) con el mensaje
                    return Conflict(new { message = "No se puede eliminar: El vehículo tiene turnos asociados." });
                }
                else
                {
                    // Otro error: Devolver BadRequest (400)
                    return BadRequest(new { message = "Error al eliminar el vehículo: " + ex.Message });
                }
            }
        }

        // GET: Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // 1. Tarea principal: Obtener el vehículo
                var vehiculoTask = _apiService.GetAsync<VehiculoViewModel>($"vehiculos/{id}");

                // 2. Tareas secundarias: Cargar listas de Marcas y Combustibles
                var marcasTask = _apiService.GetAsync<List<MarcaViewModel>>("marcas");
                var combustiblesTask = _apiService.GetAsync<List<TipoCombustibleViewModel>>("tiposcombustible");

                // Esperar tareas de listas
                await Task.WhenAll(marcasTask, combustiblesTask);

                // 3. Obtener el vehículo (espera a que termine su tarea)
                var viewModel = await vehiculoTask;
                if (viewModel == null)
                {
                    TempData["ErrorMessage"] = "Vehículo no encontrado.";
                    return RedirectToAction("Dashboard", "Usuarios");
                }

                // 4. Tarea dependiente: Cargar modelos PARA LA MARCA ACTUAL del vehículo
                var modelosTask = _apiService.GetAsync<List<ModeloViewModel>>($"modelos/marca/{viewModel.IDMarca}");

                // 5. Poblar listas de Marcas y Combustibles (con valor seleccionado)
                var marcas = await marcasTask;
                var combustibles = await combustiblesTask;

                viewModel.MarcasList = marcas?
                    .Select(m => new SelectListItem
                    {
                        Value = m.IDMarca.ToString(),
                        Text = m.Nombre,
                        Selected = m.IDMarca == viewModel.IDMarca // Marcar como seleccionado
                    })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();

                viewModel.CombustiblesList = combustibles?
                    .Select(c => new SelectListItem
                    {
                        Value = c.IdCombustible.ToString(), // Asumiendo IdCombustible
                        Text = c.Descripcion,
                        Selected = c.IdCombustible == viewModel.IDCombustible // Marcar como seleccionado
                    })
                    .OrderBy(c => c.Text)
                    .ToList() ?? new List<SelectListItem>();

                // 6. Poblar lista de Modelos (esperando la tarea dependiente)
                var modelos = await modelosTask;
                viewModel.ModelosList = modelos?
                    .Select(m => new SelectListItem
                    {
                        Value = m.IDModelo.ToString(),
                        Text = m.Nombre,
                        Selected = m.IDModelo == viewModel.IDModelo // Marcar como seleccionado
                    })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vehículo para editar.");
                TempData["ErrorMessage"] = "Error al cargar datos: " + ex.Message;
                return RedirectToAction("Dashboard", "Usuarios");
            }
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehiculoViewModel vehiculo)
        {
            if (id != vehiculo.IDVehiculo)
            {
                return BadRequest();
            }

            // Removemos las listas del ModelState, no son parte del POST
            ModelState.Remove("MarcasList");
            ModelState.Remove("ModelosList");
            ModelState.Remove("CombustiblesList");
            ModelState.Remove("MarcaNombre");
            ModelState.Remove("ModeloNombre");
            ModelState.Remove("CombustibleNombre");
            ModelState.Remove("Patente"); // Opcional: si confías en la validación de JS

            if (ModelState.IsValid)
            {
                try
                {
                    // El ApiService enviará el ViewModel con IDMarca, IDModelo, etc.
                    await _apiService.PutAsync($"vehiculos/{id}", vehiculo);

                    TempData["SuccessMessage"] = "Vehículo actualizado exitosamente.";
                    return RedirectToAction("Dashboard", "Usuarios"); // O a "MyVehicles"
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar vehículo.");
                    TempData["ErrorMessage"] = "Error al actualizar: " + ex.Message;
                }
            }

            // Si llegamos aquí, hubo un error, recargamos los dropdowns
            try
            {
                var marcasTask = _apiService.GetAsync<List<MarcaViewModel>>("marcas");
                var combustiblesTask = _apiService.GetAsync<List<TipoCombustibleViewModel>>("tiposcombustible");
                // Cargamos los modelos de la marca que el usuario *intentó* guardar
                var modelosTask = _apiService.GetAsync<List<ModeloViewModel>>($"modelos/marca/{vehiculo.IDMarca}");

                await Task.WhenAll(marcasTask, combustiblesTask, modelosTask);

                var marcas = await marcasTask;
                var combustibles = await combustiblesTask;
                var modelos = await modelosTask;

                vehiculo.MarcasList = marcas?
                    .Select(m => new SelectListItem { Value = m.IDMarca.ToString(), Text = m.Nombre, Selected = m.IDMarca == vehiculo.IDMarca })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();

                vehiculo.CombustiblesList = combustibles?
                    .Select(c => new SelectListItem { Value = c.IdCombustible.ToString(), Text = c.Descripcion, Selected = c.IdCombustible == vehiculo.IDCombustible })
                    .OrderBy(c => c.Text)
                    .ToList() ?? new List<SelectListItem>();

                vehiculo.ModelosList = modelos?
                    .Select(m => new SelectListItem { Value = m.IDModelo.ToString(), Text = m.Nombre, Selected = m.IDModelo == vehiculo.IDModelo })
                    .OrderBy(m => m.Text)
                    .ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recargando dropdowns en Edit POST fallido.");
                TempData["ErrorMessage"] = "Error al recargar datos: " + ex.Message;
            }

            return View(vehiculo);
        }
    }
}