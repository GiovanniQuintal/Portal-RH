using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDesdeBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_APLICANTE_TIPO_APLICANTE",
                table: "APLICANTE");

            migrationBuilder.DropForeignKey(
                name: "FK_EXPEDIENTE_DETALLE_EXPEDIENTE_DEX_ID",
                table: "EXPEDIENTE");

            migrationBuilder.DropForeignKey(
                name: "FK_EXPEDIENTE_EMPLEADO_EMP_ID",
                table: "EXPEDIENTE");

            migrationBuilder.DropForeignKey(
                name: "FK_HistorialExpedientes_EXPEDIENTE_ExpedienteExpId",
                table: "HistorialExpedientes");

            migrationBuilder.DropForeignKey(
                name: "FK_PERFIL_PUESTO_AREA_ARE_ID",
                table: "PERFIL_PUESTO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoAplicante",
                table: "TipoAplicante");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EXPEDIENTE",
                table: "EXPEDIENTE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DETALLE_EXPEDIENTE",
                table: "DETALLE_EXPEDIENTE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_APLICANTE",
                table: "APLICANTE");

            migrationBuilder.DropColumn(
                name: "EXP_ARCHIVO",
                table: "EXPEDIENTE");

            migrationBuilder.DropColumn(
                name: "APL_CV_ARCHIVO",
                table: "APLICANTE");

            migrationBuilder.RenameTable(
                name: "TipoAplicante",
                newName: "TipoAplicantes");

            migrationBuilder.RenameTable(
                name: "EXPEDIENTE",
                newName: "Expedientes");

            migrationBuilder.RenameTable(
                name: "DETALLE_EXPEDIENTE",
                newName: "DetalleExpedientes");

            migrationBuilder.RenameTable(
                name: "APLICANTE",
                newName: "Aplicantes");

            migrationBuilder.RenameColumn(
                name: "USU_ID",
                table: "Usuarios",
                newName: "UsuId");

            migrationBuilder.RenameColumn(
                name: "SUP_ID",
                table: "Supervisors",
                newName: "SupId");

            migrationBuilder.RenameColumn(
                name: "TIP_NOMBRE",
                table: "TipoAplicantes",
                newName: "TipNombre");

            migrationBuilder.RenameColumn(
                name: "TIP_ID",
                table: "TipoAplicantes",
                newName: "TipId");

            migrationBuilder.RenameColumn(
                name: "EXP_RUTA",
                table: "Expedientes",
                newName: "ExpRuta");

            migrationBuilder.RenameColumn(
                name: "EXP_FECHASUBIDO",
                table: "Expedientes",
                newName: "ExpFechasubido");

            migrationBuilder.RenameColumn(
                name: "EXP_FECHACADUCIDAD",
                table: "Expedientes",
                newName: "ExpFechacaducidad");

            migrationBuilder.RenameColumn(
                name: "EMP_ID",
                table: "Expedientes",
                newName: "EmpId");

            migrationBuilder.RenameColumn(
                name: "DEX_ID",
                table: "Expedientes",
                newName: "DexId");

            migrationBuilder.RenameColumn(
                name: "EXP_ID",
                table: "Expedientes",
                newName: "ExpId");

            migrationBuilder.RenameIndex(
                name: "IX_EXPEDIENTE_EMP_ID",
                table: "Expedientes",
                newName: "IX_Expedientes_EmpId");

            migrationBuilder.RenameIndex(
                name: "IX_EXPEDIENTE_DEX_ID",
                table: "Expedientes",
                newName: "IX_Expedientes_DexId");

            migrationBuilder.RenameColumn(
                name: "DEX_NOMENCLATURA",
                table: "DetalleExpedientes",
                newName: "DexNomenclatura");

            migrationBuilder.RenameColumn(
                name: "DEX_NOMBRE",
                table: "DetalleExpedientes",
                newName: "DexNombre");

            migrationBuilder.RenameColumn(
                name: "DEX_ID",
                table: "DetalleExpedientes",
                newName: "DexId");

            migrationBuilder.RenameColumn(
                name: "TIP_ID",
                table: "Aplicantes",
                newName: "TipId");

            migrationBuilder.RenameColumn(
                name: "PER_ID",
                table: "Aplicantes",
                newName: "PerId");

            migrationBuilder.RenameColumn(
                name: "APL_STATUS",
                table: "Aplicantes",
                newName: "AplStatus");

            migrationBuilder.RenameColumn(
                name: "APL_ISPROMOTED",
                table: "Aplicantes",
                newName: "AplIspromoted");

            migrationBuilder.RenameColumn(
                name: "APL_FECHA_TERMINO_ENTREVISTA",
                table: "Aplicantes",
                newName: "AplFechaTerminoEntrevista");

            migrationBuilder.RenameColumn(
                name: "APL_FECHA_INICIO_ENTREVISTA",
                table: "Aplicantes",
                newName: "AplFechaInicioEntrevista");

            migrationBuilder.RenameColumn(
                name: "APL_CV",
                table: "Aplicantes",
                newName: "AplCv");

            migrationBuilder.RenameColumn(
                name: "APL_ID",
                table: "Aplicantes",
                newName: "AplId");

            migrationBuilder.RenameIndex(
                name: "IX_APLICANTE_TIP_ID",
                table: "Aplicantes",
                newName: "IX_Aplicantes_TipId");

            migrationBuilder.AlterColumn<int>(
                name: "ARE_ID",
                table: "PERFIL_PUESTO",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HIN_ID",
                table: "EMPLEADO",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoAplicantes",
                table: "TipoAplicantes",
                column: "TipId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Expedientes",
                table: "Expedientes",
                column: "ExpId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleExpedientes",
                table: "DetalleExpedientes",
                column: "DexId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Aplicantes",
                table: "Aplicantes",
                column: "AplId");

            migrationBuilder.AddForeignKey(
                name: "FK_Aplicantes_TipoAplicantes_TipId",
                table: "Aplicantes",
                column: "TipId",
                principalTable: "TipoAplicantes",
                principalColumn: "TipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_DetalleExpedientes_DexId",
                table: "Expedientes",
                column: "DexId",
                principalTable: "DetalleExpedientes",
                principalColumn: "DexId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_EMPLEADO_EmpId",
                table: "Expedientes",
                column: "EmpId",
                principalTable: "EMPLEADO",
                principalColumn: "EMP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_HistorialExpedientes_Expedientes_ExpedienteExpId",
                table: "HistorialExpedientes",
                column: "ExpedienteExpId",
                principalTable: "Expedientes",
                principalColumn: "ExpId");

            migrationBuilder.AddForeignKey(
                name: "FK_PERFIL_PUESTO_AREA_ARE_ID",
                table: "PERFIL_PUESTO",
                column: "ARE_ID",
                principalTable: "AREA",
                principalColumn: "ARE_ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
