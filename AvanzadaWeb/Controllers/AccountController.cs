using AvanzadaWeb.Models;
using AvanzadaWeb.Services;
using AvanzadaWeb.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UsuarioLoginDto = AvanzadaWeb.Models.UsuarioLoginDto;

namespace AvanzadaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiService apiService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
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
                    var loginRequest = new
                    {
                        Email = model.Email,
                        Password = model.Password
                    };

                    var response = await _apiService.PostAsync<UsuarioLoginDto>("usuarios/login", loginRequest);

                    var sessionUser = new SessionUser
                    {
                        IDUsuario = response.IDUsuario,
                        Email = response.Email,
                        Nombre = response.Nombre,
                        Apellido = response.Apellido,
                        RolNombre = response.RolNombre ?? "Cliente",
                        UrlDefault = response.RolNombre == "Admin" ? "/Usuarios/Dashboard" : "/Usuarios/Profile",
                        Foto = response.Foto != null ? Convert.ToBase64String(response.Foto) : null
                    };

                    HttpContext.Session.SetString("User", System.Text.Json.JsonSerializer.Serialize(sessionUser));

                    TempData["SuccessMessage"] = $"Bienvenido {response.Nombre} {response.Apellido}";

                    return Redirect(sessionUser.UrlDefault);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Credenciales inválidas. Por favor, intente nuevamente.");
                    return View(model);
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
                    var usuario = new
                    {
                        Nombre = model.Nombre,
                        Apellido = model.Apellido,
                        Email = model.Email,
                        Telefono = model.Telefono,
                        ContraseñaString = model.Password,
                        IDNivel = 1
                    };

                    var resultado = await _apiService.PostAsync<object>("usuarios", usuario);

                    TempData["MensajeExito"] = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
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

            return View(model);
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(PasswordRecoveryRequest model, string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "El email es requerido";
                return View();
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                TempData["ErrorMessage"] = "El formato del email no es válido";
                return View();
            }

            try
            {
                var request = new { Email = email };
                var response = await _apiService.PostAsync<object>("usuarios/forgot-password", request);

                TempData["SuccessMessage"] = "Si el email existe, se han enviado instrucciones de recuperación a tu correo.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ForgotPassword para email: {Email}", email);

                // Por seguridad, mostrar el mismo mensaje aunque falle
                TempData["SuccessMessage"] = "Si el email existe, se han enviado instrucciones de recuperación a tu correo.";
                return RedirectToAction("Login");
            }
        }

        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Token inválido.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Primero validar el token
                    var validateRequest = new { Token = model.Token };
                    var validationResponse = await _apiService.PostAsync<object>("usuarios/validate-reset-token", validateRequest);

                    // Si la validación es exitosa, proceder con el reset
                    var resetRequest = new
                    {
                        Token = model.Token,
                        NewPassword = model.NewPassword
                    };

                    await _apiService.PostAsync<object>("usuarios/reset-password", resetRequest);

                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Token inválido") || ex.Message.Contains("expirado"))
                    {
                        ModelState.AddModelError("", "El enlace de recuperación ha expirado o es inválido. Por favor, solicita uno nuevo.");
                        return RedirectToAction("ForgotPassword");
                    }

                    ModelState.AddModelError("", "Error al restablecer la contraseña: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Account/Recover
        public IActionResult Recover()
        {
            return RedirectToAction("ForgotPassword");
        }
    }
}