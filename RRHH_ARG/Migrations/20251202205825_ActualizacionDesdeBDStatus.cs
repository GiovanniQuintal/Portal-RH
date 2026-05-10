using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDesdeBDStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_STATUS_STA_ID",
                table: "EMPLEADO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_STATUS",
                table: "STATUS");

            migrationBuilder.RenameTable(
                name: "STATUS",
                newName: "Statuses");

            migrationBuilder.RenameColumn(
                name: "STA_TIPO_STATUS",
                table: "Statuses",
                newName: "StaTipoStatus");

            migrationBuilder.RenameColumn(
                name: "STA_ID",
                table: "Statuses",
                newName: "StaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Statuses",
                table: "Statuses",
                column: "StaId");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_Statuses_STA_ID",
                table: "EMPLEADO",
                column: "STA_ID",
                principalTable: "Statuses",
                principalColumn: "StaId");
        }
    }
}
