using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AllowNullReceivedInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                schema: "cashflow",
                table: "Facturas",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Facturas_Numero_Emitida",
                schema: "cashflow",
                table: "Facturas",
                sql: "(\"Tipo\" = 'Recibida') OR (\"Numero\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Facturas_Numero_Emitida",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                schema: "cashflow",
                table: "Facturas",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
