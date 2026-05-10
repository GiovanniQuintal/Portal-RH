using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationEmpleadoHistorialIncidencia : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Agregar la nueva columna EMP_ID a HISTORIAL_INCIDENCIA
            // Basado en tu script, EMP_ID en EMPLEADO es [int] NOT NULL,
            // pero aquí la creamos nullable primero para poder insertar los datos.
            migrationBuilder.AddColumn<int>(
                name: "EMP_ID",
                table: "HISTORIAL_INCIDENCIA",
                type: "int",
                nullable: true);

            // 2. Crear Índice (Recomendado)
            migrationBuilder.CreateIndex(
                name: "IX_HISTORIAL_INCIDENCIA_EMP_ID",
                table: "HISTORIAL_INCIDENCIA",
                column: "EMP_ID");

            // 3. Agregar la NUEVA relación
            migrationBuilder.AddForeignKey(
                name: "FK_HISTORIAL_INCIDENCIA_EMPLEADO",
                table: "HISTORIAL_INCIDENCIA",
                column: "EMP_ID",
                principalTable: "EMPLEADO",
                principalColumn: "EMP_ID",
                onDelete: ReferentialAction.Restrict);

            // ============================================================
            // 4. MIGRACIÓN DE DATOS (CRÍTICO)
            // ============================================================
            migrationBuilder.Sql(
                @"UPDATE H
          SET H.EMP_ID = E.EMP_ID
          FROM HISTORIAL_INCIDENCIA H
          INNER JOIN EMPLEADO E ON E.HIN_ID = H.HIN_ID");

            // 5. Eliminar la relación VIEJA en EMPLEADO
            // IMPORTANTE: Si la consulta SQL que te di arriba NO arrojó resultados,
            // COMENTA O BORRA ESTE BLOQUE 'DropForeignKey'.
            //migrationBuilder.DropForeignKey(
              //  name: "FK_EMPLEADO_HISTORIAL_INCIDENCIA_HIN_ID", // <--- VERIFICA ESTE NOMBRE
                //table: "EMPLEADO");

            // 6. Eliminar el índice viejo en EMPLEADO (Si existe)
            // Si da error diciendo que no existe, puedes comentar esta línea.
            //migrationBuilder.DropIndex(
                //name: "IX_EMPLEADO_HIN_ID",
                //table: "EMPLEADO");

            // 7. Finalmente, borrar la columna HIN_ID de EMPLEADO
            migrationBuilder.DropColumn(
                name: "HIN_ID",
                table: "EMPLEADO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Aplicantes_TipoAplicantes_TipId",
                table: "Aplicantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Cursos_TipoCursos_TicId",
                table: "Cursos");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HistorialEntrenamientos_HEN_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HistorialEpps_HEP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_PERSONA_PER_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_Statuses_STA_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_Supervisors_SUP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_Usuarios_USU_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_Cursos_CurId",
                table: "Entrenamientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_EntrenamientoStatuses_EnsId",
                table: "Entrenamientos");

            migrationBuilder.DropForeignKey(
                name: "FK_Epps_TipoEpps_TepId",
                table: "Epps");

            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_TipoEventos_TipoEventoId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_DetalleExpedientes_DexId",
                table: "Expedientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_EMPLEADO_EmpId",
                table: "Expedientes");

            migrationBuilder.DropForeignKey(
                name: "FK_HISTORIAL_INCIDENCIA_EMPLEADO",
                table: "HISTORIAL_INCIDENCIA");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialEntrenamientos_Entrenamientos_EntId",
                table: "HistorialEntrenamientos");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialEpps_Epps_EppId",
                table: "HistorialEpps");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialExpedientes_Expedientes_ExpedienteExpId",
                table: "HistorialExpedientes");

            migrationBuilder.DropForeignKey(
                name: "FK_PERFIL_PUESTO_AREA_ARE_ID",
                table: "PERFIL_PUESTO");

            migrationBuilder.DropForeignKey(
                name: "FK_PERSONA_PERFIL_PUESTO_PEP_ID",
                table: "PERSONA");

            migrationBuilder.DropForeignKey(
                name: "FK_PERSONA_SUBSIDIARIA_SUB_ID",
                table: "PERSONA");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Rols_RolId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_HISTORIAL_INCIDENCIA_EMP_ID",
                table: "HISTORIAL_INCIDENCIA");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Eventos",
                table: "Eventos");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_PER_ID",
                table: "EMPLEADO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoEventos",
                table: "TipoEventos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoEpps",
                table: "TipoEpps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoCursos",
                table: "TipoCursos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoAplicantes",
                table: "TipoAplicantes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supervisors",
                table: "Supervisors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Statuses",
                table: "Statuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rols",
                table: "Rols");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistorialExpedientes",
                table: "HistorialExpedientes");

            migrationBuilder.DropIndex(
                name: "IX_HistorialExpedientes_ExpedienteExpId",
                table: "HistorialExpedientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistorialEpps",
                table: "HistorialEpps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistorialEntrenamientos",
                table: "HistorialEntrenamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Expedientes",
                table: "Expedientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Epps",
                table: "Epps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EntrenamientoStatuses",
                table: "EntrenamientoStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrenamientos",
                table: "Entrenamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleExpedientes",
                table: "DetalleExpedientes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cursos",
                table: "Cursos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Aplicantes",
                table: "Aplicantes");

            migrationBuilder.DropColumn(
                name: "EMP_ID",
                table: "HISTORIAL_INCIDENCIA");

            migrationBuilder.DropColumn(
                name: "ExpedienteExpId",
                table: "HistorialExpedientes");

            migrationBuilder.RenameTable(
                name: "Eventos",
                newName: "EVENTOS");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "USUARIO");

            migrationBuilder.RenameTable(
                name: "TipoEventos",
                newName: "TIPO_EVENTO");

            migrationBuilder.RenameTable(
                name: "TipoEpps",
                newName: "TIPO_EPP");

            migrationBuilder.RenameTable(
                name: "TipoCursos",
                newName: "TIPO_CURSO");

            migrationBuilder.RenameTable(
                name: "TipoAplicantes",
                newName: "TipoAplicante");

            migrationBuilder.RenameTable(
                name: "Supervisors",
                newName: "SUPERVISOR");

            migrationBuilder.RenameTable(
                name: "Statuses",
                newName: "STATUS");

            migrationBuilder.RenameTable(
                name: "Rols",
                newName: "ROL");

            migrationBuilder.RenameTable(
                name: "HistorialExpedientes",
                newName: "HISTORIAL_EXPEDIENTE");

            migrationBuilder.RenameTable(
                name: "HistorialEpps",
                newName: "HISTORIAL_EPP");

            migrationBuilder.RenameTable(
                name: "HistorialEntrenamientos",
                newName: "HISTORIAL_ENTRENAMIENTO");

            migrationBuilder.RenameTable(
                name: "Expedientes",
                newName: "EXPEDIENTE");

            migrationBuilder.RenameTable(
                name: "Epps",
                newName: "EPP");

            migrationBuilder.RenameTable(
                name: "EntrenamientoStatuses",
                newName: "ENTRENAMIENTO_STATUS");

            migrationBuilder.RenameTable(
                name: "Entrenamientos",
                newName: "ENTRENAMIENTO");

            migrationBuilder.RenameTable(
                name: "DetalleExpedientes",
                newName: "DETALLE_EXPEDIENTE");

            migrationBuilder.RenameTable(
                name: "Cursos",
                newName: "CURSO");

            migrationBuilder.RenameTable(
                name: "Aplicantes",
                newName: "APLICANTE");

            migrationBuilder.RenameColumn(
                name: "TipoEventoId",
                table: "EVENTOS",
                newName: "TIPO_EVENTO_ID");

            migrationBuilder.RenameColumn(
                name: "NameEvent",
                table: "EVENTOS",
                newName: "NAME_EVENT");

            migrationBuilder.RenameColumn(
                name: "EventDate",
                table: "EVENTOS",
                newName: "EVENT_DATE");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "EVENTOS",
                newName: "EVENT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Eventos_TipoEventoId",
                table: "EVENTOS",
                newName: "IX_EVENTOS_TIPO_EVENTO_ID");

            migrationBuilder.RenameColumn(
                name: "UsuUsuario",
                table: "USUARIO",
                newName: "USU_USUARIO");

            migrationBuilder.RenameColumn(
                name: "UsuContrasena",
                table: "USUARIO",
                newName: "USU_CONTRASENA");

            migrationBuilder.RenameColumn(
                name: "UsuCodigo",
                table: "USUARIO",
                newName: "USU_CODIGO");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "USUARIO",
                newName: "ROL_ID");

            migrationBuilder.RenameColumn(
                name: "UsuId",
                table: "USUARIO",
                newName: "USU_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Usuarios_RolId",
                table: "USUARIO",
                newName: "IX_USUARIO_ROL_ID");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TIPO_EVENTO",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "Module",
                table: "TIPO_EVENTO",
                newName: "MODULE");

            migrationBuilder.RenameColumn(
                name: "Datecolumn",
                table: "TIPO_EVENTO",
                newName: "DATECOLUMN");

            migrationBuilder.RenameColumn(
                name: "Datasource",
                table: "TIPO_EVENTO",
                newName: "DATASOURCE");

            migrationBuilder.RenameColumn(
                name: "TipoEventoId",
                table: "TIPO_EVENTO",
                newName: "TIPO_EVENTO_ID");

            migrationBuilder.RenameColumn(
                name: "TepTipo",
                table: "TIPO_EPP",
                newName: "TEP_TIPO");

            migrationBuilder.RenameColumn(
                name: "TepId",
                table: "TIPO_EPP",
                newName: "TEP_ID");

            migrationBuilder.RenameColumn(
                name: "TicNombre",
                table: "TIPO_CURSO",
                newName: "TIC_NOMBRE");

            migrationBuilder.RenameColumn(
                name: "TicId",
                table: "TIPO_CURSO",
                newName: "TIC_ID");

            migrationBuilder.RenameColumn(
                name: "TipNombre",
                table: "TipoAplicante",
                newName: "TIP_NOMBRE");

            migrationBuilder.RenameColumn(
                name: "TipId",
                table: "TipoAplicante",
                newName: "TIP_ID");

            migrationBuilder.RenameColumn(
                name: "SupNombre",
                table: "SUPERVISOR",
                newName: "SUP_NOMBRE");

            migrationBuilder.RenameColumn(
                name: "SupId",
                table: "SUPERVISOR",
                newName: "SUP_ID");

            migrationBuilder.RenameColumn(
                name: "StaTipoStatus",
                table: "STATUS",
                newName: "STA_TIPO_STATUS");

            migrationBuilder.RenameColumn(
                name: "StaId",
                table: "STATUS",
                newName: "STA_ID");

            migrationBuilder.RenameColumn(
                name: "RolTipo",
                table: "ROL",
                newName: "ROL_TIPO");

            migrationBuilder.RenameColumn(
                name: "RolId",
                table: "ROL",
                newName: "ROL_ID");

            migrationBuilder.RenameColumn(
                name: "ExpId",
                table: "HISTORIAL_EXPEDIENTE",
                newName: "EXP_ID");

            migrationBuilder.RenameColumn(
                name: "HexId",
                table: "HISTORIAL_EXPEDIENTE",
                newName: "HEX_ID");

            migrationBuilder.RenameColumn(
                name: "EppId",
                table: "HISTORIAL_EPP",
                newName: "EPP_ID");

            migrationBuilder.RenameColumn(
                name: "HepId",
                table: "HISTORIAL_EPP",
                newName: "HEP_ID");

            migrationBuilder.RenameIndex(
                name: "IX_HistorialEpps_EppId",
                table: "HISTORIAL_EPP",
                newName: "IX_HISTORIAL_EPP_EPP_ID");

            migrationBuilder.RenameColumn(
                name: "EntId",
                table: "HISTORIAL_ENTRENAMIENTO",
                newName: "ENT_ID");

            migrationBuilder.RenameColumn(
                name: "HenId",
                table: "HISTORIAL_ENTRENAMIENTO",
                newName: "HEN_ID");

            migrationBuilder.RenameIndex(
                name: "IX_HistorialEntrenamientos_EntId",
                table: "HISTORIAL_ENTRENAMIENTO",
                newName: "IX_HISTORIAL_ENTRENAMIENTO_ENT_ID");

            migrationBuilder.RenameColumn(
                name: "ExpRuta",
                table: "EXPEDIENTE",
                newName: "EXP_RUTA");

            migrationBuilder.RenameColumn(
                name: "ExpFechasubido",
                table: "EXPEDIENTE",
                newName: "EXP_FECHASUBIDO");

            migrationBuilder.RenameColumn(
                name: "ExpFechacaducidad",
                table: "EXPEDIENTE",
                newName: "EXP_FECHACADUCIDAD");

            migrationBuilder.RenameColumn(
                name: "EmpId",
                table: "EXPEDIENTE",
                newName: "EMP_ID");

            migrationBuilder.RenameColumn(
                name: "DexId",
                table: "EXPEDIENTE",
                newName: "DEX_ID");

            migrationBuilder.RenameColumn(
                name: "ExpId",
                table: "EXPEDIENTE",
                newName: "EXP_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Expedientes_EmpId",
                table: "EXPEDIENTE",
                newName: "IX_EXPEDIENTE_EMP_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Expedientes_DexId",
                table: "EXPEDIENTE",
                newName: "IX_EXPEDIENTE_DEX_ID");

            migrationBuilder.RenameColumn(
                name: "TepId",
                table: "EPP",
                newName: "TEP_ID");

            migrationBuilder.RenameColumn(
                name: "EppFecha",
                table: "EPP",
                newName: "EPP_FECHA");

            migrationBuilder.RenameColumn(
                name: "EppCantidad",
                table: "EPP",
                newName: "EPP_CANTIDAD");

            migrationBuilder.RenameColumn(
                name: "EppId",
                table: "EPP",
                newName: "EPP_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Epps_TepId",
                table: "EPP",
                newName: "IX_EPP_TEP_ID");

            migrationBuilder.RenameColumn(
                name: "EnsStatus",
                table: "ENTRENAMIENTO_STATUS",
                newName: "ENS_STATUS");

            migrationBuilder.RenameColumn(
                name: "EnsId",
                table: "ENTRENAMIENTO_STATUS",
                newName: "ENS_ID");

            migrationBuilder.RenameColumn(
                name: "EntFechaexpiracion",
                table: "ENTRENAMIENTO",
                newName: "ENT_FECHAEXPIRACION");

            migrationBuilder.RenameColumn(
                name: "EntFechacompletacion",
                table: "ENTRENAMIENTO",
                newName: "ENT_FECHACOMPLETACION");

            migrationBuilder.RenameColumn(
                name: "EntFechaasignacion",
                table: "ENTRENAMIENTO",
                newName: "ENT_FECHAASIGNACION");

            migrationBuilder.RenameColumn(
                name: "EnsId",
                table: "ENTRENAMIENTO",
                newName: "ENS_ID");

            migrationBuilder.RenameColumn(
                name: "CurId",
                table: "ENTRENAMIENTO",
                newName: "CUR_ID");

            migrationBuilder.RenameColumn(
                name: "EntId",
                table: "ENTRENAMIENTO",
                newName: "ENT_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Entrenamientos_EnsId",
                table: "ENTRENAMIENTO",
                newName: "IX_ENTRENAMIENTO_ENS_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Entrenamientos_CurId",
                table: "ENTRENAMIENTO",
                newName: "IX_ENTRENAMIENTO_CUR_ID");

            migrationBuilder.RenameColumn(
                name: "DexNomenclatura",
                table: "DETALLE_EXPEDIENTE",
                newName: "DEX_NOMENCLATURA");

            migrationBuilder.RenameColumn(
                name: "DexNombre",
                table: "DETALLE_EXPEDIENTE",
                newName: "DEX_NOMBRE");

            migrationBuilder.RenameColumn(
                name: "DexId",
                table: "DETALLE_EXPEDIENTE",
                newName: "DEX_ID");

            migrationBuilder.RenameColumn(
                name: "TicId",
                table: "CURSO",
                newName: "TIC_ID");

            migrationBuilder.RenameColumn(
                name: "CurVigencia",
                table: "CURSO",
                newName: "CUR_VIGENCIA");

            migrationBuilder.RenameColumn(
                name: "CurObligatorio",
                table: "CURSO",
                newName: "CUR_OBLIGATORIO");

            migrationBuilder.RenameColumn(
                name: "CurDescripcion",
                table: "CURSO",
                newName: "CUR_DESCRIPCION");

            migrationBuilder.RenameColumn(
                name: "CurId",
                table: "CURSO",
                newName: "CUR_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Cursos_TicId",
                table: "CURSO",
                newName: "IX_CURSO_TIC_ID");

            migrationBuilder.RenameColumn(
                name: "TipId",
                table: "APLICANTE",
                newName: "TIP_ID");

            migrationBuilder.RenameColumn(
                name: "PerId",
                table: "APLICANTE",
                newName: "PER_ID");

            migrationBuilder.RenameColumn(
                name: "AplStatus",
                table: "APLICANTE",
                newName: "APL_STATUS");

            migrationBuilder.RenameColumn(
                name: "AplIspromoted",
                table: "APLICANTE",
                newName: "APL_ISPROMOTED");

            migrationBuilder.RenameColumn(
                name: "AplFechaTerminoEntrevista",
                table: "APLICANTE",
                newName: "APL_FECHA_TERMINO_ENTREVISTA");

            migrationBuilder.RenameColumn(
                name: "AplFechaInicioEntrevista",
                table: "APLICANTE",
                newName: "APL_FECHA_INICIO_ENTREVISTA");

            migrationBuilder.RenameColumn(
                name: "AplCv",
                table: "APLICANTE",
                newName: "APL_CV");

            migrationBuilder.RenameColumn(
                name: "AplId",
                table: "APLICANTE",
                newName: "APL_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Aplicantes_TipId",
                table: "APLICANTE",
                newName: "IX_APLICANTE_TIP_ID");

            migrationBuilder.AlterColumn<string>(
                name: "SUB_NOMBRE",
                table: "SUBSIDIARIA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SUB_ID",
                table: "SUBSIDIARIA",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "PER_TELEFONO",
                table: "PERSONA",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PER_NOMBRE",
                table: "PERSONA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PER_GENERO",
                table: "PERSONA",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PER_FECHA_REGISTRO",
                table: "PERSONA",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PER_EMAIL",
                table: "PERSONA",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PER_APELLIDO_PATERNO",
                table: "PERSONA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PER_APELLIDO_MATERNO",
                table: "PERSONA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PEP_NOMBRE",
                table: "PERFIL_PUESTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PEP_ID",
                table: "PERFIL_PUESTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "INC_ID",
                table: "INCIDENCIA",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "HIN_ID",
                table: "HISTORIAL_INCIDENCIA",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "NAME_EVENT",
                table: "EVENTOS",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EMP_FECHA_REGISTRO",
                table: "EMPLEADO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EMP_FECHA_CONTRATACION",
                table: "EMPLEADO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EMP_ID",
                table: "EMPLEADO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "EMPLEADO",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HEX_ID",
                table: "EMPLEADO",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DIN_ID",
                table: "DETALLE_INCIDENCIA",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ARE_NOMBRE",
                table: "AREA",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ARE_ID",
                table: "AREA",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "USU_USUARIO",
                table: "USUARIO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "USU_CONTRASENA",
                table: "USUARIO",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "USU_CODIGO",
                table: "USUARIO",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "USU_ID",
                table: "USUARIO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "NAME",
                table: "TIPO_EVENTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MODULE",
                table: "TIPO_EVENTO",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DATECOLUMN",
                table: "TIPO_EVENTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DATASOURCE",
                table: "TIPO_EVENTO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TEP_TIPO",
                table: "TIPO_EPP",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TEP_ID",
                table: "TIPO_EPP",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "TIC_NOMBRE",
                table: "TIPO_CURSO",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TIC_ID",
                table: "TIPO_CURSO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "SUP_NOMBRE",
                table: "SUPERVISOR",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SUP_ID",
                table: "SUPERVISOR",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "STA_TIPO_STATUS",
                table: "STATUS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "STA_ID",
                table: "STATUS",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ROL_TIPO",
                table: "ROL",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ROL_ID",
                table: "ROL",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "HEP_ID",
                table: "HISTORIAL_EPP",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "HEN_ID",
                table: "HISTORIAL_ENTRENAMIENTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EPP_FECHA",
                table: "EPP",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EPP_ID",
                table: "EPP",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "ENS_STATUS",
                table: "ENTRENAMIENTO_STATUS",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ENS_ID",
                table: "ENTRENAMIENTO_STATUS",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ENT_FECHAEXPIRACION",
                table: "ENTRENAMIENTO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ENT_FECHACOMPLETACION",
                table: "ENTRENAMIENTO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ENT_FECHAASIGNACION",
                table: "ENTRENAMIENTO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ENT_ID",
                table: "ENTRENAMIENTO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "DEX_NOMENCLATURA",
                table: "DETALLE_EXPEDIENTE",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DEX_NOMBRE",
                table: "DETALLE_EXPEDIENTE",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CUR_VIGENCIA",
                table: "CURSO",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CUR_ID",
                table: "CURSO",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "APL_CV",
                table: "APLICANTE",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__EVENTOS__241A510CE2D5F0E3",
                table: "EVENTOS",
                column: "EVENT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USUARIO",
                table: "USUARIO",
                column: "USU_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK__TIPO_EVE__A7C0AD9945F447C8",
                table: "TIPO_EVENTO",
                column: "TIPO_EVENTO_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TIPO_EPP",
                table: "TIPO_EPP",
                column: "TEP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TIPO_CURSO",
                table: "TIPO_CURSO",
                column: "TIC_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoAplicante",
                table: "TipoAplicante",
                column: "TIP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SUPERVISOR",
                table: "SUPERVISOR",
                column: "SUP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_STATUS",
                table: "STATUS",
                column: "STA_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ROL",
                table: "ROL",
                column: "ROL_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HISTORIAL_EXPEDIENTE",
                table: "HISTORIAL_EXPEDIENTE",
                column: "HEX_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HISTORIAL_EPP",
                table: "HISTORIAL_EPP",
                column: "HEP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HISTORIAL_ENTRENAMIENTO",
                table: "HISTORIAL_ENTRENAMIENTO",
                column: "HEN_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EXPEDIENTE",
                table: "EXPEDIENTE",
                column: "EXP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EPP",
                table: "EPP",
                column: "EPP_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ENTRENAMIENTO_STATUS",
                table: "ENTRENAMIENTO_STATUS",
                column: "ENS_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ENTRENAMIENTO",
                table: "ENTRENAMIENTO",
                column: "ENT_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DETALLE_EXPEDIENTE",
                table: "DETALLE_EXPEDIENTE",
                column: "DEX_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CURSO",
                table: "CURSO",
                column: "CUR_ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_APLICANTE",
                table: "APLICANTE",
                column: "APL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_ApplicationUserId",
                table: "EMPLEADO",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_HEX_ID",
                table: "EMPLEADO",
                column: "HEX_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_HIN_ID",
                table: "EMPLEADO",
                column: "HIN_ID");

            migrationBuilder.CreateIndex(
                name: "IX_HISTORIAL_EXPEDIENTE_EXP_ID",
                table: "HISTORIAL_EXPEDIENTE",
                column: "EXP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_APLICANTE_TIPOAPLICANTE_TIP_ID",
                table: "APLICANTE",
                column: "TIP_ID",
                principalTable: "TipoAplicante",
                principalColumn: "TIP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_CURSO_TIPO_CURSO",
                table: "CURSO",
                column: "TIC_ID",
                principalTable: "TIPO_CURSO",
                principalColumn: "TIC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_AspNetUsers_ApplicationUserId",
                table: "EMPLEADO",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_ENTRENAMIENTO",
                table: "EMPLEADO",
                column: "HEN_ID",
                principalTable: "HISTORIAL_ENTRENAMIENTO",
                principalColumn: "HEN_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_EPP",
                table: "EMPLEADO",
                column: "HEP_ID",
                principalTable: "HISTORIAL_EPP",
                principalColumn: "HEP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_EXPEDIENTE",
                table: "EMPLEADO",
                column: "HEX_ID",
                principalTable: "HISTORIAL_EXPEDIENTE",
                principalColumn: "HEX_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_INCIDENCIA",
                table: "EMPLEADO",
                column: "HIN_ID",
                principalTable: "HISTORIAL_INCIDENCIA",
                principalColumn: "HIN_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_STATUS",
                table: "EMPLEADO",
                column: "STA_ID",
                principalTable: "STATUS",
                principalColumn: "STA_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_SUPERVISOR",
                table: "EMPLEADO",
                column: "SUP_ID",
                principalTable: "SUPERVISOR",
                principalColumn: "SUP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_USUARIO",
                table: "EMPLEADO",
                column: "USU_ID",
                principalTable: "USUARIO",
                principalColumn: "USU_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ENTRENAMIENTO_CURSO",
                table: "ENTRENAMIENTO",
                column: "CUR_ID",
                principalTable: "CURSO",
                principalColumn: "CUR_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ENTRENAMIENTO_ENTRENAMIENTO_STATUS",
                table: "ENTRENAMIENTO",
                column: "ENS_ID",
                principalTable: "ENTRENAMIENTO_STATUS",
                principalColumn: "ENS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EPP_TIPO_EPP",
                table: "EPP",
                column: "TEP_ID",
                principalTable: "TIPO_EPP",
                principalColumn: "TEP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EVENTOS_TIPO_EVENTO",
                table: "EVENTOS",
                column: "TIPO_EVENTO_ID",
                principalTable: "TIPO_EVENTO",
                principalColumn: "TIPO_EVENTO_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EXPEDIENTE_DETALLE_EXPEDIENTE",
                table: "EXPEDIENTE",
                column: "DEX_ID",
                principalTable: "DETALLE_EXPEDIENTE",
                principalColumn: "DEX_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EXPEDIENTE_EMPLEADO_EMP_ID",
                table: "EXPEDIENTE",
                column: "EMP_ID",
                principalTable: "EMPLEADO",
                principalColumn: "EMP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_HISTORIAL_ENTRENAMIENTO_ENTRENAMIENTO",
                table: "HISTORIAL_ENTRENAMIENTO",
                column: "ENT_ID",
                principalTable: "ENTRENAMIENTO",
                principalColumn: "ENT_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_HISTORIAL_EPP_EPP",
                table: "HISTORIAL_EPP",
                column: "EPP_ID",
                principalTable: "EPP",
                principalColumn: "EPP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_HISTORIAL_EXPEDIENTE_EXPEDIENTE",
                table: "HISTORIAL_EXPEDIENTE",
                column: "EXP_ID",
                principalTable: "EXPEDIENTE",
                principalColumn: "EXP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PERFIL_PUESTO_AREA",
                table: "PERFIL_PUESTO",
                column: "ARE_ID",
                principalTable: "AREA",
                principalColumn: "ARE_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PERSONA_PERFIL_PUESTO",
                table: "PERSONA",
                column: "PEP_ID",
                principalTable: "PERFIL_PUESTO",
                principalColumn: "PEP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PERSONA_SUBSIDIARIA",
                table: "PERSONA",
                column: "SUB_ID",
                principalTable: "SUBSIDIARIA",
                principalColumn: "SUB_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_USUARIO_ROL",
                table: "USUARIO",
                column: "ROL_ID",
                principalTable: "ROL",
                principalColumn: "ROL_ID");
        }
    }
}
