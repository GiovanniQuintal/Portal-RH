using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCampoDinDescripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DIN_DESCRIPCION",
                table: "DETALLE_INCIDENCIA",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DIN_DESCRIPCION",
                table: "DETALLE_INCIDENCIA");
        }
    }
}
