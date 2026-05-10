using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTipoEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
        name: "TipId",
        table: "EMPLEADO",
        type: "int",
        nullable: true);

            // BORRAR O COMENTAR: Como ya creaste esto a mano en SQL, 
            // quitamos la instrucción para que no de error de "Columna duplicada".
            /* migrationBuilder.AddColumn<byte[]>(
                name: "DIN_CONTENIDO",
                table: "DETALLE_INCIDENCIA",
                type: "varbinary(max)",
                nullable: true);
            */

            // MANTENER: Los índices y FK suelen ser seguros de dejar, 
            // pero si ya creaste la FK a mano, comenta esto también.
            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_TipId",
                table: "EMPLEADO",
                column: "TipId");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_TipoAplicante_TipId",
                table: "EMPLEADO",
                column: "TipId",
                principalTable: "TipoAplicante",
                principalColumn: "TIP_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_TipoAplicante_TipId",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_TipId",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "TipId",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "DIN_CONTENIDO",
                table: "DETALLE_INCIDENCIA");
        }
    }
}
