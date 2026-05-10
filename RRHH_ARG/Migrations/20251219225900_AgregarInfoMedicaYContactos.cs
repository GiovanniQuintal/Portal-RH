using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RRHH_ARG.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInfoMedicaYContactos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {/*
           // migrationBuilder.DropForeignKey(
             //   name: "FK_EMPLEADO_Supervisors_SupervisorSupId",
               // table: "EMPLEADO");

           // migrationBuilder.DropForeignKey(
             //   name: "FK_Epps_TipoEpps_TepId",
               // table: "Epps");

            migrationBuilder.DropIndex(
                name: "IX_EMPLEADO_SupervisorSupId",
                table: "EMPLEADO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TipoEpps",
                table: "TipoEpps");

            migrationBuilder.DropColumn(
                name: "SupervisorSupId",
                table: "EMPLEADO");

            migrationBuilder.RenameTable(
                name: "TipoEpps",
                newName: "TIPO_EPP");

            migrationBuilder.RenameColumn(
                name: "TepTipo",
                table: "TIPO_EPP",
                newName: "TEP_TIPO");

            migrationBuilder.RenameColumn(
                name: "TepId",
                table: "TIPO_EPP",
                newName: "TEP_ID");

            migrationBuilder.AddColumn<byte[]>(
                name: "PEP_ARCHIVO",
                table: "PERFIL_PUESTO",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PEP_DESCRIPCION",
                table: "PERFIL_PUESTO",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EMP_CODIGO",
                table: "EMPLEADO",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "APL_FECHA_TERMINO_ENTREVISTA",
                table: "APLICANTE",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "APL_FECHA_INICIO_ENTREVISTA",
                table: "APLICANTE",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TEP_TIPO",
                table: "TIPO_EPP",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TIPO_EPP",
                table: "TIPO_EPP",
                column: "TEP_ID");*/

            migrationBuilder.CreateTable(
                name: "CONTACTO_EMERGENCIA",
                columns: table => new
                {
                    CEM_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CEM_NOMBRE_COMPLETO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CEM_PARENTESCO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CEM_TELEFONO_1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CEM_TELEFONO_2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PER_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTACTO_EMERGENCIA", x => x.CEM_ID);
                    table.ForeignKey(
                        name: "FK_CONTACTO_EMERGENCIA_PERSONA_PER_ID",
                        column: x => x.PER_ID,
                        principalTable: "PERSONA",
                        principalColumn: "PER_ID",
                        onDelete: ReferentialAction.Cascade);
                });
/*
            migrationBuilder.CreateTable(
                name: "ENTREGA_EPP",
                columns: table => new
                {
                    ENT_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EMP_ID = table.Column<int>(type: "int", nullable: false),
                    ENT_FECHA = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ENT_EVIDENCIA = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTREGA_EPP", x => x.ENT_ID);
                    table.ForeignKey(
                        name: "FK_ENTREGA_EPP_EMPLEADO",
                        column: x => x.EMP_ID,
                        principalTable: "EMPLEADO",
                        principalColumn: "EMP_ID",
                        onDelete: ReferentialAction.Restrict);
                });*/

            migrationBuilder.CreateTable(
                name: "INFORMACION_MEDICA",
                columns: table => new
                {
                    IME_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IME_TIPO_SANGRE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IME_ALERGIAS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IME_PADECIMIENTOS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IME_OBSERVACIONES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PER_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INFORMACION_MEDICA", x => x.IME_ID);
                    table.ForeignKey(
                        name: "FK_INFORMACION_MEDICA_PERSONA_PER_ID",
                        column: x => x.PER_ID,
                        principalTable: "PERSONA",
                        principalColumn: "PER_ID",
                        onDelete: ReferentialAction.Cascade);
                });
            /*
            migrationBuilder.CreateTable(
                name: "DETALLE_ENTREGA_EPP",
                columns: table => new
                {
                    DET_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ENT_ID = table.Column<int>(type: "int", nullable: false),
                    TEP_ID = table.Column<int>(type: "int", nullable: false),
                    DET_CANTIDAD = table.Column<int>(type: "int", nullable: false),
                    DET_TALLA_MODELO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DET_ES_NUEVO = table.Column<bool>(type: "bit", nullable: false),
                    DET_DEVOLVIO_ANTERIOR = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DETALLE_ENTREGA_EPP", x => x.DET_ID);
                    table.ForeignKey(
                        name: "FK_DETALLE_ENTREGA_EPP_ENTREGA_EPP",
                        column: x => x.ENT_ID,
                        principalTable: "ENTREGA_EPP",
                        principalColumn: "ENT_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DETALLE_ENTREGA_EPP_TIPO_EPP",
                        column: x => x.TEP_ID,
                        principalTable: "TIPO_EPP",
                        principalColumn: "TEP_ID",
                        onDelete: ReferentialAction.Restrict);
                });*/

            migrationBuilder.CreateIndex(
                name: "IX_CONTACTO_EMERGENCIA_PER_ID",
                table: "CONTACTO_EMERGENCIA",
                column: "PER_ID");

           /* migrationBuilder.CreateIndex(
                name: "IX_DETALLE_ENTREGA_EPP_ENT_ID",
                table: "DETALLE_ENTREGA_EPP",
                column: "ENT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DETALLE_ENTREGA_EPP_TEP_ID",
                table: "DETALLE_ENTREGA_EPP",
                column: "TEP_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGA_EPP_EMP_ID",
                table: "ENTREGA_EPP",
                column: "EMP_ID");*/

            migrationBuilder.CreateIndex(
                name: "IX_INFORMACION_MEDICA_PER_ID",
                table: "INFORMACION_MEDICA",
                column: "PER_ID",
                unique: true);

           /* migrationBuilder.AddForeignKey(
                name: "FK_Epps_TIPO_EPP_TepId",
                table: "Epps",
                column: "TepId",
                principalTable: "TIPO_EPP",
                principalColumn: "TEP_ID",
                onDelete: ReferentialAction.Restrict);*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Epps_TIPO_EPP_TepId",
                table: "Epps");

            migrationBuilder.DropTable(
                name: "CONTACTO_EMERGENCIA");

            migrationBuilder.DropTable(
                name: "DETALLE_ENTREGA_EPP");

            migrationBuilder.DropTable(
                name: "INFORMACION_MEDICA");

            migrationBuilder.DropTable(
                name: "ENTREGA_EPP");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TIPO_EPP",
                table: "TIPO_EPP");

            migrationBuilder.DropColumn(
                name: "PEP_ARCHIVO",
                table: "PERFIL_PUESTO");

            migrationBuilder.DropColumn(
                name: "PEP_DESCRIPCION",
                table: "PERFIL_PUESTO");

            migrationBuilder.DropColumn(
                name: "EMP_CODIGO",
                table: "EMPLEADO");

            migrationBuilder.RenameTable(
                name: "TIPO_EPP",
                newName: "TipoEpps");

            migrationBuilder.RenameColumn(
                name: "TEP_TIPO",
                table: "TipoEpps",
                newName: "TepTipo");

            migrationBuilder.RenameColumn(
                name: "TEP_ID",
                table: "TipoEpps",
                newName: "TepId");

            migrationBuilder.AddColumn<int>(
                name: "SupervisorSupId",
                table: "EMPLEADO",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "APL_FECHA_TERMINO_ENTREVISTA",
                table: "APLICANTE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "APL_FECHA_INICIO_ENTREVISTA",
                table: "APLICANTE",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TepTipo",
                table: "TipoEpps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TipoEpps",
                table: "TipoEpps",
                column: "TepId");

            migrationBuilder.CreateIndex(
                name: "IX_EMPLEADO_SupervisorSupId",
                table: "EMPLEADO",
                column: "SupervisorSupId");

            migrationBuilder.AddForeignKey(
                name: "FK_EMPLEADO_Supervisors_SupervisorSupId",
                table: "EMPLEADO",
                column: "SupervisorSupId",
                principalTable: "Supervisors",
                principalColumn: "SUP_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Epps_TipoEpps_TepId",
                table: "Epps",
                column: "TepId",
                principalTable: "TipoEpps",
                principalColumn: "TepId");
        }
    }
}
