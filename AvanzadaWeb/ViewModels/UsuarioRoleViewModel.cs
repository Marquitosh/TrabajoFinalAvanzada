using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.ViewModels
{
    public class UsuarioRoleViewModel
    {
        public int IDUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RolActual { get; set; } = string.Empty;
        public int IDRolActual { get; set; }
        public int NuevoIDRol { get; set; }
    }

    public class GestionRolesViewModel
    {
        public List<UsuarioRoleViewModel> Usuarios { get; set; } = new();
        public List<NivelUsuarioViewModel> RolesDisponibles { get; set; } = new();
    }

    public class NivelUsuarioViewModel
    {
        public int IDNivel { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string RolNombre { get; set; } = string.Empty;
    }
}
