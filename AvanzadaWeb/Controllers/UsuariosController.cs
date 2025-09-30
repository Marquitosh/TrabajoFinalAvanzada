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
                {
                    return RedirectToAction("Login", "Account");
                }

                var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);

                // Obtener datos actualizados desde la API
                var usuario = await _apiService.GetAsync<Usuario>($"usuarios/{sessionUser.IDUsuario}");

                if (usuario == null)
                {
                    Console.WriteLine("ERROR: No se pudo cargar el usuario desde la API");
                    TempData["ErrorMessage"] = "Error al cargar el perfil.";
                    return View();
                }

                // Mapear a ViewModel
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
            catch (Exception ex)
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
                    // Obtener el ID del usuario desde la sesión
                    var userJson = HttpContext.Session.GetString("User");
                    if (string.IsNullOrEmpty(userJson))
                    {
                        Console.WriteLine("ERROR: No hay sesión de usuario");
                        return RedirectToAction("Login", "Account");
                    }

                    var sessionUser = System.Text.Json.JsonSerializer.Deserialize<SessionUser>(userJson);
                    int userId = sessionUser.IDUsuario;
                    Console.WriteLine($"UserId desde sesión: {userId}");

                    // Validar contraseñas si se proporcionaron
                    if (!string.IsNullOrEmpty(NewPassword) || !string.IsNullOrEmpty(ConfirmPassword))
                    {
                        Console.WriteLine("Validando contraseñas...");
                        if (NewPassword != ConfirmPassword)
                        {
                            Console.WriteLine("ERROR: Las contraseñas no coinciden");
                            ModelState.AddModelError("", "Las contraseñas no coinciden.");
                            return View("Profile", model);
                        }

                        if (NewPassword.Length < 6)
                        {
                            Console.WriteLine("ERROR: Contraseña muy corta");
                            ModelState.AddModelError("", "La contraseña debe tener al menos 6 caracteres.");
                            return View("Profile", model);
                        }
                        Console.WriteLine("Contraseñas validadas correctamente");
                    }

                    // Procesar foto
                    byte[]? fotoBytes = null;
                    if (FotoFile != null && FotoFile.Length > 0)
                    {
                        Console.WriteLine($"Procesando foto: {FotoFile.FileName}, Tamaño: {FotoFile.Length} bytes");
                        fotoBytes = await ProcessPhoto(FotoFile);
                        if (fotoBytes != null)
                        {
                            Console.WriteLine($"Foto procesada: {fotoBytes.Length} bytes");
                        }
                        else
                        {
                            Console.WriteLine("ERROR: La foto no se pudo procesar");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se subió nueva foto - se mantendrá la actual");
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

                    Console.WriteLine($"Enviando datos a API: usuarios/updateprofile/{userId}");

                    // Llamar a la API para actualizar
                    var response = await _apiService.PutAsync<Usuario>($"usuarios/updateprofile/{userId}", updateData);
                    Console.WriteLine("API respondió correctamente");

                    // ACTUALIZAR LA SESIÓN con los nuevos datos
                    if (response != null)
                    {
                        var updatedSessionUser = new SessionUser
                        {
                            IDUsuario = response.IDUsuario,
                            Email = response.Email,
                            Nombre = response.Nombre,
                            Apellido = response.Apellido,
                            NivelUsuario = sessionUser.NivelUsuario, // Mantener el nivel de la sesión anterior
                            Foto = response.Foto != null ? Convert.ToBase64String(response.Foto) : null
                        };

                        HttpContext.Session.SetString("User", System.Text.Json.JsonSerializer.Serialize(updatedSessionUser));
                        Console.WriteLine("Sesión actualizada con nuevos datos");
                    }

                    TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                    Console.WriteLine("=== FIN UpdateProfile (ÉXITO) ===");

                    // Recargar los datos frescos desde la base de datos
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR en UpdateProfile: {ex.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    Console.WriteLine($"Tipo de excepción: {ex.GetType().Name}");

                    TempData["ErrorMessage"] = "Error al actualizar el perfil: " + ex.Message;
                }
            }
            else
            {
                Console.WriteLine("ModelState no es válido después de limpiar:");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    if (errors.Count > 0)
                    {
                        Console.WriteLine($" - {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            Console.WriteLine("=== FIN UpdateProfile (ERROR) ===");
            return View("Profile", model);
        }

        private async Task<byte[]> ProcessPhoto(IFormFile fotoFile)
        {
            try
            {
                Console.WriteLine($"ProcessPhoto iniciado - File: {fotoFile?.FileName}, Size: {fotoFile?.Length}");

                if (fotoFile == null || fotoFile.Length == 0)
                {
                    Console.WriteLine("ProcessPhoto: Archivo vacío o null");
                    return null;
                }

                // Validar tipo de archivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fotoFile.FileName).ToLower();

                Console.WriteLine($"ProcessPhoto - Extensión: {extension}");

                if (!allowedExtensions.Contains(extension))
                {
                    Console.WriteLine($"ProcessPhoto - Extensión no permitida: {extension}");
                    throw new Exception("Formato de archivo no permitido. Use JPG, PNG o GIF.");
                }

                // Validar tamaño (2MB máximo)
                if (fotoFile.Length > 2 * 1024 * 1024)
                {
                    Console.WriteLine($"ProcessPhoto - Archivo demasiado grande: {fotoFile.Length} bytes");
                    throw new Exception("La imagen es demasiado grande. Tamaño máximo: 2MB.");
                }

                // Convertir a byte array
                using var memoryStream = new MemoryStream();
                await fotoFile.CopyToAsync(memoryStream);
                var result = memoryStream.ToArray();

                Console.WriteLine($"ProcessPhoto completado - Bytes: {result.Length}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en ProcessPhoto: {ex}");
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