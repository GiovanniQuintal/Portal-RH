using System;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Model
{
    public class Empleado
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellidos { get; set; } = string.Empty;

        public string Genero { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;

        public string Empleo { get; set; } = string.Empty;

        public string Subsidiaria { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        public DateTime FechaNacimiento { get; set; }

        public DateTime FechaContratacion { get; set; }

        // Propiedades para la barra de progreso de expedientes
        public int ExpedientesCompletos { get; set; }
        public int ExpedientesTotales { get; set; }
        public int ExpedientesPorcentaje => ExpedientesTotales > 0 ? (int)((double)ExpedientesCompletos / ExpedientesTotales * 100) : 0;
    }
}