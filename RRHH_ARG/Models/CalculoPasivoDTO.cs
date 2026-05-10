namespace RRHH_ARG.Models
{
    public class CalculoPasivoDTO
    {
        public int EmpleadoId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime FechaBaja { get; set; }

        // Salarios
        public decimal SalarioDiario { get; set; }
        public decimal SalarioDiarioIntegrado { get; set; } // El calculado en memoria (sdiReal)

        // --- FINIQUITO (Derechos irrenunciables) ---
        public decimal DiasSueldoPendiente { get; set; }
        public decimal MontoSueldoPendiente { get; set; }

        public decimal DiasAguinaldoProporcional { get; set; }
        public decimal MontoAguinaldo { get; set; }

        public decimal DiasVacacionesPendientes { get; set; }
        public decimal MontoVacaciones { get; set; }

        public decimal MontoPrimaVacacional { get; set; }

        // --- LIQUIDACIÓN (Indemnizaciones) ---
        public decimal AniosCompletosTrabajados { get; set; }
        public decimal MontoPrimaAntiguedad { get; set; }

        public decimal MontoIndemnizacion90Dias { get; set; }
        public decimal MontoIndemnizacion20Dias { get; set; }

        // --- TOTALES ---
        public decimal TotalFiniquito => MontoAguinaldo + MontoVacaciones + MontoPrimaVacacional + MontoSueldoPendiente;
        public decimal TotalLiquidacion => MontoPrimaAntiguedad + MontoIndemnizacion90Dias + MontoIndemnizacion20Dias;
        public decimal GranTotal => TotalFiniquito + TotalLiquidacion;
    }
}