using AvanzadaAPI.Models;
using AvanzadaWeb.Filters;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IApiService apiService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // GET: /Admin/ManageAppointments
        public async Task<IActionResult> ManageAppointments()
        {
            try
            {
                // 1. Llamar a la API y deserializar en el NUEVO ViewModel de Admin
                var turnosDesdeApi = await _apiService.GetAsync<List<TurnoAdminViewModel>>("turnos");

                // 2. Mapeo SIMPLIFICADO al ViewModel que la VISTA espera (AppointmentViewModel)
                var turnosViewModel = turnosDesdeApi.Select(t => new AppointmentViewModel
                {
                    IDTurno = t.IdTurno,
                    Usuario = t.Usuario, // Ya viene el nombre formateado
                    Vehiculo = t.Vehiculo, // Ya viene formateado
                    Fecha = t.FechaHora.Date,
                    Hora = t.FechaHora.ToString("HH:mm"),
                    DuracionTotal = t.DuracionTotal, // Ya viene calculada
                    Estado = t.Estado, // Ya viene la descripción
                    Servicios = t.ServiciosNombres.Select(nombre => new AppointmentServiceViewModel
                    {
                        Nombre = nombre,
                        // Tiempo y Costo individuales no vienen en el DTO aplanado
                        Tiempo = 0,
                        Costo = 0
                    }).ToList(),
                    Observaciones = t.Observaciones
                }).ToList();

                // Pasa el ViewModel correcto a la vista
                return View(turnosViewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los turnos: " + ex.Message;
                return View(new List<AppointmentViewModel>()); // Devuelve lista vacía en caso de error
            }
        }

        // POST: /Admin/ConfirmAppointment
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            try
            {
                bool success = await _apiService.PutAsync($"turnos/{id}/confirmar", null);

                if (success)
                {
                    TempData["Message"] = "El turno fue confirmado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "La API no pudo confirmar el turno.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al confirmar el turno: {ex.Message}";
            }

            return RedirectToAction("ManageAppointments");
        }

        // POST: /Admin/CancelAppointment
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                bool success = await _apiService.PutAsync($"turnos/{id}/cancelar", null); 

                if (success)
                {
                    TempData["Message"] = "El turno fue cancelado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "La API no pudo cancelar el turno.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cancelar el turno: {ex.Message}";
            }

            return RedirectToAction("ManageAppointments");
        }

        // GET: /Admin/ManageLog
        public async Task<IActionResult> ManageLog()
        {
            try
            {
                var logs = await _apiService.GetAsync<List<Log>>("logs");

                var logsViewModel = logs.Select(l => new LogViewModel
                {
                    Fecha = l.Fecha,
                    Usuario = l.Usuario,
                    Accion = l.Accion,
                    Descripcion = l.Descripcion,
                    Nivel = l.Nivel,
                    IPAddress = l.IPAddress
                }).ToList();

                return View(logsViewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los logs: " + ex.Message;
                return View(new List<LogViewModel>());
            }
        }

        // ManageRoles
        public async Task<IActionResult> ManageRoles()
        {
            try
            {
                var usuarios = await _apiService.GetAsync<List<Usuario>>("usuarios");

                var niveles = await _apiService.GetAsync<List<NivelUsuario>>("nivelesusuario");

                // Mapear a ViewModel
                var usuariosViewModel = usuarios?.Select(u => new UsuarioRoleViewModel
                {
                    IDUsuario = u.IDUsuario,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}",
                    Email = u.Email,
                    RolActual = niveles?.FirstOrDefault(n => n.IDNivel == u.IDNivel)?.Descripcion ?? "Desconocido",
                    IDRolActual = u.IDNivel
                }).ToList() ?? new List<UsuarioRoleViewModel>();

                var rolesViewModel = niveles?.Select(n => new NivelUsuarioViewModel
                {
                    IDNivel = n.IDNivel,
                    Descripcion = n.Descripcion,
                    RolNombre = n.RolNombre
                }).ToList() ?? new List<NivelUsuarioViewModel>();

                var model = new GestionRolesViewModel
                {
                    Usuarios = usuariosViewModel,
                    RolesDisponibles = rolesViewModel
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los usuarios: " + ex.Message;
                return View(new GestionRolesViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarRol(int idUsuario, int nuevoRolId)
        {
            try
            {
                var usuario = await _apiService.GetAsync<Usuario>($"usuarios/{idUsuario}");

                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado.";
                    return RedirectToAction("ManageRoles");
                }

                var roles = await _apiService.GetAsync<List<NivelUsuario>>("nivelesusuario");
                var rolExiste = roles.Any(r => r.IDNivel == nuevoRolId);

                if (!rolExiste)
                {
                    TempData["ErrorMessage"] = "El rol seleccionado no existe.";
                    return RedirectToAction("ManageRoles");
                }

                var updateData = new
                {
                    IDNivel = nuevoRolId
                };

                await _apiService.PutAsync<object>($"usuarios/{idUsuario}", updateData);

                TempData["SuccessMessage"] = $"Rol actualizado correctamente para {usuario.Nombre} {usuario.Apellido}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar el rol: {ex.Message}";
            }

            return RedirectToAction("ManageRoles");
        }

        // GET: /Admin/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            try
            {
                // Creamos el ViewModel principal
                var model = new ConfiguracionViewModel();

                // Tareas para obtener datos de la API en paralelo
                var horariosTask = _apiService.GetAsync<ConfiguracionViewModel>("configuracion/horarios"); // Reutilizamos el VM por simplicidad
                var combustiblesTask = _apiService.GetAsync<List<TipoCombustibleViewModel>>("tiposcombustible");
                var serviciosTask = _apiService.GetAsync<List<TipoServicioViewModel>>("tiposervicios");

                // Esperamos a que todas terminen
                await Task.WhenAll(horariosTask, combustiblesTask, serviciosTask);

                // Asignamos horarios (manejando posible null de la API)
                var horariosResult = await horariosTask;
                if (horariosResult != null)
                {
                    //_logger.LogWarning($"Horarios Recibidos de API -> Lunes: {horariosResult.LunesInicio ?? "null"} - {horariosResult.LunesFin ?? "null"} | Martes: {horariosResult.MartesInicio ?? "null"} - {horariosResult.MartesFin ?? "null"}");

                    model.IdHorario = horariosResult.IdHorario;
                    model.LunesInicio = horariosResult.LunesInicio;
                    model.LunesFin = horariosResult.LunesFin;
                    model.MartesInicio = horariosResult.MartesInicio;
                    model.MartesFin = horariosResult.MartesFin;
                    model.MiercolesInicio = horariosResult.MiercolesInicio;
                    model.MiercolesFin = horariosResult.MiercolesFin;
                    model.JuevesInicio = horariosResult.JuevesInicio;
                    model.JuevesFin = horariosResult.JuevesFin;
                    model.ViernesInicio = horariosResult.ViernesInicio;
                    model.ViernesFin = horariosResult.ViernesFin;
                    model.SabadoInicio = horariosResult.SabadoInicio;
                    model.SabadoFin = horariosResult.SabadoFin;
                    model.DomingoInicio = horariosResult.DomingoInicio;
                    model.DomingoFin = horariosResult.DomingoFin;
                }
                else
                {
                    _logger.LogWarning("API devolvió null para configuracion/horarios.");
                    // El modelo ya tiene valores por defecto (null para strings)
                }

                // Asignamos listas (manejando posible null de la API)
                model.TiposCombustible = await combustiblesTask ?? new List<TipoCombustibleViewModel>();
                model.TiposServicio = await serviciosTask ?? new List<TipoServicioViewModel>();


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la página de configuración.");
                ViewBag.ErrorMessage = "Error al cargar la configuración: " + ex.Message;
                return View(new ConfiguracionViewModel()); // Devolver modelo vacío
            }
        }

        // POST: /Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(ConfiguracionViewModel model)
        {
            // Remover validaciones no necesarias aquí (las hace la API)
            ModelState.Remove("IdHorario"); // El ID no se envía usualmente en el form

            if (ModelState.IsValid) // Validación básica del ViewModel
            {
                try
                {
                    // Llama al PUT de la API para guardar los horarios
                    // Usamos la versión de PutAsync que devuelve bool (espera 204 No Content)
                    bool success = await _apiService.PutAsync("configuracion/horarios", model);

                    if (success)
                    {
                        TempData["SuccessMessage"] = "Configuración de horarios guardada correctamente.";
                        return RedirectToAction("Settings"); // Recargar la página
                    }
                    else
                    {
                        // La API devolvió un status no exitoso (4xx, 5xx) pero no lanzó excepción
                        TempData["ErrorMessage"] = "La API no pudo guardar la configuración.";
                    }
                }
                catch (HttpRequestException httpEx) // Captura errores específicos de la API
                {
                    _logger.LogError(httpEx, "Error de API al guardar configuración de horarios.");
                    // Intenta leer el mensaje de error de la API si existe
                    string apiError = httpEx.Message; // Usar el mensaje por defecto
                                                      // TODO: Podrías intentar leer el response body si esperas un JSON de error
                    TempData["ErrorMessage"] = $"Error al guardar: {apiError}";
                }
                catch (Exception ex) // Otros errores inesperados
                {
                    _logger.LogError(ex, "Error inesperado al guardar configuración de horarios.");
                    TempData["ErrorMessage"] = "Error inesperado al guardar la configuración: " + ex.Message;
                }
            }
            else
            {
                // Si la validación del ViewModel falla (poco probable con solo DataType.Time)
                TempData["ErrorMessage"] = "Los datos ingresados no son válidos.";
            }

            // Si llegamos aquí, hubo un error, volvemos a mostrar el formulario con los datos ingresados
            return View(model);
        }
    }
}