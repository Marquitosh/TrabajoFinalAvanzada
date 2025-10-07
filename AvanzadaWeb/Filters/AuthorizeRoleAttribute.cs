using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AvanzadaWeb.Models;
using System.Text.Json;

namespace AvanzadaWeb.Filters
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userJson = context.HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userJson))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var sessionUser = JsonSerializer.Deserialize<SessionUser>(userJson);

            if (sessionUser == null || !_allowedRoles.Contains(sessionUser.RolNombre))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}