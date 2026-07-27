using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddInvoicing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FacturaId",
                schema: "cashflow",
                table: "Pagos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacturaId",
                schema: "cashflow",
                table: "Ingresos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Facturas",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    NumeroDynamics = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FechaFactura = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    SociedadId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseImponible = table.Column<decimal>(type: "numeric", nullable: true),
                    Iva = table.Column<decimal>(type: "numeric", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    FormaPago = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Banco = table.Column<string>(type: "text", nullable: true),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comprobada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.CheckConstraint("CK_Facturas_Tipo", "(\"Tipo\" = 'Emitida' AND \"ClienteId\" IS NOT NULL) OR (\"Tipo\" = 'Recibida' AND \"ProveedorId\" IS NOT NULL)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_FacturaId",
                schema: "cashflow",
                table: "Pagos",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_FacturaId",
                schema: "cashflow",
                table: "Ingresos",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ClienteId",
                schema: "cashflow",
                table: "Facturas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EmpresaId",
                schema: "cashflow",
                table: "Facturas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_EstadoId",
                schema: "cashflow",
                table: "Facturas",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Numero_Tipo",
                schema: "cashflow",
                table: "Facturas",
                columns: new[] { "Numero", "Tipo", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ProveedorId",
                schema: "cashflow",
                table: "Facturas",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_SociedadId",
                schema: "cashflow",
                table: "Facturas",
                column: "SociedadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_Facturas_FacturaId",
                schema: "cashflow",
                table: "Ingresos",
                column: "FacturaId",
                principalSchema: "cashflow",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                schema: "cashflow",
                table: "Pagos",
                column: "FacturaId",
                principalSchema: "cashflow",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_Facturas_FacturaId",
                schema: "cashflow",
                table: "Ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Facturas_FacturaId",
                schema: "cashflow",
                table: "Pagos");

            migrationBuilder.DropTable(
                name: "Facturas",
                schema: "cashflow");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_FacturaId",
                schema: "cashflow",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_FacturaId",
                schema: "cashflow",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "FacturaId",
                schema: "cashflow",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "FacturaId",
                schema: "cashflow",
                table: "Ingresos");
        }
    }
}
