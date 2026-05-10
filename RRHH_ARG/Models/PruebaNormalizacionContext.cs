using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RRHH_ARG.Models;

public partial class PruebaNormalizacionContext : DbContext
{
    public PruebaNormalizacionContext()
    {
    }

    public PruebaNormalizacionContext(DbContextOptions<PruebaNormalizacionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aplicante> Aplicantes { get; set; }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Curso> Cursos { get; set; }

    public virtual DbSet<DetalleExpediente> DetalleExpedientes { get; set; }

    public virtual DbSet<DetalleIncidencium> DetalleIncidencia { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Entrenamiento> Entrenamientos { get; set; }

    public virtual DbSet<EntrenamientoStatus> EntrenamientoStatuses { get; set; }

    public virtual DbSet<Epp> Epps { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Expediente> Expedientes { get; set; }

    public virtual DbSet<HistorialEntrenamiento> HistorialEntrenamientos { get; set; }

    public virtual DbSet<HistorialEpp> HistorialEpps { get; set; }

    

    public virtual DbSet<HistorialIncidencium> HistorialIncidencia { get; set; }

    public virtual DbSet<Incidencium> Incidencia { get; set; }

    public virtual DbSet<PerfilPuesto> PerfilPuestos { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Subsidiarium> Subsidiaria { get; set; }

    public virtual DbSet<Supervisor> Supervisors { get; set; }

    public virtual DbSet<TipoCurso> TipoCursos { get; set; }

    public virtual DbSet<TipoEpp> TipoEpps { get; set; }

    public virtual DbSet<TipoEvento> TipoEventos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=SERVER\\JBSQL;Initial Catalog=PRUEBA_NORMALIZACION;Persist Security Info=True;User ID=Developer;Password=Argoses2025;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aplicante>(entity =>
        {
            entity.HasKey(e => e.AplId);

            entity.ToTable("APLICANTE");

            entity.Property(e => e.AplId)
                .ValueGeneratedNever()
                .HasColumnName("APL_ID");
            entity.Property(e => e.AplCv)
                .HasMaxLength(255)
                .HasColumnName("APL_CV");
            entity.Property(e => e.AplFechaInicioEntrevista)
                .HasColumnType("datetime")
                .HasColumnName("APL_FECHA_INICIO_ENTREVISTA");
            entity.Property(e => e.AplFechaTerminoEntrevista)
                .HasColumnType("datetime")
                .HasColumnName("APL_FECHA_TERMINO_ENTREVISTA");
            entity.Property(e => e.AplIspromoted).HasColumnName("APL_ISPROMOTED");
            entity.Property(e => e.AplStatus).HasColumnName("APL_STATUS");
            entity.Property(e => e.PerId).HasColumnName("PER_ID");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreId);

            entity.ToTable("AREA");

            entity.Property(e => e.AreId)
                .ValueGeneratedNever()
                .HasColumnName("ARE_ID");
            entity.Property(e => e.AreNombre)
                .HasMaxLength(100)
                .HasColumnName("ARE_NOMBRE");
        });

        modelBuilder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.CurId);

            entity.ToTable("CURSO");

            entity.Property(e => e.CurId)
                .ValueGeneratedNever()
                .HasColumnName("CUR_ID");
            entity.Property(e => e.CurDescripcion).HasColumnName("CUR_DESCRIPCION");
            entity.Property(e => e.CurObligatorio).HasColumnName("CUR_OBLIGATORIO");
            entity.Property(e => e.CurVigencia)
                .HasColumnType("datetime")
                .HasColumnName("CUR_VIGENCIA");
            entity.Property(e => e.TicId).HasColumnName("TIC_ID");

            entity.HasOne(d => d.Tic).WithMany(p => p.Cursos)
                .HasForeignKey(d => d.TicId)
                .HasConstraintName("FK_CURSO_TIPO_CURSO");
        });

        modelBuilder.Entity<DetalleExpediente>(entity =>
        {
            entity.HasKey(e => e.DexId);

            entity.ToTable("DETALLE_EXPEDIENTE");

            entity.Property(e => e.DexId)
                .ValueGeneratedNever()
                .HasColumnName("DEX_ID");
            entity.Property(e => e.DexNombre)
                .HasMaxLength(100)
                .HasColumnName("DEX_NOMBRE");
            entity.Property(e => e.DexNomenclatura)
                .HasMaxLength(100)
                .HasColumnName("DEX_NOMENCLATURA");
        });

        modelBuilder.Entity<DetalleIncidencium>(entity =>
        {
            entity.HasKey(e => e.DinId);

            entity.ToTable("DETALLE_INCIDENCIA");

            entity.Property(e => e.DinId)
                .ValueGeneratedNever()
                .HasColumnName("DIN_ID");
            entity.Property(e => e.DinArchivo)
                .HasMaxLength(255)
                .HasColumnName("DIN_ARCHIVO");
            //entity.Property(e => e.DinTipo)
             //   .HasMaxLength(100)
              //  .HasColumnName("DIN_TIPO");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.EmpId);

            entity.ToTable("EMPLEADO");

            entity.Property(e => e.EmpId)
                .ValueGeneratedNever()
                .HasColumnName("EMP_ID");
            entity.Property(e => e.EmpFechaContratacion)
                .HasColumnType("datetime")
                .HasColumnName("EMP_FECHA_CONTRATACION");
            entity.Property(e => e.EmpFechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("EMP_FECHA_REGISTRO");
            entity.Property(e => e.HenId).HasColumnName("HEN_ID");
            entity.Property(e => e.HepId).HasColumnName("HEP_ID");
            
            entity.Property(e => e.PerId).HasColumnName("PER_ID");
            entity.Property(e => e.StaId).HasColumnName("STA_ID");
            entity.Property(e => e.SupId).HasColumnName("SUP_ID");
            

            entity.HasOne(d => d.Hen).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.HenId)
                .HasConstraintName("FK_EMPLEADO_HISTORIAL_ENTRENAMIENTO");

            entity.HasOne(d => d.Hep).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.HepId)
                .HasConstraintName("FK_EMPLEADO_HISTORIAL_EPP");
    

            entity.HasOne(d => d.Sta).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.StaId)
                .HasConstraintName("FK_EMPLEADO_STATUS");

           

        modelBuilder.Entity<Entrenamiento>(entity =>
        {
            entity.HasKey(e => e.EntId);

            entity.ToTable("ENTRENAMIENTO");

            entity.Property(e => e.EntId)
                .ValueGeneratedNever()
                .HasColumnName("ENT_ID");
            entity.Property(e => e.CurId).HasColumnName("CUR_ID");
            entity.Property(e => e.EnsId).HasColumnName("ENS_ID");
            entity.Property(e => e.EntFechaasignacion)
                .HasColumnType("datetime")
                .HasColumnName("ENT_FECHAASIGNACION");
            entity.Property(e => e.EntFechacompletacion)
                .HasColumnType("datetime")
                .HasColumnName("ENT_FECHACOMPLETACION");
            entity.Property(e => e.EntFechaexpiracion)
                .HasColumnType("datetime")
                .HasColumnName("ENT_FECHAEXPIRACION");

            entity.HasOne(d => d.Cur).WithMany(p => p.Entrenamientos)
                .HasForeignKey(d => d.CurId)
                .HasConstraintName("FK_ENTRENAMIENTO_CURSO");

            entity.HasOne(d => d.Ens).WithMany(p => p.Entrenamientos)
                .HasForeignKey(d => d.EnsId)
                .HasConstraintName("FK_ENTRENAMIENTO_ENTRENAMIENTO_STATUS");
        });

        modelBuilder.Entity<EntrenamientoStatus>(entity =>
        {
            entity.HasKey(e => e.EnsId);

            entity.ToTable("ENTRENAMIENTO_STATUS");

            entity.Property(e => e.EnsId)
                .ValueGeneratedNever()
                .HasColumnName("ENS_ID");
            entity.Property(e => e.EnsStatus)
                .HasMaxLength(100)
                .HasColumnName("ENS_STATUS");
        });

        modelBuilder.Entity<Epp>(entity =>
        {
            entity.ToTable("EPP");

            entity.Property(e => e.EppId)
                .ValueGeneratedNever()
                .HasColumnName("EPP_ID");
            entity.Property(e => e.EppCantidad).HasColumnName("EPP_CANTIDAD");
            entity.Property(e => e.EppFecha)
                .HasColumnType("datetime")
                .HasColumnName("EPP_FECHA");
            entity.Property(e => e.TepId).HasColumnName("TEP_ID");

            entity.HasOne(d => d.Tep).WithMany(p => p.Epps)
                .HasForeignKey(d => d.TepId)
                .HasConstraintName("FK_EPP_TIPO_EPP");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__EVENTOS__241A510CE2D5F0E3");

            entity.ToTable("EVENTOS");

            entity.Property(e => e.EventId).HasColumnName("EVENT_ID");
            entity.Property(e => e.EventDate).HasColumnName("EVENT_DATE");
            entity.Property(e => e.NameEvent)
                .HasMaxLength(255)
                .HasColumnName("NAME_EVENT");
            entity.Property(e => e.TipoEventoId).HasColumnName("TIPO_EVENTO_ID");

            entity.HasOne(d => d.TipoEvento).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.TipoEventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EVENTOS_TIPO_EVENTO");
        });

        modelBuilder.Entity<Expediente>(entity =>
        {
            entity.HasKey(e => e.ExpId);

            entity.ToTable("EXPEDIENTE");

            entity.Property(e => e.ExpId)
                .ValueGeneratedNever()
                .HasColumnName("EXP_ID");
            entity.Property(e => e.DexId).HasColumnName("DEX_ID");
            entity.Property(e => e.ExpFechacaducidad)
                .HasColumnType("datetime")
                .HasColumnName("EXP_FECHACADUCIDAD");
            entity.Property(e => e.ExpFechasubido)
                .HasColumnType("datetime")
                .HasColumnName("EXP_FECHASUBIDO");

            entity.HasOne(d => d.Dex).WithMany(p => p.Expedientes)
                .HasForeignKey(d => d.DexId)
                .HasConstraintName("FK_EXPEDIENTE_DETALLE_EXPEDIENTE");
        });

        modelBuilder.Entity<HistorialEntrenamiento>(entity =>
        {
            entity.HasKey(e => e.HenId);

            entity.ToTable("HISTORIAL_ENTRENAMIENTO");

            entity.Property(e => e.HenId)
                .ValueGeneratedNever()
                .HasColumnName("HEN_ID");
            entity.Property(e => e.EntId).HasColumnName("ENT_ID");

            entity.HasOne(d => d.Ent).WithMany(p => p.HistorialEntrenamientos)
                .HasForeignKey(d => d.EntId)
                .HasConstraintName("FK_HISTORIAL_ENTRENAMIENTO_ENTRENAMIENTO");
        });

        modelBuilder.Entity<HistorialEpp>(entity =>
        {
            entity.HasKey(e => e.HepId);

            entity.ToTable("HISTORIAL_EPP");

            entity.Property(e => e.HepId)
                .ValueGeneratedNever()
                .HasColumnName("HEP_ID");
            entity.Property(e => e.EppId).HasColumnName("EPP_ID");

            entity.HasOne(d => d.Epp).WithMany(p => p.HistorialEpps)
                .HasForeignKey(d => d.EppId)
                .HasConstraintName("FK_HISTORIAL_EPP_EPP");
        });

        

            
        });

        modelBuilder.Entity<HistorialIncidencium>(entity =>
        {
            entity.HasKey(e => e.HinId);

            entity.ToTable("HISTORIAL_INCIDENCIA");

            entity.Property(e => e.HinId)
                .ValueGeneratedNever()
                .HasColumnName("HIN_ID");
            entity.Property(e => e.IncId).HasColumnName("INC_ID");

            entity.HasOne(d => d.Inc).WithMany(p => p.HistorialIncidencia)
                .HasForeignKey(d => d.IncId)
                .HasConstraintName("FK_HISTORIAL_INCIDENCIA_INCIDENCIA1");
        });

        modelBuilder.Entity<Incidencium>(entity =>
        {
            entity.HasKey(e => e.IncId);

            entity.ToTable("INCIDENCIA");

            entity.Property(e => e.IncId)
                .ValueGeneratedNever()
                .HasColumnName("INC_ID");
            entity.Property(e => e.DinId).HasColumnName("DIN_ID");
            entity.Property(e => e.IncFecha)
                .HasColumnType("datetime")
                .HasColumnName("INC_FECHA");

            entity.HasOne(d => d.Din).WithMany(p => p.Incidencia)
                .HasForeignKey(d => d.DinId)
                .HasConstraintName("FK_INCIDENCIA_DETALLE_INCIDENCIA");
        });

        modelBuilder.Entity<PerfilPuesto>(entity =>
        {
            entity.HasKey(e => e.PepId);

            entity.ToTable("PERFIL_PUESTO");

            entity.Property(e => e.PepId)
                .ValueGeneratedNever()
                .HasColumnName("PEP_ID");
            entity.Property(e => e.PepNombre)
                .HasMaxLength(100)
                .HasColumnName("PEP_NOMBRE");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.PerId);

            entity.ToTable("PERSONA");

            entity.Property(e => e.PerId)
                .ValueGeneratedNever()
                .HasColumnName("PER_ID");
            
            entity.Property(e => e.PepId).HasColumnName("PEP_ID");
            entity.Property(e => e.PerApellidoMaterno)
                .HasMaxLength(100)
                .HasColumnName("PER_APELLIDO_MATERNO");
            entity.Property(e => e.PerApellidoPaterno)
                .HasMaxLength(100)
                .HasColumnName("PER_APELLIDO_PATERNO");
            entity.Property(e => e.PerEmail)
                .HasMaxLength(150)
                .HasColumnName("PER_EMAIL");
            entity.Property(e => e.PerFechaNacimiento)
                .HasColumnType("datetime")
                .HasColumnName("PER_FECHA_NACIMIENTO");
            entity.Property(e => e.PerFechaRegistro)
                .HasColumnType("datetime")
                .HasColumnName("PER_FECHA_REGISTRO");
            entity.Property(e => e.PerGenero)
                .HasMaxLength(50)
                .HasColumnName("PER_GENERO");
            entity.Property(e => e.PerNombre)
                .HasMaxLength(100)
                .HasColumnName("PER_NOMBRE");
            entity.Property(e => e.PerTelefono)
                .HasMaxLength(20)
                .HasColumnName("PER_TELEFONO");
            entity.Property(e => e.SubId).HasColumnName("SUB_ID");

            entity.HasOne(d => d.Pep).WithMany(p => p.Personas)
                .HasForeignKey(d => d.PepId)
                .HasConstraintName("FK_PERSONA_PERFIL_PUESTO");

            entity.HasOne(d => d.Sub).WithMany(p => p.Personas)
                .HasForeignKey(d => d.SubId)
                .HasConstraintName("FK_PERSONA_SUBSIDIARIA");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("ROL");

            entity.Property(e => e.RolId)
                .ValueGeneratedNever()
                .HasColumnName("ROL_ID");
            entity.Property(e => e.RolTipo)
                .HasMaxLength(100)
                .HasColumnName("ROL_TIPO");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StaId);

            entity.ToTable("STATUS");

            entity.Property(e => e.StaId)
                .ValueGeneratedNever()
                .HasColumnName("STA_ID");
            entity.Property(e => e.StaTipoStatus)
                .HasMaxLength(100)
                .HasColumnName("STA_TIPO_STATUS");
        });

        modelBuilder.Entity<Subsidiarium>(entity =>
        {
            entity.HasKey(e => e.SubId);

            entity.ToTable("SUBSIDIARIA");

            entity.Property(e => e.SubId)
                .ValueGeneratedNever()
                .HasColumnName("SUB_ID");
            entity.Property(e => e.SubNombre)
                .HasMaxLength(100)
                .HasColumnName("SUB_NOMBRE");
        });

        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.HasKey(e => e.SupId);

            entity.ToTable("SUPERVISOR");

            entity.Property(e => e.SupId)
                .ValueGeneratedNever()
                .HasColumnName("SUP_ID");
            entity.Property(e => e.SupNombre)
                .HasMaxLength(150)
                .HasColumnName("SUP_NOMBRE");
        });

        modelBuilder.Entity<TipoCurso>(entity =>
        {
            entity.HasKey(e => e.TicId);

            entity.ToTable("TIPO_CURSO");

            entity.Property(e => e.TicId)
                .ValueGeneratedNever()
                .HasColumnName("TIC_ID");
            entity.Property(e => e.TicNombre)
                .HasMaxLength(100)
                .HasColumnName("TIC_NOMBRE");
        });

        modelBuilder.Entity<TipoEpp>(entity =>
        {
            entity.HasKey(e => e.TepId);

            entity.ToTable("TIPO_EPP");

            entity.Property(e => e.TepId)
                .ValueGeneratedNever()
                .HasColumnName("TEP_ID");
            entity.Property(e => e.TepTipo)
                .HasMaxLength(100)
                .HasColumnName("TEP_TIPO");
        });

        modelBuilder.Entity<TipoEvento>(entity =>
        {
            entity.HasKey(e => e.TipoEventoId).HasName("PK__TIPO_EVE__A7C0AD9945F447C8");

            entity.ToTable("TIPO_EVENTO");

            entity.Property(e => e.TipoEventoId).HasColumnName("TIPO_EVENTO_ID");
            entity.Property(e => e.Datasource)
                .HasMaxLength(100)
                .HasColumnName("DATASOURCE");
            entity.Property(e => e.Datecolumn)
                .HasMaxLength(100)
                .HasColumnName("DATECOLUMN");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("MODULE");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuId);

            entity.ToTable("USUARIO");

            entity.Property(e => e.UsuId)
                .ValueGeneratedNever()
                .HasColumnName("USU_ID");
            entity.Property(e => e.RolId).HasColumnName("ROL_ID");
            entity.Property(e => e.UsuCodigo)
                .HasMaxLength(50)
                .HasColumnName("USU_CODIGO");
            entity.Property(e => e.UsuContrasena)
                .HasMaxLength(255)
                .HasColumnName("USU_CONTRASENA");
            entity.Property(e => e.UsuUsuario)
                .HasMaxLength(100)
                .HasColumnName("USU_USUARIO");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
