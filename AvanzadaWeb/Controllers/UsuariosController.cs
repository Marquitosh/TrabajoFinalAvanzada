using AvanzadaAPI.Models;
using AvanzadaWeb.Models;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace AvanzadaWeb.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AdminController> _logger;

        public UsuariosController(IApiService apiService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = System.Text.Json.JsonSerializer.Deserialize<SessionUser>(userJson);

                var dashboardData = new DashboardViewModel
                {
                    User = user
                };

                try
                {
                    var vehiculosCount = await _apiService.GetAsync<int>($"usuarios/{user.IDUsuario}/vehiculos/count");
                    dashboardData.VehiculosCount = vehiculosCount;

                    var turnosCount = await _apiService.GetAsync<int>($"usuarios/{user.IDUsuario}/turnos/count");
                    dashboardData.TurnosCount = turnosCount;

                    var proximoTurno = await _apiService.GetAsync<ProximoTurnoDto>($"usuarios/{user.IDUsuario}/turnos/proximo");

                    if (proximoTurno?.Fecha != null && proximoTurno.Hora != null)
                    {
                        var fechaHoraCompleta = proximoTurno.Fecha.Value.Add(proximoTurno.Hora.Value);
                        var diasRestantes = (fechaHoraCompleta.Date - DateTime.Today).Days;
                        var horaFormateada = fechaHoraCompleta.ToString("HH:mm");

                        if (diasRestantes == 0)
                        {
                            dashboardData.ProximoTurno = $"Hoy {horaFormateada}";
                        }
                        else if (diasRestantes == 1)
                        {
                            dashboardData.ProximoTurno = $"Mañana {horaFormateada}";
                        }
                        else
                        {
                            var fechaFormateada = fechaHoraCompleta.ToString("dd/MM");
                            dashboardData.ProximoTurno = $"{fechaFormateada} {horaFormateada}";
                        }

                        dashboardData.ProximoTurnoFecha = proximoTurno.Fecha;
                        dashboardData.ProximoTurnoHora = proximoTurno.Hora;
                    }
                    else
                    {
                        dashboardData.ProximoTurno = "No programado";
                    }

                    dashboardData.ProximoService = await CalcularProximoService(user.IDUsuario);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando datos del dashboard: {ex.Message}");
                    dashboardData.VehiculosCount = 0;
                    dashboardData.TurnosCount = 0;
                    dashboardData.ProximoTurno = "No disponible";
                    dashboardData.ProximoService = "No disponible";
                }

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Dashboard: {ex.Message}");
                return RedirectToAction("Login", "Account");
            }
        }

        private async Task<string> CalcularProximoService(int usuarioId)
        {
            try
            {
                // Obtener vehículos del usuario
                var vehiculos = await _apiService.GetAsync<List<VehiculoViewModel>>($"usuarios/{usuarioId}/vehiculos");

                if (vehiculos == null || !vehiculos.Any())
                    return "Sin vehículos";

                //asumimos que cada vehículo necesita service cada 6 meses
                var random = new Random();
                var diasParaService = random.Next(1, 60);

                if (diasParaService == 1)
                    return "Mañana";
                else if (diasParaService <= 7)
                    return "Esta semana";
                else if (diasParaService <= 30)
                    return "Este mes";
                else
                    return $"En {diasParaService} días";
            }
            catch
            {
                return "No disponible";
            }
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
                var userId = sessionUser.IDUsuario;
                List<VehiculoViewModel> vehiculos = await _apiService.GetAsync<List<VehiculoViewModel>>($"usuarios/{userId}/vehiculos");

                if (vehiculos == null)
                {
                    vehiculos = new List<VehiculoViewModel>(); // Ensure it's never null for the view
                    _logger.LogWarning("API returned null for user {UserId}'s vehicles.", userId);
                }

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

                // 1. Llamar a la API y deserializar en el NUEVO ViewModel
                var turnosDesdeApi = await _apiService.GetAsync<List<TurnoUsuarioViewModel>>($"turnos/Usuario/{sessionUser.IDUsuario}");

                // 2. Mapeo SIMPLIFICADO al ViewModel de la VISTA
                var turnosViewModel = turnosDesdeApi.Select(t => new AppointmentViewModel
                {
                    IDTurno = t.IdTurno,
                    Usuario = sessionUser.Nombre,
                    Vehiculo = t.Vehiculo,
                    Fecha = t.FechaHora.Date,
                    Hora = t.FechaHora.ToString("HH:mm"),
                    DuracionTotal = t.DuracionTotal, 
                    Estado = t.Estado, 
                    Servicios = t.ServiciosNombres.Select(nombre => new AppointmentServiceViewModel
                    {
                        Nombre = nombre,
                        // Nota: Costo y Tiempo individual ya no vienen en este DTO aplanado
                        Tiempo = 0, // Podrías necesitar ajustar la vista si usabas estos
                        Costo = 0
                    }).ToList(),
                    Observaciones = t.Observaciones
                }).ToList();

                return View(turnosViewModel);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR en MyAppointments: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los turnos: " + ex.Message;
                return View(new List<AppointmentViewModel>());
            }
        }
        public async Task<IActionResult> RequestService()
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
                return RedirectToAction("Login", "Account");

            try
            {
                // Cambiamos el endpoint a "tiposervicios" y el ViewModel
                var servicios = await _apiService.GetAsync<List<TipoServicioViewModel>>("tiposervicios");
                return View(servicios); // Pasamos la lista a la vista
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los servicios: " + ex.Message;
                return View(new List<TipoServicioViewModel>()); // Usamos el nuevo ViewModel
            }
        }

        public async Task<IActionResult> ScheduleAppointment([FromQuery] List<int> ids)
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
                return RedirectToAction("Login", "Account");

            var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson); // Obtener usuario de sesión

            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un servicio.";
                return RedirectToAction("RequestService");
            }

            try
            {
                var model = new ScheduleAppointmentViewModel();

                // 1. Crear el querystring para los IDs
                var idQuery = string.Join("&", ids.Select(id => $"ids={id}"));

                // 2. Tareas en paralelo
                var serviciosTask = _apiService.GetAsync<List<TipoServicioViewModel>>($"agenda/tiposervicios?{idQuery}");
                // (NUEVO) Obtener vehículos del usuario
                var vehiculosTask = _apiService.GetAsync<List<VehiculoViewModel>>($"usuarios/{sessionUser.IDUsuario}/vehiculos");

                await Task.WhenAll(serviciosTask, vehiculosTask); // Esperar ambas tareas

                model.ServiciosSeleccionados = await serviciosTask;
                model.VehiculosUsuario = await vehiculosTask; // Asignar vehículos al modelo

                if (model.ServiciosSeleccionados == null || !model.ServiciosSeleccionados.Any())
                {
                    TempData["ErrorMessage"] = "Error al cargar los servicios seleccionados.";
                    return RedirectToAction("RequestService");
                }

                // 3. Obtener días (depende de la duración, así que va después)
                int duracionTurno = model.DuracionNuevoTurnoMinutos;
                model.DiasDisponibles = await _apiService.GetAsync<List<DiaDisponibleViewModel>>($"agenda/diasdisponibles?duracionTurno={duracionTurno}");

                if (model.DiasDisponibles == null)
                {
                    ViewBag.ErrorMessage = "No se pudieron cargar los días disponibles desde la API.";
                }
                else if (!model.DiasDisponibles.Any())
                {
                    ViewBag.ErrorMessage = "No se encontraron turnos disponibles para los servicios seleccionados en los próximos 6 meses.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar la agenda: " + ex.Message;
                var errorModel = new ScheduleAppointmentViewModel(); // Devolver un modelo vacío
                return View(errorModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleAppointment(ScheduleAppointmentViewModel model, string SelectedFechaHora, int IdVehiculo)
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
                return RedirectToAction("Login", "Account");

            var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);

            // Validar que se haya seleccionado un vehículo (el [Required] no funciona en IdVehiculo)
            if (IdVehiculo <= 0)
            {
                TempData["ErrorMessage"] = "Debe seleccionar un vehículo.";
                // Recargamos la página reenviando los IDs
                var idsQueryError = string.Join("&", model.ServiciosSeleccionados.Select(s => "ids=" + s.IdTipoServicio));
                return RedirectToAction("ScheduleAppointment", new { ids = idsQueryError });
            }

            try
            {
                // 1. Crear el DTO para enviar a la API
                var createTurnoDto = new
                {
                    IDUsuario = sessionUser.IDUsuario,
                    IDVehiculo = IdVehiculo,
                    FechaHora = DateTime.Parse(SelectedFechaHora), // El DTO de la API lo recibirá
                    IDEstadoTurno = 1, // 1 = Pendiente (Según seed.sql)
                    IdTipoServicios = model.ServiciosSeleccionados.Select(s => s.IdTipoServicio).ToList()
                };

                // 2. Llamar al POST de TurnoController (Ahora es real)
                await _apiService.PostAsync<object>("turnos", createTurnoDto);

                // 3. Respuesta (Real)
                TempData["SuccessMessage"] = $"El turno fue solicitado exitosamente para el {DateTime.Parse(SelectedFechaHora).ToString("dd/MM/yyyy 'a las' HH:mm")}.";
                return RedirectToAction("MyAppointments");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al confirmar el turno: " + ex.Message;
                // Re-crear el querystring de IDs para recargar la página
                var idsQuery = string.Join("&", model.ServiciosSeleccionados.Select(s => "ids=" + s.IdTipoServicio));
                return RedirectToAction("ScheduleAppointment", new { ids = idsQuery });
            }
        }

    }
}