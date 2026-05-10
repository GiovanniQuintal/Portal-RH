using System;

namespace RRHH_ARG.Models
{
    public class EmpleadoDashboardItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string Area { get; set; }

        // Propiedad calculada para saber si tiene alertas
        public bool TieneTarjetasSinRevisar { get; set; }
    }
}
