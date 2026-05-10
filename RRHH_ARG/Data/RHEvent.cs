using System;
using System.ComponentModel.DataAnnotations;

namespace RRHH_ARG.Data
{

    public class RHEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public EventType Type { get; set; }
        public string IconPath { get; set; }
        public int ReferenceId { get; set; }

        public RHEvent(string title, DateTime date, EventType type, string iconPath, int referenceId)
        {
            Title = title;
            Date = date;
            Type = type;
            IconPath = iconPath;
            ReferenceId = referenceId;
        }

        public enum EventType
        {
            ENTREVISTA,
            EXPEDIENTE_VENCIMIENTO,
            CUMPLEAÑOS,
            DIA_FESTIVO
        }
    }
}