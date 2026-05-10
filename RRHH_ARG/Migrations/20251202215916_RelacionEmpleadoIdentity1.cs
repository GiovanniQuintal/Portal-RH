using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class RelacionEmpleadoIdentity1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_ASPNETUSERS",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_ASP_NET_USER_ID",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "ASP_NET_USER_ID",
                table: "EMPLEADO");

            migrationBuilder.AddColumn<int>(
                name: "USU_ID",
                table: "EMPLEADO",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_USU_ID",
                table: "EMPLEADO",
                column: "USU_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_Usuarios_USU_ID",
                table: "EMPLEADO",
                column: "USU_ID",
                principalTable: "Usuarios",
                principalColumn: "USU_ID");
        }
    }
}
