using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("PD_PRIORITIES")]
    public class PdPriority
    {
        [Key]
        public int Id { get; set; }
        public int EmpId { get; set; }

        public int? IndicatorId { get; set; }
        [ForeignKey("IndicatorId")]
        public virtual PdIndicator? Indicator { get; set; }

        [Required]
        public string Title { get; set; } = ""; // Inicializar para evitar nulos

        // --- CORRECCIÓN: AGREGAR '?' PARA PERMITIR NULOS ---
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ActionPlan { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }

        // --- CORRECCIÓN: AGREGAR '?' ---
        public string? UnitOfMeasure { get; set; }
        public string? Notes { get; set; }

        public bool IsSubmitted { get; set; }
        public bool IsApproved { get; set; }

        public bool IsArchived { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool ReadyForReview { get; set; }

        // Resultado Final (Se llena al final del año o cierre)
        public string? FinalStatus { get; set; }

        [ForeignKey("EmpId")]
        public virtual Empleado? Empleado { get; set; }

        [NotMapped]
        public string? CustomerImpact
        {
            get => Description;
            set => Description = value;
        }

        // 1. AGREGA ESTE NUEVO CAMPO (MHB = Más es mejor, MAB = Menos es mejor)
        public string TrackingType { get; set; } = "MHB";

        // 2. REEMPLAZA tu ProgressPercentage actual por este:
        [NotMapped]
        public int ProgressPercentage
        {
            get
            {
                if (TrackingType == "MAB")
                {
                    // Lógica "Menos es Mejor" (Ej. Accidentes)
                    // Si la meta es 0 y lleva 0, está al 100%. Si ya superó la meta (ej. 1 accidente), cae a 0%.
                    if (ActualValue <= TargetValue) return 100;
                    return 0;
                }
                else
                {
                    // Lógica "Más es Mejor" (Ej. Tarjetas)
                    if (TargetValue == 0) return 0; // Evita error matemático

                    int progress = (int)((ActualValue / TargetValue) * 100);
                    return Math.Min(progress, 100); // Topado al 100%
                }
            }
        }

    }
}