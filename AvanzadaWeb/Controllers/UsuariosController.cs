using AvanzadaWeb.Models;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AvanzadaWeb.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IApiService _apiService;

        public UsuariosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Acciones de administración (existentes)
        public async Task<IActionResult> Index()
        {
            var usuarios = await _apiService.GetAsync<List<UsuarioViewModel>>("usuarios");
            return View(usuarios);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UsuarioViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PostAsync<UsuarioViewModel>("usuarios", usuario);
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _apiService.GetAsync<UsuarioViewModel>($"usuarios/{id}");
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UsuarioViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                await _apiService.PutAsync<UsuarioViewModel>($"usuarios/{id}", usuario);
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _apiService.DeleteAsync($"usuarios/{id}");
            return RedirectToAction(nameof(Index));
        }

        // Nuevas acciones para el panel de usuario
        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Login", "Account");

                var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);
                var usuario = await _apiService.GetAsync<UsuarioViewModel>($"usuarios/{sessionUser.IDUsuario}");

                return View(usuario);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar el perfil: " + ex.Message;
                return View();
            }
        }

        public async Task<IActionResult> MyVehicles()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Login", "Account");

                var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);
                var vehiculos = await _apiService.GetAsync<List<VehiculoViewModel>>($"vehiculos/Usuario/{sessionUser.IDUsuario}");

                return View(vehiculos);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los vehículos: " + ex.Message;
                return View(new List<VehiculoViewModel>());
            }
        }

        public async Task<IActionResult> MyAppointments()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Login", "Account");

                var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);
                var turnos = await _apiService.GetAsync<List<TurnoViewModel>>($"turnos/Usuario/{sessionUser.IDUsuario}");

                return View(turnos);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los turnos: " + ex.Message;
                return View(new List<TurnoViewModel>());
            }
        }

        public IActionResult RequestService()
        {
            return View();
        }

        public IActionResult ScheduleAppointment(List<int> ids)
        {
            
            return View();
        }

        // POST: /Admin/ConfirmAppointment
        [HttpPost]
        public IActionResult ConfirmAppointment()
        {
            // 🚧 Acá va la lógica para confirmar el turno (ej: update en BD)
            TempData["Message"] = $"El turno fue solicitado exitosamente.";

            return RedirectToAction("ScheduleAppointment");
        }
    }
}