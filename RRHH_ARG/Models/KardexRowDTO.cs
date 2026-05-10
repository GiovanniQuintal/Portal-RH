namespace RRHH_ARG.Models
{
    public class KardexRowDTO
    {
        public int KardexId { get; set; }
        public int Aniversario { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal Derecho { get; set; }
        public decimal Tomados { get; set; }
        public decimal Saldo => Derecho - Tomados;
        public DateTime FechaInicio { get; set; } // <--- ¡AGREGA ESTA LÍNEA!
        public DateTime FechaCaducidad { get; set; }
        public bool IsVigente { get; set; }
        public List<KardexDetalleDTO> Detalles { get; set; } = new();
    }

    public class KardexDetalleDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal DiasDescontados { get; set; }
        public DateTime FechaRegistro { get; set; }

        public int? IncidenciaId { get; set; }
    }

    public class KardexResultDTO
    {
        public List<KardexRowDTO> Filas { get; set; } = new();
        public int AjusteInicial { get; set; } // En entero, como acordamos
    }
}
