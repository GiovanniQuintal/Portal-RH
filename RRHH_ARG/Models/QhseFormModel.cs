using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Models
{
    public class QhseFormModel
    {
        public bool EsInseguro { get; set; } = true;
        public bool TodoSeguro { get; set; } = true;

        [Required(ErrorMessage = "El nombre del observador es obligatorio")]
        public string Observador { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El lugar del evento es obligatorio")]
        public string Lugar { get; set; }

        public Dictionary<string, bool> Posiciones { get; set; } = new()
        {
            { "Golpear contra objetos", false },
            { "Ser golpeado por objetos", false },
            { "Quedar atrapado entre o dentro de objetos", false },
            { "Caidas", false },
            { "Contacto con temperaturas extremas", false },
            { "Contacto con corriente electrica", false },
            { "Inhalacion de sustancia peligrosa", false },
            { "Absorcion de sustancia peligrosa", false },
            { "Ingestion de sustancia peligrosa", false },
            { "Sobre esfuerzo", false },
            { "Movimiento repetitivo", false }
        };

        public Dictionary<string, bool> Epp { get; set; } = new()
        {
            { "Cabeza", false },
            { "Ojos y cara", false },
            { "Oidos", false },
            { "Aparato respiratorio", false },
            { "Brazos y manos", false },
            { "Tronco", false },
            { "Piernas y pies", false }
        };

        public Dictionary<string, bool> Procedimientos { get; set; } = new()
        {
            { "No se conocen ni se entienden", false },
            { "Inadecuado", false },
            { "No se cumplen", false }
        };

        public Dictionary<string, bool> Herramientas { get; set; } = new()
        {
            { "Inadecuados para el trabajo", false },
            { "En condicion insegura", false },
            { "Usados en forma incorrecta", false }
        };

        [Required(ErrorMessage = "Describe la observación")]
        public string DescripcionObservacion { get; set; }

        [Required(ErrorMessage = "Describe la causa")]
        public string DescripcionCausa { get; set; }

        [Required(ErrorMessage = "Indica la acción correctiva")]
        public string AccionCorrectiva { get; set; }

        [Required(ErrorMessage = "El responsable es obligatorio")]
        public string Responsable { get; set; }

        public DateTime? FechaCompromiso { get; set; }
    }
}
