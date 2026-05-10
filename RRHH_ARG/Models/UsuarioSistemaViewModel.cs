namespace RRHH_ARG.Models
{
    public class UsuarioSistemaViewModel
    {
        public string UserId { get; set; }
        public string EmpleadoNombreCompleto { get; set; } // PER_NOMBRE + PATERNO
        public string Email { get; set; }
        public string CurrentRole { get; set; } // El rol actual
        public bool EsPrimerIngreso { get; set; } // Para saber si ya cambió la contraseña

        // Propiedad auxiliar para manejar el cambio en el select antes de confirmar
        public string SelectedRoleDraft { get; set; }
    }
}
