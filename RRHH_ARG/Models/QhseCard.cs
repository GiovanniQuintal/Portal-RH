using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models
{
    [Table("QHSE_CARD")]
    public class QhseCard
    {
        [Key]
        [Column("QHC_ID")]
        public int QhcId { get; set; }

        [Column("EMP_ID")]
        public int EmpId { get; set; }

        [ForeignKey("EmpId")]
        public virtual Empleado Empleado { get; set; }

        [Column("QHC_OBSERVADOR")]
        public string ObservadorNombre { get; set; }

        [Column("QHC_FECHA")]
        public DateTime Fecha { get; set; }

        [Column("QHC_LUGAR")]
        public string Lugar { get; set; }

        [Column("QHC_ES_INSEGURO")]
        public bool EsInseguro { get; set; }

        [Column("QHC_TODO_SEGURO")]
        public bool TodoSeguro { get; set; }

        [Column("QHC_DESC_OBSERVACION")]
        public string? DescripcionObservacion { get; set; }

        [Column("QHC_DESC_CAUSA")]
        public string? DescripcionCausa { get; set; }

        [Column("QHC_ACCION_CORRECTIVA")]
        public string? AccionCorrectiva { get; set; }

        [Column("QHC_RESPONSABLE_CIERRE")]
        public string? ResponsableCierre { get; set; }

        [Column("QHC_FECHA_COMPROMISO")]
        public DateTime? FechaCompromiso { get; set; }

        [Column("QHC_FECHA_REGISTRO")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // --- NUEVOS CAMPOS PARA ASIGNACIÓN Y EVIDENCIA ---
        [Column("QHC_ASIGNADO_EMP_ID")]
        public int? AsignadoEmpId { get; set; }

        // ---> AGREGA ESTA LÍNEA <---
        [ForeignKey("AsignadoEmpId")]
        public virtual Empleado? EmpleadoAsignado { get; set; }

        [Column("QHC_EVIDENCIA_TEXTO")]
        public string? EvidenciaTexto { get; set; }

        [Column("QHC_EVIDENCIA_FOTO")]
        public string? EvidenciaFoto { get; set; } // Guardará la ruta del archivo o string en base64

        public virtual ICollection<QhseCardDetail> Detalles { get; set; } = new List<QhseCardDetail>();

        // --- CAMBIO IMPORTANTE ---
        // BORRA la propiedad: public bool RevisadoPorQHSE { get; set; }

        // AGREGA ESTA:
        [Column("QHC_ESTATUS")]
        public int Estatus { get; set; }

        // Propiedades de ayuda para la vista (no van a la BD)
        [NotMapped]
        public string EstatusTexto => Estatus switch
        {
            0 => "Nuevo",
            1 => "Asignado",
            2 => "Resuelto",
            _ => "Desconocido"
        };

        [NotMapped]
        public string EstatusColor => Estatus switch
        {
            0 => "bg-danger",
            1 => "bg-warning text-dark",
            2 => "bg-success",
            _ => "bg-secondary"
        };
    }
}