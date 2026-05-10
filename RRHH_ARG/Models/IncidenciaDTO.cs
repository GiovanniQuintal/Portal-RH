using System;

namespace RRHH_ARG.Models
{
    public class IncidenciaDTO
    {
        // --- Identificadores (Ocultos en la vista, vitales para la lógica) ---
        public int IncidenciaId { get; set; } // ID de la tabla INCIDENCIA
        public int EmpleadoId { get; set; }   // ID del Empleado seleccionado

        // --- Datos de Lectura (Para mostrar en la tabla) ---
        // El servicio llenará esto haciendo Joins
        public string NombreEmpleado { get; set; } = string.Empty;
        public string AreaEmpleado { get; set; } = string.Empty;

        // --- Datos de Escritura/Lectura (Formulario y Tabla) ---
        // Estos datos viajan desde el Modal hacia la BD
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? NombreArchivo { get; set; }

        // --- NUEVO CAMPO TEMPORAL ---
        // Usamos esto para llevar el archivo del Modal al Servicio
        public byte[]? ArchivoBytes { get; set; }
        public DateTime FechaIncidencia { get; set; } = DateTime.Now;
        public decimal Cantidad { get; set; } = 1; // <--- NUEVO

        public int TipoId { get; set; } // <--- NUEVO: Para guardar el ID desde el ComboBox
    }
}