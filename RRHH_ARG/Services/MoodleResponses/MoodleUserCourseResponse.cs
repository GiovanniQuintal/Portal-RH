using System.Text.Json.Serialization;

namespace RRHH_ARG.Services.MoodleResponses
{
    // Respuesta de core_enrol_get_users_courses
    public class MoodleUserCourseResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } // ID del curso en Moodle

        [JsonPropertyName("shortname")]
        public string Shortname { get; set; }

        [JsonPropertyName("fullname")]
        public string Fullname { get; set; }

        [JsonPropertyName("progress")]
        public double? Progress { get; set; } // % de avance (si está habilitado en Moodle)

        [JsonPropertyName("completed")]
        public bool? Completed { get; set; }

        [JsonPropertyName("startdate")]
        public long Startdate { get; set; }

        [JsonPropertyName("enddate")]
        public long Enddate { get; set; }

        // Opcional: Para saber si está oculto
        [JsonPropertyName("visible")]
        public int Visible { get; set; }
    }
}