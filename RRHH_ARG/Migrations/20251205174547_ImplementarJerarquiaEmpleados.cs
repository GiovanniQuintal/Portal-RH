using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class ImplementarJerarquiaEmpleados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. COMENTAMOS EL DROP (Esto causaba el error porque no existía)
            /* migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_Supervisors_SUP_ID",
                table: "EMPLEADO");
            */

            // 2. COMENTAMOS TODO ESTO
            // EF Core se confundió e intentó crear una columna "fantasma" para 
            // conectarse a la tabla vieja de Supervisores. NO QUEREMOS ESTO.
            /*
            migrationBuilder.AddColumn<int>(
                name: "SupervisorSupId",
                table: "EMPLEADO",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_SupervisorSupId",
                table: "EMPLEADO",
                column: "SupervisorSupId");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_Supervisors_SupervisorSupId",
                table: "EMPLEADO",
                column: "SupervisorSupId",
                principalTable: "Supervisors",
                principalColumn: "Id", // O la PK que tuviera esa tabla
                onDelete: ReferentialAction.Restrict);
            */

            // 3. DEJAMOS SOLO ESTO (LA JOYA DE LA CORONA) 👑
            // Esta es la única instrucción que importa: Conectar SUP_ID con EMP_ID
            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_EMPLEADO_SUP_ID",
                table: "EMPLEADO",
                column: "SUP_ID",
                principalTable: "EMPLEADO",
                principalColumn: "EMP_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_EMPLEADO_SUP_ID",
                table: "EMPLEADO");

            migrationBuilder.DropForeignKey(
                name: "FK_EMPLEADO_Supervisors_SupervisorSupId",
                table: "EMPLEADO");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_SupervisorSupId",
                table: "EMPLEADO");

            migrationBuilder.DropColumn(
                name: "SupervisorSupId",
                table: "EMPLEADO");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_Supervisors_SUP_ID",
                table: "EMPLEADO",
                column: "SUP_ID",
                principalTable: "Supervisors",
                principalColumn: "SUP_ID");
        }
    }
}
