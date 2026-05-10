using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("Nomina")]
    public class Nomina
    {
        [Key]
        public int NominaId { get; set; }

        [Required]
        [StringLength(10)]
        public string Periodo { get; set; }

        public int Anno { get; set; }

        public int Mes { get; set; }

        public DateTime PeriodoInicio { get; set; }

        public DateTime PeriodoFin { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "ABIERTA";

        public bool Bloqueado { get; set; }

        public decimal TotalSueldosNetos { get; set; }

        // Navegación (Relación 1 a Muchos con los recibos)
        public virtual ICollection<NominaEmpleado> NominaEmpleados { get; set; } = new List<NominaEmpleado>();
    }
}