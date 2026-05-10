using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components.DesignTokens;
using RRHH_ARG.Models;

namespace RRHH_ARG.Data // Ajusta el namespace si es necesario
{
    // 1. Heredamos de IdentityDbContext (Lo tuyo)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- TUS DBSETS ---
        public virtual DbSet<Aplicante> Aplicante { get; set; }
        public virtual DbSet<Area> Area { get; set; }
        public virtual DbSet<Curso> Cursos { get; set; }
        public virtual DbSet<DetalleExpediente> DetalleExpediente { get; set; }
        public virtual DbSet<DetalleIncidencium> DetalleIncidencium { get; set; } // O DetalleIncidencia
        public virtual DbSet<Empleado> Empleados { get; set; }
        public virtual DbSet<Entrenamiento> Entrenamientos { get; set; }
        public virtual DbSet<EntrenamientoStatus> EntrenamientoStatuses { get; set; }
        public virtual DbSet<Epp> Epps { get; set; }
        public virtual DbSet<Evento> Eventos { get; set; }
        public virtual DbSet<Expediente> Expediente { get; set; }
        public virtual DbSet<HistorialEntrenamiento> HistorialEntrenamientos { get; set; }
        public virtual DbSet<HistorialEpp> HistorialEpps { get; set; }
        public virtual DbSet<HistorialExpediente> HistorialExpedientes { get; set; }
        public virtual DbSet<HistorialIncidencium> HistorialIncidencium { get; set; } // O HistorialIncidencia
        public virtual DbSet<Incidencium> Incidencium { get; set; } // O Incidencia
        public virtual DbSet<PerfilPuesto> PerfilPuesto { get; set; }
        public virtual DbSet<Persona> Personas { get; set; }
        public virtual DbSet<Rol> Rols { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Subsidiarium> Subsidiaria { get; set; }
        public virtual DbSet<Supervisor> Supervisors { get; set; }
        public virtual DbSet<TipoAplicante> TipoAplicante { get; set; }
        public virtual DbSet<TipoCurso> TipoCursos { get; set; }
        public virtual DbSet<TipoEpp> TipoEpps { get; set; }
        public virtual DbSet<TipoEvento> TipoEventos { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<EntregaEpp> EntregaEpp { get; set; }
        public virtual DbSet<DetalleEntregaEpp> DetalleEntregaEpp { get; set; }
        public virtual DbSet<InformacionMedica> InformacionMedicas { get; set; }
        public virtual DbSet<ContactoEmergencia> ContactosEmergencias { get; set; }
        public virtual DbSet<MoodleCurso> MoodleCursos { get; set; }
        public virtual DbSet<MoodleCursoEmpleado> MoodleCursoEmpleados { get; set; }
        public virtual DbSet<DetalleExpedienteUnico> DetalleExpedienteUnico { get; set; }

        public virtual DbSet<MoodleCursoArea> MoodleCursoAreas { get; set; }
        // 1. Prioridades y Touchpoints (Base)
        public virtual DbSet<PdPriority> PdPriorities { get; set; }
        public virtual DbSet<PdTouchpoint> PdTouchpoints { get; set; }

        // 2. Objetivos Globales
        public virtual DbSet<PdGlobalGoal> PdGlobalGoals { get; set; }

        // 3. NUEVA CONFIGURACIÓN DE AÑO Y REVISIONES (R1, R2...)
        // Eliminamos "PdConfig" y agregamos estos dos:
        public virtual DbSet<PdYearConfig> PdYearConfigs { get; set; }
        public virtual DbSet<PdRevisionDate> PdRevisionDates { get; set; }
        public virtual DbSet<PdTouchpointItem> PdTouchpointItems { get; set; }
        public virtual DbSet<QhseCard> QhseCards { get; set; }
        public virtual DbSet<QhseCardDetail> QhseCardDetails { get; set; }
        public virtual DbSet<PdIndicator> PdIndicators { get; set; }
        
        public virtual DbSet<TipoIncidencia> TipoIncidencia { get; set; }
        
        public virtual DbSet<KardexVacacion> KardexVacaciones { get; set; }
        public virtual DbSet<VacacionTomada> VacacionesTomadas { get; set; }
        public virtual DbSet<ParametroAnual> ParametrosAnuales { get; set; }
        public virtual DbSet<ProyeccionPasivo> ProyeccionesPasivo { get; set; }

        public virtual DbSet<EmpleadoBeneficio> EmpleadoBeneficios { get; set; }

        public DbSet<ExpedienteExclusion> ExpedienteExclusiones { get; set; }

        // MÓDULO DE NÓMINA (Nuevos)
        public DbSet<Nomina> Nominas { get; set; }
        public DbSet<NominaConceptos> NominaConceptos { get; set; }
        public DbSet<NominaEmpleado> NominaEmpleados { get; set; }
        public DbSet<NominaEmpleadoDetalle> NominaEmpleadoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ========================================================
            // PARTE 1: CONFIGURACI�N DE IDENTITY (LOGIN)
            // ========================================================
            base.OnModelCreating(modelBuilder);

            // ========================================================
            // PARTE 2: CORRECCIONES DE ERRORES (TUS IGNORES)
            // ========================================================

            // Evitar error "Invalid column ApplicationUserId"
            modelBuilder.Entity<Empleado>().Ignore("ApplicationUserId");
            modelBuilder.Entity<Empleado>().Ignore("ApplicationUserId1");

            // Evitar error "Invalid column HexId" (Porque la columna aun no existe en SQL)



            modelBuilder.Entity<RRHH_ARG.Models.DetalleExpedienteUnico>(entity =>
            {
                entity.HasKey(e => e.DeuId); // <--- ESTO ES LO QUE TE FALTA
                entity.ToTable("DETALLE_EXPEDIENTE_UNICO");
            });


            // ========================================================
            // PARTE 3: EL MAPEO DE TU COMPA�ERO (EL DICCIONARIO)
            // ========================================================

            // Mapeo de INCIDENCIA
            modelBuilder.Entity<Incidencium>(entity =>
            {
                entity.ToTable("INCIDENCIA");
                entity.HasKey(e => e.IncId);
                entity.Property(e => e.IncId).HasColumnName("INC_ID");
                entity.Property(e => e.DinId).HasColumnName("DIN_ID");
                entity.Property(e => e.IncFecha).HasColumnType("datetime").HasColumnName("INC_FECHA");

                // Relaci�n
                entity.HasOne(d => d.Din).WithMany(p => p.Incidencia)
                    .HasForeignKey(d => d.DinId).HasConstraintName("FK_INCIDENCIA_DETALLE_INCIDENCIA");
            });

            // Mapeo de DETALLE_INCIDENCIA
            modelBuilder.Entity<DetalleIncidencium>(entity =>
            {
                entity.ToTable("DETALLE_INCIDENCIA");
                entity.HasKey(e => e.DinId);
                entity.Property(e => e.DinId).HasColumnName("DIN_ID");
                entity.Property(e => e.DinArchivo).HasMaxLength(255).HasColumnName("DIN_ARCHIVO");
                //entity.Property(e => e.DinTipo).HasMaxLength(100).HasColumnName("DIN_TIPO");
                entity.Property(e => e.DinDescripcion).HasMaxLength(500).HasColumnName("DIN_DESCRIPCION");
            });

            // Mapeo de HISTORIAL_INCIDENCIA
            modelBuilder.Entity<HistorialIncidencium>(entity =>
            {
                entity.ToTable("HISTORIAL_INCIDENCIA");
                entity.HasKey(e => e.HinId);
                entity.Property(e => e.HinId).HasColumnName("HIN_ID");
                entity.Property(e => e.IncId).HasColumnName("INC_ID");

                entity.Property(e => e.EmpId).HasColumnName("EMP_ID");

                entity.HasOne(d => d.Inc).WithMany(p => p.HistorialIncidencia)
                    .HasForeignKey(d => d.IncId).HasConstraintName("FK_HISTORIAL_INCIDENCIA_INCIDENCIA1");

                entity.HasOne(d => d.Empleado).WithMany(p => p.HistorialIncidencia).HasForeignKey(d => d.EmpId).HasConstraintName("FK_HISTORIAL_INCIDENCIA_EMPLEADO");
            });

            // Mapeo de PERSONA (Para arreglar PerNombre, etc.)
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("PERSONA");
                entity.HasKey(e => e.PerId);
                entity.Property(e => e.PerId).HasColumnName("PER_ID");
                entity.Property(e => e.PerNombre).HasColumnName("PER_NOMBRE");
                entity.Property(e => e.PerApellidoPaterno).HasColumnName("PER_APELLIDO_PATERNO");
                entity.Property(e => e.PerApellidoMaterno).HasColumnName("PER_APELLIDO_MATERNO");
                entity.Property(e => e.PerEmail).HasColumnName("PER_EMAIL");
                entity.Property(e => e.PepId).HasColumnName("PEP_ID");
                entity.Property(e => e.SubId).HasColumnName("SUB_ID");
                entity.Property(e => e.PerTelefono).HasColumnName("PER_TELEFONO");
                entity.Property(e => e.PerGenero).HasColumnName("PER_GENERO");
                entity.Property(e => e.PerFechaNacimiento).HasColumnName("PER_FECHA_NACIMIENTO");
                entity.Property(e => e.PerFechaRegistro).HasColumnName("PER_FECHA_REGISTRO");
            });

            // Mapeo de AREA (Para arreglar AreNombre)
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("AREA");
                entity.HasKey(e => e.AreId);
                entity.Property(e => e.AreId).HasColumnName("ARE_ID");
                entity.Property(e => e.AreNombre).HasColumnName("ARE_NOMBRE");
            });

            // Mapeo de PERFIL_PUESTO
            modelBuilder.Entity<PerfilPuesto>(entity =>
            {
                entity.ToTable("PERFIL_PUESTO");
                entity.HasKey(e => e.PepId);
                entity.Property(e => e.PepId).HasColumnName("PEP_ID");
                entity.Property(e => e.PepNombre).HasColumnName("PEP_NOMBRE");
                entity.Property(e => e.AreId).HasColumnName("ARE_ID");
            });

            // Mapeo de SUBSIDIARIA
            modelBuilder.Entity<Subsidiarium>(entity =>
            {
                entity.ToTable("SUBSIDIARIA");
                entity.HasKey(e => e.SubId);
                entity.Property(e => e.SubId).HasColumnName("SUB_ID");
                entity.Property(e => e.SubNombre).HasColumnName("SUB_NOMBRE");
            });

            // Mapeo de EMPLEADO (Solo lo b�sico que existe hoy)
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.ToTable("EMPLEADO");
                entity.HasKey(e => e.EmpId);
                entity.Property(e => e.EmpId).HasColumnName("EMP_ID");
                entity.Property(e => e.PerId).HasColumnName("PER_ID");
                //entity.Property(e => e.HinId).HasColumnName("HIN_ID");
                entity.Property(e => e.HenId).HasColumnName("HEN_ID");
                entity.Property(e => e.HepId).HasColumnName("HEP_ID");

                entity.HasOne(e => e.Usuario)            // "Un Empleado tiene un Usuario..."
              .WithMany()                        // "...el Usuario puede estar en muchos lados (o WithOne si es exclusivo)"
              .HasForeignKey(e => e.AspNetUserId) // "Se conectan mediante esta columna en Empleado"
              .HasPrincipalKey(u => u.Id)         // "Que apunta al Id de la tabla AspNetUsers"
              .HasConstraintName("FK_EMPLEADO_ASPNETUSERS"); // Nombre para la BD
                //entity.Property(e => e.HinId).HasColumnName("HIN_ID");
                // Mapea las otras columnas que SI existen (StaId, SupId, UsuId, Fechas...)
                // NO mapees HexId, HenId, HepId aqu� porque ya los ignoramos arriba.
                entity.HasOne(e => e.SupervisorDirecto)
        .WithMany(e => e.Subordinados)
        .HasForeignKey(e => e.SupId)
        .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Aplicante>(entity =>
            {
                entity.ToTable("APLICANTE"); // Asumo que la tabla se llama as�
                entity.HasKey(e => e.AplId);
                entity.Property(e => e.AplId).HasColumnName("APL_ID");
                entity.Property(e => e.PerId).HasColumnName("PER_ID");
                entity.Property(e => e.AplFechaInicioEntrevista).HasColumnName("APL_FECHA_INICIO_ENTREVISTA");
                entity.Property(e => e.TipId).HasColumnName("TIP_ID"); // Para saber el puesto

                // Relaciones
                entity.HasOne(d => d.TipoAplicante).WithMany(p => p.Aplicantes)
                    .HasForeignKey(d => d.TipId).HasConstraintName("FK_APLICANTE_TIPO_APLICANTE");

                // Asumo relaci�n con Persona para saber el nombre del aplicante
                // Si no tienes esta propiedad de navegaci�n en Aplicante.cs, agr�gala:
                // public virtual Persona? Per { get; set; }
                // Y luego:
                // entity.HasOne(d => d.Per).WithMany().HasForeignKey(d => d.PerId);
            });

            // 3. Mapeo de EXPEDIENTE (Asegurar fecha caducidad)
            modelBuilder.Entity<Expediente>(entity =>
            {
                entity.ToTable("EXPEDIENTE");
                entity.HasKey(e => e.ExpId);
                entity.Property(e => e.ExpId).HasColumnName("EXP_ID");
                entity.Property(e => e.ExpFechacaducidad).HasColumnName("EXP_FECHACADUCIDAD");
                entity.Property(e => e.DexId).HasColumnName("DEX_ID");
                entity.Property(e => e.EmpId).HasColumnName("EMP_ID");

                // Relaciones necesarias para los Joins
                entity.HasOne(d => d.Empleado).WithMany(p => p.Expedientes)
                    .HasForeignKey(d => d.EmpId);

                entity.HasOne(d => d.Dex).WithMany(p => p.Expedientes)
                    .HasForeignKey(d => d.DexId);
            });

            // Dentro de OnModelCreating...

            modelBuilder.Entity<DetalleExpediente>(entity =>
            {
                // 1. Nombre real de la tabla en SQL
                entity.ToTable("DETALLE_EXPEDIENTE");

                // 2. Llave primaria
                entity.HasKey(e => e.DexId);

                // 3. Mapeo de columnas (Asumiendo que usan guiones bajos DEX_...)
                entity.Property(e => e.DexId).HasColumnName("DEX_ID");


                entity.Property(e => e.DexNombre).HasColumnName("DEX_NOMBRE");
                entity.Property(e => e.DexNomenclatura).HasColumnName("DEX_NOMENCLATURA");
            });

            modelBuilder.Entity<Status>(entity =>
            {
                // 1. Aqu� le decimos que la tabla real se llama "STATUS"
                entity.ToTable("STATUS");

                // 2. Mapeamos la llave primaria
                entity.HasKey(e => e.StaId);
                entity.Property(e => e.StaId).HasColumnName("STA_ID");

                // 3. Mapeamos la columna del nombre/texto
                // Aseg�rate de que en tu BD la columna se llame as�. 
                // Si en SQL se llama "STA_NOMBRE" o "STA_DESCRIPCION", cambia lo que est� entre comillas.
                entity.Property(e => e.StaTipoStatus).HasColumnName("STA_TIPO_STATUS");
            });

            // Mapeo de DETALLE_ENTREGA_EPP (Corrección completa)
            modelBuilder.Entity<DetalleEntregaEpp>(entity =>
            {
                entity.ToTable("DETALLE_ENTREGA_EPP");
                entity.HasKey(e => e.DetId);

                // Mapeo de columnas
                entity.Property(e => e.DetId)
                      .HasColumnName("DET_ID")
                      .ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EntId).HasColumnName("ENT_ID");
                entity.Property(e => e.TepId).HasColumnName("TEP_ID");
                entity.Property(e => e.DetCantidad).HasColumnName("DET_CANTIDAD");

                // CLAVE: Relación con EntregaEpp (Cascada para que borre detalles al borrar entrega)
                entity.HasOne(d => d.Entrega)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.EntId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_DETALLE_ENTREGA_EPP_ENTREGA_EPP");

                // CLAVE: Relación con TipoEpp (SIN WithMany vacío, especificamos la colección)
                entity.HasOne(d => d.TipoEpp)
                      .WithMany(t => t.DetallesEntrega) // Apunta a la colección que agregamos en TipoEpp
                      .HasForeignKey(d => d.TepId)
                      .OnDelete(DeleteBehavior.Restrict) // No borrar tipo si tiene entregas
                      .HasConstraintName("FK_DETALLE_ENTREGA_EPP_TIPO_EPP");
            });

            // Mapeo de ENTREGA_EPP (Asegúrate que también está correcto)
            modelBuilder.Entity<EntregaEpp>(entity =>
            {
                entity.ToTable("ENTREGA_EPP");
                entity.HasKey(e => e.EntId);

                entity.Property(e => e.EntId)
                      .HasColumnName("ENT_ID")
                      .ValueGeneratedOnAdd(); // Auto-incremento

                entity.Property(e => e.EmpId).HasColumnName("EMP_ID");
                entity.Property(e => e.EntFecha).HasColumnName("ENT_FECHA");
                entity.Property(e => e.EntEvidencia)
                      .HasColumnName("ENT_EVIDENCIA")
                      .HasColumnType("varbinary(max)"); // Especificar tipo binario

                // Relación con Empleado
                entity.HasOne(d => d.Empleado)
                      .WithMany()
                      .HasForeignKey(d => d.EmpId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_ENTREGA_EPP_EMPLEADO");
            });

            // Mapeo de TIPO_EPP (Actualizado con la nueva colección)
            modelBuilder.Entity<TipoEpp>(entity =>
            {
                entity.ToTable("TIPO_EPP");
                entity.HasKey(e => e.TepId);

                entity.Property(e => e.TepId).HasColumnName("TEP_ID");
                entity.Property(e => e.TepTipo)
                      .HasColumnName("TEP_TIPO")
                      .HasMaxLength(100); // Opcional: especificar tamaño

                // Relación original con Epps (si la usas)
                entity.HasMany(t => t.Epps)
                      .WithOne(e => e.Tep)
                      .HasForeignKey(e => e.TepId)
                      .OnDelete(DeleteBehavior.Restrict);

                // NUEVA: Relación con DetallesEntrega (la que agregamos)
                entity.HasMany(t => t.DetallesEntrega)
                      .WithOne(d => d.TipoEpp)
                      .HasForeignKey(d => d.TepId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TipoCurso>(entity =>
            {
                // 1. Indicar el nombre real de la tabla en SQL
                entity.ToTable("TIPO_CURSO");

                // 2. Indicar la llave primaria
                entity.HasKey(e => e.TicId);

                // 3. Mapear las columnas
                entity.Property(e => e.TicId).HasColumnName("TIC_ID");
                entity.Property(e => e.TicNombre).HasColumnName("TIC_NOMBRE");
            });

            // Mapeo de MOODLE_CURSO
            modelBuilder.Entity<MoodleCurso>(entity =>
            {
                entity.ToTable("MOODLE_CURSO");
                entity.HasKey(e => e.McId);

                entity.Property(e => e.McId).HasColumnName("MC_ID");
                entity.Property(e => e.MoodleCursoId).HasColumnName("MOODLE_CURSO_ID");
                entity.Property(e => e.McNombre).HasMaxLength(200).HasColumnName("MC_NOMBRE");
                entity.Property(e => e.McDescripcion).HasColumnName("MC_DESCRIPCION");
                entity.Property(e => e.McFechaInicio).HasColumnType("datetime").HasColumnName("MC_FECHA_INICIO");
                entity.Property(e => e.McFechaFin).HasColumnType("datetime").HasColumnName("MC_FECHA_FIN");
                entity.Property(e => e.McEstado).HasColumnName("MC_ESTADO");
                entity.Property(e => e.TicId).HasColumnName("TIC_ID");
                entity.Property(e => e.McFechaSync).HasColumnType("datetime").HasColumnName("MC_FECHA_SYNC");

                // Columnas agregadas con ALTER TABLE
                entity.Property(e => e.McCodigo).HasMaxLength(50).HasColumnName("MC_CODIGO");
                entity.Property(e => e.McEsPublico).HasColumnName("MC_ES_PUBLICO");

                // Relación con TipoCurso
                entity.HasOne(d => d.TipoCurso)
                    .WithMany() // Si TipoCurso no tiene colección de MoodleCursos, déjalo vacío
                    .HasForeignKey(d => d.TicId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_MOODLE_CURSO_TIPO");
            });

            // Mapeo de MOODLE_CURSO_AREA (Tabla Intermedia)
            modelBuilder.Entity<MoodleCursoArea>(entity =>
            {
                entity.ToTable("MOODLE_CURSO_AREA");
                entity.HasKey(e => new { e.McId, e.AreId }); // Llave compuesta

                entity.Property(e => e.McId).HasColumnName("MC_ID");
                entity.Property(e => e.AreId).HasColumnName("ARE_ID");

                entity.HasOne(d => d.Curso)
                    .WithMany(p => p.MoodleCursoAreas)
                    .HasForeignKey(d => d.McId)
                    .HasConstraintName("FK_MCA_CURSO");

                entity.HasOne(d => d.Area)
                    .WithMany() // Asumo que Area no tiene la lista de cursos mapeada inversamente aun
                    .HasForeignKey(d => d.AreId)
                    .HasConstraintName("FK_MCA_AREA");
            });

            // Mapeo de MOODLE_CURSO_EMPLEADO (Tabla de progreso)
            modelBuilder.Entity<MoodleCursoEmpleado>(entity =>
            {
                entity.ToTable("MOODLE_CURSO_EMPLEADO");
                entity.HasKey(e => new { e.EmpId, e.McId }); // Llave compuesta

                entity.Property(e => e.EmpId).HasColumnName("EMP_ID");
                entity.Property(e => e.McId).HasColumnName("MC_ID");
                entity.Property(e => e.MceFechaAsignacion).HasColumnType("datetime").HasColumnName("MCE_FECHA_ASIGNACION");
                entity.Property(e => e.MceFechaInicio).HasColumnType("datetime").HasColumnName("MCE_FECHA_INICIO");
                entity.Property(e => e.MceFechaFin).HasColumnType("datetime").HasColumnName("MCE_FECHA_FIN");
                entity.Property(e => e.McePorcentajeAvance).HasColumnType("decimal(5, 2)").HasColumnName("MCE_PORCENTAJE_AVANCE");
                entity.Property(e => e.MceEstado).HasColumnName("MCE_ESTADO");
                entity.Property(e => e.MceFechaUltimoAcceso).HasColumnType("datetime").HasColumnName("MCE_FECHA_ULTIMO_ACCESO");
                entity.Property(e => e.MceFechaSync).HasColumnType("datetime").HasColumnName("MCE_FECHA_SYNC");

                entity.HasOne(d => d.Empleado)
                    .WithMany() // Puedes agregar: .WithMany(p => p.CursosMoodle) en Empleado si quieres
                    .HasForeignKey(d => d.EmpId)
                    .HasConstraintName("FK_MCE_EMPLEADO");

                entity.HasOne(d => d.Curso)
                    .WithMany(p => p.MoodleCursoEmpleados)
                    .HasForeignKey(d => d.McId)
                    .HasConstraintName("FK_MCE_CURSO");
            });

            modelBuilder.Entity<QhseCard>(entity =>
            {
                entity.ToTable("QHSE_CARD");
                entity.HasKey(e => e.QhcId);
                // La relación con Empleado
                entity.HasOne(d => d.Empleado)
                      .WithMany() // Si quieres lista en empleado: .WithMany(p => p.TarjetasQhse)
                      .HasForeignKey(d => d.EmpId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MAPEO QHSE_CARD_DETAIL
            modelBuilder.Entity<QhseCardDetail>(entity =>
            {
                entity.ToTable("QHSE_CARD_DETAIL");
                entity.HasKey(e => e.QcdId);

                // Relación cascada: Si borro la tarjeta, se borran sus items
                entity.HasOne(d => d.Card)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(d => d.QhcId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PdIndicator>(entity =>
            {
                entity.HasOne(d => d.GlobalGoal)
                      .WithMany(p => p.Indicators)
                      .HasForeignKey(d => d.GlobalGoalId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_INDICATOR_GLOBALGOAL");
            });

            modelBuilder.Entity<PdPriority>(entity =>
            {
                entity.HasOne(d => d.Indicator)
                      .WithMany(p => p.Priorities)
                      .HasForeignKey(d => d.IndicatorId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_PRIORITY_INDICATOR");
            });

            // ========================================================
            // PARTE 4: MAPEO DEL MÓDULO DE NÓMINA (PAYROLL)
            // ========================================================

            

            //modelBuilder.Entity<TipoIncidencia>(entity =>
            //{
                //entity.ToTable("TIPO_INCIDENCIA");
                //entity.HasKey(e => e.TIN_ID);
                //entity.Property(e => e.TIN_ID).HasColumnName("TIN_ID");
               // entity.Property(e => e.TIN_NOMBRE).HasColumnName("TIN_NOMBRE");
               // entity.Property(e => e.CON_ID).HasColumnName("CON_ID");

               // entity.HasOne(d => d.ConceptoNomina)
                //      .WithMany(p => p.TiposIncidencia)
                //      .HasForeignKey(d => d.CON_ID)
                //      .HasConstraintName("FK_TIPO_INC_CONCEPTO");
          //  //);

            

            modelBuilder.Entity<EmpleadoBeneficio>()
        .HasKey(eb => new { eb.EmpId, eb.ConId });
        }
    }
}