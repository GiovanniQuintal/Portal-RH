using System;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Model
{
    public class Aplicante
    {
        public int Id { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellidos { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;

        [Required]
        public DateTime FechaNacimiento { get; set; } = DateTime.Now.AddYears(-18);

        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Empleo { get; set; } = string.Empty;
        public string Subsidiaria { get; set; } = string.Empty;

        // Estatus de Reclutamiento (Candidato / Rechazado)
        public string Status { get; set; } = string.Empty;

        // Tipo de contrato
        public string TipoEmpleado { get; set; } = "Empleado";

        // (Tus otros campos, como Fechas de Entrevista, pueden ir aquí)
        public DateTime? FechaInicioEntrevista { get; set; }
        public DateTime? FechaTerminoEntrevista { get; set; }

        // --- SOLUCIÓN: Propiedad para rastrear promoción ---
        public bool IsPromoted { get; set; } = false;
    }
}