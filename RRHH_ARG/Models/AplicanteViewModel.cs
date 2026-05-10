using System;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models
{
    public class AplicanteViewModel
    {
        // IDs
        public int AplId { get; set; }
        public int PerId { get; set; }

        // Datos de Persona
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [StringLength(100)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [StringLength(100)]
        public string ApellidoMaterno { get; set; } = string.Empty;

        public string Apellidos => $"{ApellidoPaterno} {ApellidoMaterno}".Trim();

        [Required(ErrorMessage = "El género es obligatorio")]
        public string Genero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string Telefono { get; set; } = string.Empty;

        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaRegistro { get; set; }

        // Datos relacionados
        public string Area { get; set; } = string.Empty;
        public string Empleo { get; set; } = string.Empty;
        public string Subsidiaria { get; set; } = string.Empty;

        // Datos de Aplicante
        public bool AplStatus { get; set; }
        public string Status => AplStatus ? "Candidato" : "Rechazado";

        public DateTime? FechaInicioEntrevista { get; set; }
        public DateTime? FechaTerminoEntrevista { get; set; }
        public bool IsPromoted { get; set; }

        public string TipoEmpleado { get; set; } = string.Empty;
        public string? AplCv { get; set; }
    }
}