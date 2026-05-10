using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RRHH_ARG.Models;

public partial class DetalleExpedienteUnico
{
    [Key] // <-- 2. AÑADE ESTA LÍNEA
    [Column("DEU_ID")]
    public int DeuId { get; set; }
    [Column("DEU_NOMENCLATURA")]
    public string DeuNomenclatura { get; set; } = null!;
    [Column("DEU_NOMBRE")]
    public string DeuNombre { get; set; } = null!;
    [Column("EMP_ID")]
    public int EmpId { get; set; }

    public virtual Empleado Emp { get; set; } = null!;

    public virtual ICollection<Expediente> Expedientes { get; set; } = new List<Expediente>();
}