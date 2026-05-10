using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Models;

namespace RRHH_ARG.Models
{
    public class CardNode : SvgNodeModel
    {
        public new string Puesto { get; }
        // Cambiamos Title por una Lista
        public List<string> Nombres { get; }

        public CardNode(string puesto, List<string> nombres, Point? position = null) : base(position)
        {
            Puesto = puesto;
            Nombres = nombres;
        }
    }
}