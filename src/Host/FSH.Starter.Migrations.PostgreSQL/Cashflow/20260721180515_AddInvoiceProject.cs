using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddInvoiceProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProyectoId",
                schema: "cashflow",
                table: "Facturas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ProyectoId",
                schema: "cashflow",
                table: "Facturas",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_ProyectoId",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ProyectoId",
                schema: "cashflow",
                table: "Facturas");
        }
    }
}
