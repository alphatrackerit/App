using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Avicola
{
    /// <inheritdoc />
    public partial class AvicolaGestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documentos",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Origen = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OrigenId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    NombreArchivo = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Concepto = table.Column<string>(type: "text", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimientos_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Cantidad = table.Column<decimal>(type: "numeric", nullable: false),
                    Unidad = table.Column<string>(type: "text", nullable: true),
                    CostoEstimado = table.Column<decimal>(type: "numeric", nullable: true),
                    CostoReal = table.Column<decimal>(type: "numeric", nullable: true),
                    Estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FechaPedido = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaRecepcion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    GalponId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Galpones_GalponId",
                        column: x => x.GalponId,
                        principalSchema: "avicola",
                        principalTable: "Galpones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pedidos_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Origen_OrigenId",
                schema: "avicola",
                table: "Documentos",
                columns: new[] { "Origen", "OrigenId" });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_Categoria",
                schema: "avicola",
                table: "Movimientos",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_LoteId",
                schema: "avicola",
                table: "Movimientos",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_Tipo",
                schema: "avicola",
                table: "Movimientos",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Estado",
                schema: "avicola",
                table: "Pedidos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_GalponId",
                schema: "avicola",
                table: "Pedidos",
                column: "GalponId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_LoteId",
                schema: "avicola",
                table: "Pedidos",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ProveedorId",
                schema: "avicola",
                table: "Pedidos",
                column: "ProveedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentos",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Movimientos",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Pedidos",
                schema: "avicola");
        }
    }
}
