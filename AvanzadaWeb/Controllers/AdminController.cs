using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Controllers
{
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
        public IActionResult ManageAppointments()
        {
            // 🚧 Datos de ejemplo (en un caso real vendrían de la BD)
            var turnos = new List<AppointmentViewModel>
        {
            new AppointmentViewModel
            {
                Usuario = "Juan Pérez",
                Fecha = DateTime.Today.AddDays(2),
                Hora = "10:00",
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
                Servicios = new List<AppointmentServiceViewModel>
                {
                    new AppointmentServiceViewModel { Nombre = "Alineación y balanceo", Tiempo = 40, Costo = 4000 },
                    new AppointmentServiceViewModel { Nombre = "Cambio de pastillas de freno", Tiempo = 35, Costo = 3200 }
                }
            }
        };

            return View(turnos);
        }
        public IActionResult ManageRoles()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Login", "Account");

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los vehículos: " + ex.Message;
                return View(new List<VehiculoViewModel>());
            }
        }

        // POST: /Admin/ConfirmAppointment
        [HttpPost]
        public IActionResult ConfirmAppointment(string usuario)
        {
            // 🚧 Acá va la lógica para confirmar el turno (ej: update en BD)
            TempData["Message"] = $"El turno de {usuario} fue confirmado.";

            return RedirectToAction("ManageAppointments");
        }

        // POST: /Admin/CancelAppointment
        [HttpPost]
        public IActionResult CancelAppointment(string usuario)
        {
            // 🚧 Acá va la lógica para cancelar el turno (ej: update en BD)
            TempData["Message"] = $"El turno de {usuario} fue cancelado.";
            return RedirectToAction("ManageAppointments");
        }
        public IActionResult ManageLog()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("User");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Login", "Account");

                var logs = new List<LogViewModel>
{
    new LogViewModel
    {
        Fecha = DateTime.Now.AddMinutes(-10),
        Usuario = "admin",
        Accion = "Inicio de sesión",
        Descripcion = "El usuario admin inició sesión exitosamente.",
        Nivel = "Info"
    },
    new LogViewModel
    {
        Fecha = DateTime.Now.AddHours(-1),
        Usuario = "jlopez",
        Accion = "Actualización de perfil",
        Descripcion = "El usuario jlopez actualizó su dirección de correo.",
        Nivel = "Info"
    },
    new LogViewModel
    {
        Fecha = DateTime.Now.AddDays(-1),
        Usuario = "admin",
        Accion = "Error al guardar turno",
        Descripcion = "Excepción lanzada al intentar guardar un turno. Detalles: NullReferenceException.",
        Nivel = "Error"
    },
    new LogViewModel
    {
        Fecha = DateTime.Now.AddMinutes(-30),
        Usuario = "mgarcia",
        Accion = "Intento de acceso no autorizado",
        Descripcion = "El usuario mgarcia intentó acceder a una sección restringida.",
        Nivel = "Warning"
    }
};

                return View(logs);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los vehículos: " + ex.Message;
                return View(new List<VehiculoViewModel>());
            }
        }
    }
}