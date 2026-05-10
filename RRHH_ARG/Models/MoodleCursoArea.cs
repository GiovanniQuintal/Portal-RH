namespace RRHH_ARG.Models
{
    public class MoodleCursoArea
    {
        public int McId { get; set; }
        public int AreId { get; set; }

        public virtual MoodleCurso Curso { get; set; } = null!;
        public virtual Area Area { get; set; } = null!;
    }
}