using System;

namespace RRHH_ARG.Models
{
    public class EmpleadoViewModel
    {
        public int Id { get; set; }
        public int PerId { get; set; }
        public string CodigoEmpleado { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }

        // Propiedad visual combinada
        public string Apellidos => $"{ApellidoPaterno} {ApellidoMaterno}".Trim();

        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaContratacion { get; set; }
        public DateTime? FechaBaja { get; set; }
        public string Genero { get; set; }

        public string Area { get; set; }
        public string Empleo { get; set; }      // Cargo (Analista, etc.)

        public string? Empleo2 { get; set; }         // Puesto Secundario
        public string? Area2 { get; set; }
        public string EmpleosCombinados => string.IsNullOrEmpty(Empleo2) ? Empleo : $"{Empleo} / {Empleo2}";
        public string Subsidiaria { get; set; }
        public string? SupervisorNombre { get; set; }


        // --- LÓGICA DE ESTATUS Y TIPO ---

        // 1. IDs para Binding en los Selects (Edición)
        public int StaId { get; set; } // ID de la tabla Status (Activo/Inactivo)
        public int TipId { get; set; } // ID de la tabla TipoAplicante (Becario/Empleado)

        // 2. Textos Reales traídos de BD
        public string EstatusReal { get; set; } // Texto "Activo", "Inactivo"
        public string TipoReal { get; set; }    // Texto "Becario", "Practicante"

        // --- ASIGNACION DE NUEVO ROL ---
        public bool AsignarRolSupervisor { get; set; } // Para el nuevo combo
        public bool TieneSubordinados { get; set; }
        // --- NUEVO: DATOS DE EMERGENCIA ---
        public List<ContactoEmergenciaDTO> ContactosEmergencia { get; set; } = new List<ContactoEmergenciaDTO>();

        // --- NUEVO: DATOS MÉDICOS ---
        public int ImeId { get; set; } // Para saber si editar o crear
        public string? ImeTipoSangre { get; set; }
        public string? ImeAlergias { get; set; }
        public string? ImePadecimientos { get; set; }
        public string? ImeObservaciones { get; set; }

        public decimal? SaeMensual { get; set; }
        public decimal? SaeQuincenal { get; set; }
        public decimal? SaeSdi { get; set; }

        public string? Banco { get; set; }
        public string? CuentaBancaria { get; set; }

        public List<BeneficioEmpleadoDTO> Beneficios { get; set; } = new();

        // 3. PROPIEDAD INTELIGENTE PARA LA TABLA
        public string Status
        {
            get
            {
                // Si el estatus sistema es Inactivo, ignoramos el tipo y mostramos Inactivo
                if (!string.IsNullOrEmpty(EstatusReal) && EstatusReal.Trim().Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    return "Inactivo";
                }

                // Si está activo, mostramos su tipo (ej. Becario)
                return !string.IsNullOrEmpty(TipoReal) ? TipoReal : "Empleado";
            }
            set { /* Necesario para serialización */ }
        }

        // --- Métricas ---
        public int ExpedientesTotales { get; set; }
        public int ExpedientesCompletos { get; set; }
        public int ExpedientesPorcentaje =>
            ExpedientesTotales == 0 ? 0 : (int)((double)ExpedientesCompletos / ExpedientesTotales * 100);

        public int? SupId { get; set; }

        public string RolSeleccionado { get; set; }
    }
    public class ContactoEmergenciaDTO
    {
        public int CemId { get; set; }
        public string CemNombreCompleto { get; set; } = "";
        public string CemParentesco { get; set; } = "";
        public string CemTelefono1 { get; set; } = "";
        public string CemTelefono2 { get; set; } = "";
    }

    public class BeneficioEmpleadoDTO
    {
        public int ConId { get; set; }
        public string ConceptoNombre { get; set; }
        public string Clasificacion { get; set; } // <--- NUEVA LÍNEA
        public decimal MontoQ1 { get; set; }
        public decimal MontoQ2 { get; set; }
    }
}