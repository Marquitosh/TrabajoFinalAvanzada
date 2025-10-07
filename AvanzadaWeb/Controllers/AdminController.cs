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

        public AdminController(IApiService apiService)
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

        // GET: /Admin/ManageAppointments
        public async Task<IActionResult> ManageAppointments()
        {
            try
            {
                var turnos = await _apiService.GetAsync<List<Turno>>("turnos");

                var turnosViewModel = turnos.Select(t => new AppointmentViewModel
                {
                    IDTurno = t.IDTurno,
                    Usuario = t.Usuario?.NombreCompleto ?? $"Usuario {t.IDUsuario}",
                    Vehiculo = t.Vehiculo?.Patente ?? $"Vehículo {t.IDVehiculo}",
                    Fecha = t.Fecha,
                    Hora = t.Hora.ToString(@"hh\:mm"),
                    Estado = t.EstadoTurno?.Descripcion ?? "Desconocido",
                    Servicios = new List<AppointmentServiceViewModel>
                {
                    new AppointmentServiceViewModel
                    {
                        Nombre = t.Servicio?.Nombre ?? "Servicio no especificado",
                        Tiempo = t.Servicio?.TiempoEstimado ?? 0,
                        Costo = t.Servicio?.Precio ?? 0
                    }
                },
                    Observaciones = t.Observaciones
                }).ToList();

                return View(turnosViewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los turnos: " + ex.Message;
                return View(new List<AppointmentViewModel>());
            }
        }

        // POST: /Admin/ConfirmAppointment
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            try
            {
                await _apiService.PutAsync<object>($"turnos/{id}/confirmar", null);
                TempData["Message"] = "El turno fue confirmado exitosamente.";
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
                await _apiService.PutAsync<object>($"turnos/{id}/cancelar", null);
                TempData["Message"] = "El turno fue cancelado exitosamente.";
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
    }
}