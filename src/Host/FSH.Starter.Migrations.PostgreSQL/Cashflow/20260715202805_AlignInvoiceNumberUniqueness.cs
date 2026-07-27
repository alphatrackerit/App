using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AlignInvoiceNumberUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_Numero_Tipo",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Numero_Emitida",
                schema: "cashflow",
                table: "Facturas",
                columns: new[] { "Numero", "TenantId" },
                unique: true,
                filter: "\"Tipo\" = 'Emitida'");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Numero_Tipo",
                schema: "cashflow",
                table: "Facturas",
                columns: new[] { "Numero", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_Numero_Emitida",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_Numero_Tipo",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Numero_Tipo",
                schema: "cashflow",
                table: "Facturas",
                columns: new[] { "Numero", "Tipo", "TenantId" },
                unique: true);
        }
    }
}
