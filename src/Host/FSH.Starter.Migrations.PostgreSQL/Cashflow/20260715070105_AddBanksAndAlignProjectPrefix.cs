using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddBanksAndAlignProjectPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrefijoId",
                schema: "cashflow",
                table: "Proyectos",
                newName: "PrefijoCategoriaId");

            migrationBuilder.RenameIndex(
                name: "IX_Proyectos_PrefijoId",
                schema: "cashflow",
                table: "Proyectos",
                newName: "IX_Proyectos_PrefijoCategoriaId");

            migrationBuilder.CreateTable(
                name: "Bancos",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FilaInicio = table.Column<int>(type: "integer", nullable: false),
                    FechaColumna = table.Column<int>(type: "integer", nullable: false),
                    ConceptoColumna = table.Column<int>(type: "integer", nullable: false),
                    ImporteColumna = table.Column<int>(type: "integer", nullable: false),
                    SaldoColumna = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bancos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosBancos",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric", nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric", nullable: false),
                    Banco = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosBancos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bancos_Nombre",
                schema: "cashflow",
                table: "Bancos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBancos_Banco",
                schema: "cashflow",
                table: "MovimientosBancos",
                column: "Banco");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bancos",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "MovimientosBancos",
                schema: "cashflow");

            migrationBuilder.RenameColumn(
                name: "PrefijoCategoriaId",
                schema: "cashflow",
                table: "Proyectos",
                newName: "PrefijoId");

            migrationBuilder.RenameIndex(
                name: "IX_Proyectos_PrefijoCategoriaId",
                schema: "cashflow",
                table: "Proyectos",
                newName: "IX_Proyectos_PrefijoId");
        }
    }
}
