using AvanzadaAPI.Models;
using AvanzadaWeb.Filters;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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



        public async Task<IActionResult> ManageLog(DateTime? fechaDesde, DateTime? fechaHasta, string? nivel, string? usuario)
        {
            try
            {
                // Construir query string para filtros
                var queryParams = new List<string>();

                if (fechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={fechaDesde.Value:yyyy-MM-dd}");

                if (fechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={fechaHasta.Value:yyyy-MM-dd}");

                if (!string.IsNullOrEmpty(nivel))
                    queryParams.Add($"nivel={nivel}");

                if (!string.IsNullOrEmpty(usuario))
                    queryParams.Add($"usuario={usuario}");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

                // Llamar a la API
                var logs = await _apiService.GetAsync<List<LogViewModel>>($"logs{queryString}");

                // CREAR EL MODELO WRAPPER
                var model = new LogsIndexViewModel
                {
                    Logs = logs ?? new List<LogViewModel>(),
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Nivel = nivel,
                    Usuario = usuario
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error al cargar los logs: {ex.Message}";
                return View(new LogsIndexViewModel());
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
                var marcasTask = _apiService.GetAsync<List<MarcaViewModel>>("marcas");
                var modelosTask = _apiService.GetAsync<List<ModeloViewModel>>("modelos");

                // Esperamos a que todas terminen
                await Task.WhenAll(horariosTask, combustiblesTask, serviciosTask, marcasTask, modelosTask);

                // Asignamos horarios (manejando posible null de la API)
                var horariosResult = await horariosTask;
                if (horariosResult != null)
                {
                    _logger.LogWarning($"[WEB GET] Horarios Recibidos de API -> LunesInicio: {horariosResult.LunesInicio ?? "null"}");

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
                model.Marcas = await marcasTask ?? new List<MarcaViewModel>();
                model.Modelos = await modelosTask ?? new List<ModeloViewModel>();

                // --- Preparar MarcasList para el dropdown del modal Modelo ---
                model.MarcasList = model.Marcas
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem { Value = m.IDMarca.ToString(), Text = m.Nombre })
                    .ToList();

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

        #region CRUD Tipos de Combustible (AJAX)

        // POST: /Admin/CreateCombustible
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCombustible([FromBody] TipoCombustibleViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PostAsync<TipoCombustibleViewModel>("tiposcombustible", model);
                    return Ok(new { success = true, message = "Combustible creado." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }
            return BadRequest(new { success = false, message = "Datos inválidos." });
        }

        // PUT: /Admin/EditCombustible/5
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCombustible(int id, [FromBody] TipoCombustibleViewModel model)
        {
            if (id != model.IdCombustible)
            {
                return BadRequest(new { success = false, message = "ID no coincide." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PutAsync($"tiposcombustible/{id}", model);
                    return Ok(new { success = true, message = "Combustible actualizado." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }
            return BadRequest(new { success = false, message = "Datos inválidos." });
        }

        // DELETE: /Admin/DeleteCombustible/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCombustible(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"tiposcombustible/{id}");
                return Ok(new { success = true, message = "Combustible eliminado." });
            }
            catch (Exception ex)
            {
                // Analizar el mensaje de la excepción genérica
                if (ex.Message.Contains("Conflict") || (ex.InnerException != null && ex.InnerException.Message.Contains("Conflict")))
                {
                    // La API devolvió un 409 (Conflicto)
                    return Conflict(new { message = "No se puede eliminar: El combustible está en uso por uno o más vehículos." });
                }
                _logger.LogError(ex, "Error al eliminar TipoCombustible ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region CRUD Tipos de Servicio (AJAX)

        // GET: /Admin/GetServicio/5
        [HttpGet]
        public async Task<IActionResult> GetServicio(int id)
        {
            try
            {
                // Usamos el VM que ya corregimos (Precio, TiempoEstimado)
                var servicio = await _apiService.GetAsync<TipoServicioViewModel>($"tiposervicios/{id}");
                if (servicio == null)
                {
                    return NotFound();
                }
                return Ok(servicio); // Devuelve el objeto JSON
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // POST: /Admin/CreateServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateServicio([FromBody] TipoServicioViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PostAsync<TipoServicioViewModel>("tiposervicios", model);
                    return Ok(new { success = true, message = "Servicio creado." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }
            return BadRequest(new { success = false, message = "Datos inválidos." });
        }

        // PUT: /Admin/EditServicio/5
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditServicio(int id, [FromBody] TipoServicioViewModel model)
        {
            if (id != model.IdTipoServicio)
            {
                return BadRequest(new { success = false, message = "ID no coincide." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _apiService.PutAsync($"tiposervicios/{id}", model);
                    return Ok(new { success = true, message = "Servicio actualizado." });
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }
            return BadRequest(new { success = false, message = "Datos inválidos." });
        }

        // DELETE: /Admin/DeleteServicio/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteServicio(int id)
        {
            try
            {
                await _apiService.DeleteAsync($"tiposervicios/{id}");
                return Ok(new { success = true, message = "Servicio eliminado." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Conflict") || (ex.InnerException != null && ex.InnerException.Message.Contains("Conflict")))
                {
                    return Conflict(new { message = "No se puede eliminar: El servicio está en uso por uno o más turnos." });
                }
                _logger.LogError(ex, "Error al eliminar TipoServicio ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Marcas (AJAX)

        // GET: Admin/GetMarca/5 (Para poblar modal de edición)
        [HttpGet]
        public async Task<IActionResult> GetMarca(int id)
        {
            try
            {
                var model = await _apiService.GetAsync<MarcaViewModel>($"marcas/{id}");
                if (model == null) return NotFound();
                return Ok(model); // Devuelve JSON
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al cargar: " + ex.Message });
            }
        }

        // POST: Admin/CreateMarca
        [HttpPost]
        public async Task<IActionResult> CreateMarca([FromBody] MarcaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _apiService.PostAsync<MarcaViewModel>("marcas", model);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    // Capturar error de conflicto (409) si la API lo devuelve
                    if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        // Intenta leer el mensaje del cuerpo de la respuesta de la API
                        string apiError = "Error desconocido.";
                        // Aquí necesitarías una forma de leer el Response Body,
                        // ApiService podría necesitar ser modificado para exponer esto.
                        // Por ahora, usamos un mensaje genérico.
                        apiError = "Ya existe una marca con ese nombre."; // Mensaje genérico
                        return Conflict(new { message = apiError });
                    }
                    return BadRequest(new { message = "Error al crear: " + ex.Message });
                }
            }
            return BadRequest(ModelState);
        }

        // PUT: Admin/EditMarca/5
        [HttpPut]
        public async Task<IActionResult> EditMarca(int id, [FromBody] MarcaViewModel model)
        {
            if (id != model.IDMarca)
            {
                return BadRequest(new { message = "El ID no coincide." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    bool success = await _apiService.PutAsync($"marcas/{id}", model);
                    if (success) return Ok(new { success = true, message = "Marca actualizada." });
                    else return NotFound(new { message = "Error de API al actualizar." });
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return Conflict(new { message = "Ya existe otra marca con ese nombre." }); // Mensaje genérico
                    }
                    return BadRequest(new { message = "Error al actualizar: " + ex.Message });
                }
            }
            return BadRequest(ModelState);
        }

        // DELETE: Admin/DeleteMarca/5
        [HttpDelete]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            try
            {
                bool success = await _apiService.DeleteAsync($"marcas/{id}");
                if (success) return Ok(new { success = true, message = "Marca eliminada." });
                else return NotFound(new { message = "Error de API al eliminar." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Conflict") || (ex.InnerException != null && ex.InnerException.Message.Contains("Conflict")))
                {
                    return Conflict(new { message = "No se puede eliminar: La marca está en uso por uno o más modelos." });
                }
                _logger.LogError(ex, "Error al eliminar Marca ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Modelos (AJAX)

        // GET: Admin/GetModelo/5 (Para poblar modal de edición)
        [HttpGet]
        public async Task<IActionResult> GetModelo(int id)
        {
            try
            {
                // El ModeloViewModel ya tiene IDMarca
                var model = await _apiService.GetAsync<ModeloViewModel>($"modelos/{id}");
                if (model == null) return NotFound();
                return Ok(model); // Devuelve JSON
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al cargar: " + ex.Message });
            }
        }


        // POST: Admin/CreateModelo
        [HttpPost]
        public async Task<IActionResult> CreateModelo([FromBody] ModeloViewModel model)
        {
            // Remover IDModelo ya que es para creación
            ModelState.Remove("IDModelo");

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _apiService.PostAsync<ModeloViewModel>("modelos", model);
                    return Ok(new { success = true, message = "Modelo creado." });
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        return Conflict(new { message = "Ya existe un modelo con ese nombre para la marca seleccionada." });
                    }
                    if (ex is HttpRequestException httpEx2 && httpEx2.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        // Podría ser "Marca no existe"
                        return BadRequest(new { message = "La marca seleccionada no es válida." });
                    }
                    return BadRequest(new { message = "Error al crear: " + ex.Message });
                }
            }
            // Añadir log detallado si ModelState no es válido
            // _logger.LogWarning("CreateModelo ModelState inválido: {@ModelState}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }

        // PUT: Admin/EditModelo/5
        [HttpPut]
        public async Task<IActionResult> EditModelo(int id, [FromBody] ModeloViewModel model)
        {
            if (id != model.IDModelo)
            {
                return BadRequest(new { message = "El ID no coincide." });
            }

            if (ModelState.IsValid)
            {
                // --- LOG 1: Loguear qué estamos enviando ---
                _logger.LogInformation("Iniciando PUT /api/modelos/{Id}. Enviando JSON: {JsonPayload}",
                    id, System.Text.Json.JsonSerializer.Serialize(model));

                try
                {
                    bool success = await _apiService.PutAsync($"modelos/{id}", model);
                    if (success) {
                        _logger.LogInformation("PUT /api/modelos/{Id} exitoso (204 No Content)", id);
                        return Ok(new { success = true, message = "Modelo actualizado." });
                    }
                    else
                    {
                        // --- LOG 2: ApiService devolvió false (raro, usualmente lanza excepción) ---
                        _logger.LogWarning("ApiService.PutAsync devolvió 'false' para /api/modelos/{Id}. La API no devolvió 204.", id);
                        return NotFound(new { message = "Error de API al actualizar." });
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        _logger.LogError(httpEx, "Error [HttpRequestException] al llamar a PUT /api/modelos/{Id}. StatusCode: {StatusCode}. Mensaje (JSON de la API): {Message}",
                        id, httpEx.StatusCode, httpEx.Message);
                        return Conflict(new { message = "Ya existe otro modelo con ese nombre para la marca seleccionada." });
                    }
                    if (ex is HttpRequestException httpEx2 && httpEx2.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogError(httpEx2, "Error [HttpRequestException] al llamar a PUT /api/modelos/{Id}. StatusCode: {StatusCode}. Mensaje (JSON de la API): {Message}",
                        id, httpEx2.StatusCode, httpEx2.Message);
                        return BadRequest(new { message = "La marca seleccionada no es válida." });
                    }
                    // --- LOG 4: Loguear errores genéricos ---
                    _logger.LogError(ex, "Error [Exception] inesperado en EditModelo para ID {Id}", id); return BadRequest(new { message = "Error al actualizar: " + ex.Message });
                }
            }
            _logger.LogWarning("EditModelo ModelState inválido: {@ModelState}", ModelState.Values.SelectMany(v => v.Errors));
            return BadRequest(ModelState);
        }

        // DELETE: Admin/DeleteModelo/5
        [HttpDelete]
        public async Task<IActionResult> DeleteModelo(int id)
        {
            try
            {
                bool success = await _apiService.DeleteAsync($"modelos/{id}");
                if (success) return Ok(new { success = true, message = "Modelo eliminado." });
                else return NotFound(new { message = "Error de API al eliminar." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Conflict") || (ex.InnerException != null && ex.InnerException.Message.Contains("Conflict")))
                {
                    return Conflict(new { message = "No se puede eliminar: El modelo está en uso por uno o más vehículos." });
                }
                _logger.LogError(ex, "Error al eliminar Modelo ID {Id}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }


}