using AvanzadaWeb.Models;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AvanzadaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private static Dictionary<string, string> _recoveryCodes = new Dictionary<string, string>();

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Si ya está logueado, redirigir al home
            if (HttpContext.Session.GetString("User") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener todos los usuarios para buscar por email
                    var usuarios = await _apiService.GetAsync<List<UsuarioViewModel>>("usuarios");
                    var usuario = usuarios.FirstOrDefault(u => u.Email == model.Email);

                    if (usuario != null)
                    {

                        // Crear objeto de sesión
                        var sessionUser = new SessionUser
                        {
                            IDUsuario = usuario.IDUsuario,
                            Email = usuario.Email,
                            Nombre = usuario.Nombre,
                            Apellido = usuario.Apellido,
                            NivelUsuario = usuario.NivelDescripcion,
                            Foto = usuario.Foto
                        };

                        // Guardar en sesión
                        HttpContext.Session.SetString("User", JsonSerializer.Serialize(sessionUser));

                        return RedirectToAction("Dashboard", "Usuarios");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Usuario no encontrado");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al iniciar sesión: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User");
            return RedirectToAction("Login");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Crear el objeto usuario para enviar a la API
                    var usuario = new
                    {
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        ContraseñaString = model.Password,
                        IDNivel = 1 // Cliente por defecto
                    };

                    var resultado = await _apiService.PostAsync<object>("usuarios", usuario);

                    // registro exitoso
                    TempData["MensajeExito"] = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Manejar errores específicos de la API
                    if (ex.Message.Contains("El email ya está registrado"))
                    {
                        ModelState.AddModelError("Email", "El email ya está registrado");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error en el registro: " + ex.Message);
                    }
                }
            }

            // algo falló, mostrar el formulario nuevamente
            return View(model);
        }

        // GET: Account/Recover
        public IActionResult Recover()
        {
            // Pasa un nuevo modelo vacío a la vista
            return View(new RecoverViewModel());
        }

        // POST: Account/Recover
        [HttpPost]
        public async Task<IActionResult> Recover(RecoverViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Si ya se envió el código, verificar y actualizar contraseña
                    if (model.CodeSent && !string.IsNullOrEmpty(model.VerificationCode))
                    {
                        // Verificar el código
                        if (_recoveryCodes.ContainsKey(model.Email) &&
                            _recoveryCodes[model.Email] == model.VerificationCode)
                        {
                            // Llamar a la API para actualizar la contraseña
                            var updateRequest = new
                            {
                                Email = model.Email,
                                NewPassword = model.NewPassword
                            };

                            await _apiService.PostAsync<object>("usuarios/update-password", updateRequest);

                            TempData["MensajeExito"] = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";
                            return RedirectToAction("Login");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Código de verificación incorrecto");
                            model.CodeSent = true;
                            return View(model);
                        }
                    }
                    else
                    {
                        // Verificar si el email existe
                        var usuarios = await _apiService.GetAsync<List<UsuarioViewModel>>("usuarios");
                        var usuario = usuarios.FirstOrDefault(u => u.Email == model.Email);

                        if (usuario != null)
                        {
                            // Generar código de verificación con un randomizer
                            var random = new Random();
                            var code = random.Next(100000, 999999).ToString();

                            // Guardar el código
                            _recoveryCodes[model.Email] = code;

                            // Mostrar el código en pantalla
                            TempData["VerificationCode"] = code;

                            model.CodeSent = true;
                            return View(model);
                        }
                        else
                        {
                            ModelState.AddModelError("", "No existe una cuenta con este email");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error en el proceso de recuperación: " + ex.Message);
                }
            }

            return View(model);
        }
    }
}