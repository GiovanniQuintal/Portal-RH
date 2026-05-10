namespace RRHH_ARG.Models

{
    // Para el ComboBox del Supervisor
    public class SubordinadoDTO
    {
        public int MoodleId { get; set; }
        public int EmpId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;

        // Propiedad auxiliar para mostrar en el Combo: "Juan Perez (Sistemas)"
        public string DisplayText => $"{NombreCompleto} ({Area})";
    }

    // Para la Grid (Tabla) de Cursos
    public class CursoResumenDto
    {
        public string Codigo { get; set; } = string.Empty;      // MC_CODIGO
        public int MoodleId { get; set; }
        public string Nombre { get; set; } = string.Empty;      // MC_NOMBRE

        public string Departamento { get; set; } = string.Empty; // ARE_NOMBRE (Del curso o del empleado, usaremos el del curso)
        public DateTime? FechaInicio { get; set; }              // MC_FECHA_INICIO
        public DateTime? FechaFin { get; set; }                 // MC_FECHA_FIN (Expiración)
        public string Tipo { get; set; } = string.Empty;        // "Obligatorio" o "Libre"
        public decimal Avance { get; set; }                     // MCE_PORCENTAJE_AVANCE
        public string Estado { get; set; } = string.Empty;      // Activo/Completado/Expirado
        public string ClaseEstado { get; set; } = string.Empty; // Para colorear el badge (opcional)
    }
}
