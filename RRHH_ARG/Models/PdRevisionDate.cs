using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("PD_REVISION_DATES")]
    public class PdRevisionDate
    {
        [Key] public int Id { get; set; }
        public int YearConfigId { get; set; }
        public int RevisionNumber { get; set; } // 1, 2, 3, 4
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLockedByRH { get; set; }

        // --- ESTO FALTABA ---
        [ForeignKey("YearConfigId")]
        public virtual PdYearConfig? YearConfig { get; set; }

        [NotMapped]
        public bool IsActiveNow => DateTime.Now >= StartDate && DateTime.Now <= EndDate && !IsLockedByRH;
    }
}