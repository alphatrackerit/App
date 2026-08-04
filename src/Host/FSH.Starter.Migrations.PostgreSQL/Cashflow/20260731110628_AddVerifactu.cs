using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class AddVerifactu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VerifactuEstado",
                schema: "cashflow",
                table: "Facturas",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "NoAplica");

            migrationBuilder.AddColumn<string>(
                name: "Nif",
                schema: "cashflow",
                table: "Empresas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VerifactuAjustes",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Entorno = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NombreSoftware = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    VersionSoftware = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    NumeroInstalacion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CertificadoCifrado = table.Column<byte[]>(type: "bytea", nullable: true),
                    PasswordCertificadoCifrado = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UltimaHuella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UltimaHuellaFecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifactuAjustes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerifactuAjustes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "cashflow",
                        principalTable: "Empresas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VerifactuRegistros",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    HuellaAnterior = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FechaGeneracion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QrPayload = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Estado = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CodigoRespuestaAeat = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PeticionXml = table.Column<string>(type: "text", nullable: true),
                    RespuestaXml = table.Column<string>(type: "text", nullable: true),
                    Reintentos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifactuRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerifactuRegistros_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalSchema: "cashflow",
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerifactuAjustes_EmpresaId",
                schema: "cashflow",
                table: "VerifactuAjustes",
                columns: new[] { "EmpresaId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifactuAjustes_EmpresaId1",
                schema: "cashflow",
                table: "VerifactuAjustes",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifactuRegistros_Estado",
                schema: "cashflow",
                table: "VerifactuRegistros",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_VerifactuRegistros_FacturaId",
                schema: "cashflow",
                table: "VerifactuRegistros",
                columns: new[] { "FacturaId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifactuRegistros_FacturaId1",
                schema: "cashflow",
                table: "VerifactuRegistros",
                column: "FacturaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerifactuAjustes",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "VerifactuRegistros",
                schema: "cashflow");

            migrationBuilder.DropColumn(
                name: "VerifactuEstado",
                schema: "cashflow",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "Nif",
                schema: "cashflow",
                table: "Empresas");
        }
    }
}
