using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddProformas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProformaId",
                schema: "cashflow",
                table: "Facturas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Proformas",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    SociedadId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: true),
                    BaseImponible = table.Column<decimal>(type: "numeric", nullable: true),
                    Iva = table.Column<decimal>(type: "numeric", nullable: true),
                    Total = table.Column<decimal>(type: "numeric", nullable: false),
                    FormaPago = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    Documento = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proformas", x => x.Id);
                    table.CheckConstraint("CK_Proformas_Tipo", "(\"Tipo\" = 'Emitida' AND \"ClienteId\" IS NOT NULL) OR (\"Tipo\" = 'Recibida' AND \"ProveedorId\" IS NOT NULL)");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ProformaId",
                schema: "cashflow",
                table: "Facturas",
                column: "ProformaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_ClienteId",
                schema: "cashflow",
                table: "Proformas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_EmpresaId",
                schema: "cashflow",
                table: "Proformas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_EstadoId",
                schema: "cashflow",
                table: "Proformas",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_Numero_Emitida",
                schema: "cashflow",
                table: "Proformas",
                columns: new[] { "Numero", "TenantId" },
                unique: true,
                filter: "\"Tipo\" = 'Emitida'");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_Numero_Tipo",
                schema: "cashflow",
                table: "Proformas",
                columns: new[] { "Numero", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_ProveedorId",
                schema: "cashflow",
                table: "Proformas",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_ProyectoId",
                schema: "cashflow",
                table: "Proformas",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proformas_SociedadId",
                schema: "cashflow",
                table: "Proformas",
                column: "SociedadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Facturas_Proformas_ProformaId",
                schema: "cashflow",
                table: "Facturas",
                column: "ProformaId",
                principalSchema: "cashflow",
                principalTable: "Proformas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Facturas_Proformas_ProformaId",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.DropTable(
                name: "Proformas",
                schema: "cashflow");

            migrationBuilder.DropIndex(
                name: "IX_Facturas_ProformaId",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ProformaId",
                schema: "cashflow",
                table: "Facturas");
        }
    }
}
