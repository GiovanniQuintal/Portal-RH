using System;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Model
{
    public class Incidencia
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado (Paso 1).")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El tipo de incidencia es obligatorio.")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [MinLength(10, ErrorMessage = "La descripción debe tener al menos 10 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaIncidencia { get; set; } = DateTime.Now;

        // --- INICIO DE LA SOLUCIÓN ---
        // Nuevo campo para el documento adjunto (opcional)
        public string? NombreArchivo { get; set; }
        // --- FIN DE LA SOLUCIÓN ---
    }
}