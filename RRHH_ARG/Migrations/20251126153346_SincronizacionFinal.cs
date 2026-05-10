using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_AspNetUsers_ApplicationUserId",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_ENTRENAMIENTO",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_EPP",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_EXPEDIENTE",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_HISTORIAL_INCIDENCIA",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_STATUS",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_SUPERVISOR",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_USUARIO",
                table: "EMPLEADO");

            migrationBuilder.DropTable(
                name: "APLICANTE");

            migrationBuilder.DropTable(
                name: "EVENTOS");

            migrationBuilder.DropTable(
                name: "HISTORIAL_ENTRENAMIENTO");

            migrationBuilder.DropTable(
                name: "HISTORIAL_EPP");

            migrationBuilder.DropTable(
                name: "HISTORIAL_EXPEDIENTE");

            migrationBuilder.DropTable(
                name: "HISTORIAL_INCIDENCIA");

            migrationBuilder.DropTable(
                name: "PERSONA");

            migrationBuilder.DropTable(
                name: "STATUS");

            migrationBuilder.DropTable(
                name: "SUPERVISOR");

            migrationBuilder.DropTable(
                name: "USUARIO");

            migrationBuilder.DropTable(
                name: "TipoAplicante");

            migrationBuilder.DropTable(
                name: "TIPO_EVENTO");

            migrationBuilder.DropTable(
                name: "ENTRENAMIENTO");

            migrationBuilder.DropTable(
                name: "EPP");

            migrationBuilder.DropTable(
                name: "EXPEDIENTE");

            migrationBuilder.DropTable(
                name: "INCIDENCIA");

            migrationBuilder.DropTable(
                name: "PERFIL_PUESTO");

            migrationBuilder.DropTable(
                name: "SUBSIDIARIA");

            migrationBuilder.DropTable(
                name: "ROL");

            migrationBuilder.DropTable(
                name: "CURSO");

            migrationBuilder.DropTable(
                name: "ENTRENAMIENTO_STATUS");

            migrationBuilder.DropTable(
                name: "TIPO_EPP");

            migrationBuilder.DropTable(
                name: "DETALLE_EXPEDIENTE");

            migrationBuilder.DropTable(
                name: "DETALLE_INCIDENCIA");

            migrationBuilder.DropTable(
                name: "AREA");

            migrationBuilder.DropTable(
                name: "TIPO_CURSO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EMPLEADO",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_HEN_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_HEP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_HEX_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_HIN_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_STA_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_SUP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_USU_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "EMP_FECHA_CONTRATACION",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "EMP_FECHA_REGISTRO",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "HEN_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "HEP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "HEX_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "HIN_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "PER_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "STA_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "SUP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "USU_ID",
                table: "EMPLEADO");

            migrationBuilder.RenameTable(
                name: "EMPLEADO",
                newName: "Empleado");

            migrationBuilder.RenameColumn(
                name: "EMP_ID",
                table: "Empleado",
                newName: "ExpedientesTotales");

            migrationBuilder.RenameIndex(
                name: "IX_EMPLEADO_ApplicationUserId",
                table: "Empleado",
                newName: "IX_Empleado_ApplicationUserId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Empleado",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Empleo",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExpedientesCompletos",
                table: "Empleado",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaContratacion",
                table: "Empleado",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Empleado",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistro",
                table: "Empleado",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Subsidiaria",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Empleado",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Empleado",
                table: "Empleado",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleado_AspNetUsers_ApplicationUserId",
                table: "Empleado",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
