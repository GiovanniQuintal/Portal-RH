using RRHH_ARG.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class Empleado
{
    [Key]
    [Column("EMP_ID")]
    public int EmpId { get; set; }

    public int? TipId { get; set; }

    [ForeignKey("TipId")]
    public virtual TipoAplicante Tipo { get; set; }

    [Column("PER_ID")]
    public int? PerId { get; set; }

    [Column("ASP_NET_USER_ID")]
    public string? AspNetUserId { get; set; }

    [Column("STA_ID")]
    public int? StaId { get; set; }

    // --- AQUÍ EMPIEZA LA MAGIA DEL AUTO-REFERENCIADO ---

    // 1. La Clave Foránea (Ya la tenías, se queda igual)
    [Column("SUP_ID")]
    public int? SupId { get; set; }
    [Column("EMP_CODIGO")]
    public string? EmpCodigo { get; set; }

    // 2. EL JEFE (Padre)
    // Antes tenías: public virtual Supervisor? Sup
    // AHORA: Apunta a 'Empleado'. Le cambié el nombre a 'SupervisorDirecto' para ser más claro, 
    // pero apunta a la misma clase en la que estás.
    [ForeignKey("SupId")]
    public virtual Empleado? SupervisorDirecto { get; set; }

    // 3. LOS SUBORDINADOS (Hijos)
    // Esta es la lista de empleados que tienen a ESTE empleado como su jefe.
    // [InverseProperty] le dice a EF: "Esta lista es la contraparte de la relación 'SupervisorDirecto'"
    [InverseProperty("SupervisorDirecto")]
    public virtual ICollection<Empleado> Subordinados { get; set; } = new List<Empleado>();

    // ----------------------------------------------------

    [Column("EMP_FECHA_REGISTRO")]
    public DateTime? EmpFechaRegistro { get; set; }

    [Column("EMP_FECHA_CONTRATACION")]
    public DateTime? EmpFechaContratacion { get; set; }

    [Column("EMP_FECHA_BAJA")]
    public DateTime? EmpFechaBaja { get; set; }

    [Column("HEN_ID")]
    public int? HenId { get; set; }

    [Column("HEP_ID")]
    public int? HepId { get; set; }

    public decimal? SAE_MENSUAL { get; set; }
    public decimal? SAE_QUINCENAL { get; set; }
    public decimal? SAE_SDI { get; set; }
    [Column("EMP_BANCO")]
    public string? Banco { get; set; }

    [Column("EMP_CUENTA")]
    public string? CuentaBancaria { get; set; }


    [Column("vac_tomadas_iniciales")]
    public int VacTomadasIniciales { get; set; } = 0;

    // --- Propiedades de Navegación Restantes ---

    public virtual ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();

    [ForeignKey("HenId")]
    public virtual HistorialEntrenamiento? Hen { get; set; }

    [ForeignKey("HepId")]
    public virtual HistorialEpp? Hep { get; set; }

    [ForeignKey("StaId")]
    public virtual Status? Sta { get; set; }

    // ELIMINA O COMENTA ESTO: 
    // Ya no necesitas la clase 'Supervisor', ahora usamos la relación de arriba.
    // [ForeignKey("SupId")]
    // public virtual Supervisor? Sup { get; set; }

    [ForeignKey("PerId")]
    public virtual Persona? Per { get; set; }

    public virtual ICollection<DetalleExpedienteUnico> DetalleExpedienteUnicos { get; set; } = new List<DetalleExpedienteUnico>();
    public virtual ICollection<HistorialIncidencium> HistorialIncidencia { get; set; } = new List<HistorialIncidencium>();

    [ForeignKey("AspNetUserId")]
    public virtual ApplicationUser? Usuario { get; set; }

    public virtual ICollection<NominaEmpleado> NominaEmpleados { get; set; } = new List<NominaEmpleado>();
}