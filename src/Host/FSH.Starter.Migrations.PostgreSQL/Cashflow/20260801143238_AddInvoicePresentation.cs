using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddInvoicePresentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FacturaConceptos",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacturaConceptos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacturaConceptos_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalSchema: "cashflow",
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacturaConceptos_FacturaId",
                schema: "cashflow",
                table: "FacturaConceptos",
                column: "FacturaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FacturaConceptos",
                schema: "cashflow");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Direccion",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Logo",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "cashflow",
                table: "Empresas");
        }
    }
}
