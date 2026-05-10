namespace RRHH_ARG.Models
{
    public class VacacionSolicitudDTO
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaInicial { get; set; } = DateTime.Now;
        public DateTime FechaFinal { get; set; } = DateTime.Now;
        public DateTime? FechaPago { get; set; }
        public decimal DiasDescanso { get; set; } = 0; // Séptimos y festivos
        public decimal DiasVacaciones { get; set; }    // Resultado calculado
        public decimal SaldoDisponible { get; set; }   // Para validación
        public decimal PrimaVacacionalEstimada { get; set; }
    }
}