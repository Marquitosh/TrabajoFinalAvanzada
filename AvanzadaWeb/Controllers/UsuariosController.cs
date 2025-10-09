using AvanzadaAPI.Models;
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
                {
                    return RedirectToAction("Login", "Account");
                }

                var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);

                var usuario = await _apiService.GetAsync<Usuario>($"usuarios/{sessionUser.IDUsuario}");

                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "Error al cargar el perfil.";
                    return View();
                }

                var model = new UsuarioViewModel
                {
                    IDUsuario = usuario.IDUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    IDNivel = usuario.IDNivel,
                    NivelDescripcion = usuario.NivelUsuario?.Descripcion ?? "Cliente",
                    FotoBase64 = usuario.Foto != null ? Convert.ToBase64String(usuario.Foto) : null
                };

                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Error al cargar el perfil.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UsuarioViewModel model, IFormFile FotoFile, string NewPassword, string ConfirmPassword)
        {
            ModelState.Remove("FotoFile");
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("NivelDescripcion");
            ModelState.Remove("FotoBase64");

            if (ModelState.IsValid)
            {
                try
                {
                    var userJson = HttpContext.Session.GetString("User");
                    if (string.IsNullOrEmpty(userJson))
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var sessionUser = System.Text.Json.JsonSerializer.Deserialize<SessionUser>(userJson);
                    int userId = sessionUser.IDUsuario;

                    if (!string.IsNullOrEmpty(NewPassword) || !string.IsNullOrEmpty(ConfirmPassword))
                    {
                        if (NewPassword != ConfirmPassword)
                        {
                            ModelState.AddModelError("", "Las contraseñas no coinciden.");
                            return View("Profile", model);
                        }

                        if (NewPassword.Length < 6)
                        {
                            ModelState.AddModelError("", "La contraseña debe tener al menos 6 caracteres.");
                            return View("Profile", model);
                        }
                    }

                    // Procesar foto
                    byte[]? fotoBytes = null;
                    if (FotoFile != null && FotoFile.Length > 0)
                    {
                        fotoBytes = await ProcessPhoto(FotoFile);
                       
                    }

                    // Crear objeto DTO para enviar a la API
                    var updateData = new
                    {
                        Nombre = model.Nombre?.Trim(),
                        Apellido = model.Apellido?.Trim(),
                        Email = model.Email?.Trim(),
                        Telefono = model.Telefono?.Trim(),
                        ContraseñaString = NewPassword,
                        Foto = fotoBytes
                    };

                    var response = await _apiService.PutAsync<Usuario>($"usuarios/updateprofile/{userId}", updateData);

                    if (response != null)
                    {
                        var updatedSessionUser = new SessionUser
                        {
                            IDUsuario = response.IDUsuario,
                            Email = response.Email,
                            Nombre = response.Nombre,
                            Apellido = response.Apellido,
                            RolNombre = sessionUser.RolNombre,
                            Foto = response.Foto != null ? Convert.ToBase64String(response.Foto) : null
                        };

                        HttpContext.Session.SetString("User", System.Text.Json.JsonSerializer.Serialize(updatedSessionUser));
                    }

                    TempData["SuccessMessage"] = "Perfil actualizado correctamente.";

                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error al actualizar el perfil: " + ex.Message;
                }
            }

            return View("Profile", model);
        }

        private async Task<byte[]> ProcessPhoto(IFormFile fotoFile)
        {
            try
            { 

                if (fotoFile == null || fotoFile.Length == 0)
                {
                    return null;
                }

                // Validar tipo de archivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fotoFile.FileName).ToLower();


                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("Formato de archivo no permitido. Use JPG, PNG o GIF.");
                }

                // Validar tamaño (2MB máximo)
                if (fotoFile.Length > 2 * 1024 * 1024)
                {
                    throw new Exception("La imagen es demasiado grande. Tamaño máximo: 2MB.");
                }

                // Convertir a byte array
                using var memoryStream = new MemoryStream();
                await fotoFile.CopyToAsync(memoryStream);
                var result = memoryStream.ToArray();

                return result;
            }
            catch (Exception)
            {
                throw;
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

                //var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);
                //var turnos = await _apiService.GetAsync<List<TurnoViewModel>>($"turnos/Usuario/{sessionUser.IDUsuario}");

                // 🚧 Datos de ejemplo (en un caso real vendrían de la BD)
                var turnos = new List<AppointmentViewModel>
        {
            new AppointmentViewModel
            {
                Usuario = "Juan Pérez",
                Fecha = DateTime.Today.AddDays(2),
                Hora = "10:00",
                Estado = "Pendiente",
                Servicios = new List<AppointmentServiceViewModel>
                {
                    new AppointmentServiceViewModel { Nombre = "Cambio de aceite", Tiempo = 30, Costo = 2500 },
                    new AppointmentServiceViewModel { Nombre = "Chequeo general", Tiempo = 45, Costo = 3500 }
                }
            },
            new AppointmentViewModel
            {
                Usuario = "María Gómez",
                Fecha = DateTime.Today.AddDays(3),
                Hora = "14:30",
                Estado = "Confirmado",
                Servicios = new List<AppointmentServiceViewModel>
                {
                    new AppointmentServiceViewModel { Nombre = "Alineación y balanceo", Tiempo = 40, Costo = 4000 },
                    new AppointmentServiceViewModel { Nombre = "Cambio de pastillas de freno", Tiempo = 35, Costo = 3200}
                }
            } };
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