using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models // <--- ¡ASEGÚRATE DE QUE ESTA LÍNEA EXISTA!
{
    [Table("PD_YEAR_CONFIG")]
    public class PdYearConfig
    {
        [Key]
        public int Id { get; set; }
        public int Year { get; set; }
        public int RevisionCount { get; set; } // 2 o 4
        public bool IsActive { get; set; }

        // Relación con las fechas
        public virtual ICollection<PdRevisionDate> RevisionDates { get; set; } = new List<PdRevisionDate>();
    }
}