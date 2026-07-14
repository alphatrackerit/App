using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddCashflowCatalogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Codigo",
                schema: "cashflow",
                table: "Paises",
                newName: "CodigoIso");

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaAlta",
                schema: "cashflow",
                table: "Proveedores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NifCif",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrioridadVisual",
                schema: "cashflow",
                table: "Proveedores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoProveedor",
                schema: "cashflow",
                table: "Proveedores",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                schema: "cashflow",
                table: "Paises",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                schema: "cashflow",
                table: "Estados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                schema: "cashflow",
                table: "Estados",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistroFiscal",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaAlta",
                schema: "cashflow",
                table: "Clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NifCif",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoCliente",
                schema: "cashflow",
                table: "Clientes",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Prefijos",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    PrefijoGrupoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prefijos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prefijos_Prefijos_PrefijoGrupoId",
                        column: x => x.PrefijoGrupoId,
                        principalSchema: "cashflow",
                        principalTable: "Prefijos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sociedades",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NifCif = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CodigoPostal = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Ciudad = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Pais = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sociedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sociedades_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "cashflow",
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Estados_Tipo",
                schema: "cashflow",
                table: "Estados",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Prefijos_Nombre",
                schema: "cashflow",
                table: "Prefijos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Prefijos_PrefijoGrupoId",
                schema: "cashflow",
                table: "Prefijos",
                column: "PrefijoGrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Prefijos_Tipo",
                schema: "cashflow",
                table: "Prefijos",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Sociedades_ClienteId",
                schema: "cashflow",
                table: "Sociedades",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sociedades_Nombre",
                schema: "cashflow",
                table: "Sociedades",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prefijos",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Sociedades",
                schema: "cashflow");

            migrationBuilder.DropIndex(
                name: "IX_Estados_Tipo",
                schema: "cashflow",
                table: "Estados");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Contacto",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Direccion",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "NifCif",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "PrioridadVisual",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "TipoProveedor",
                schema: "cashflow",
                table: "Proveedores");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                schema: "cashflow",
                table: "Paises");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                schema: "cashflow",
                table: "Estados");

            migrationBuilder.DropColumn(
                name: "Tipo",
                schema: "cashflow",
                table: "Estados");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "RegistroFiscal",
                schema: "cashflow",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ColorHex",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Contacto",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Direccion",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "NifCif",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Telefono",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoCliente",
                schema: "cashflow",
                table: "Clientes");

            migrationBuilder.RenameColumn(
                name: "CodigoIso",
                schema: "cashflow",
                table: "Paises",
                newName: "Codigo");
        }
    }
}
