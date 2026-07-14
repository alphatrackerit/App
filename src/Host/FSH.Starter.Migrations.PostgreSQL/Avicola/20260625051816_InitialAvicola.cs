using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Avicola
{
    /// <inheritdoc />
    public partial class InitialAvicola : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "avicola");

            migrationBuilder.CreateTable(
                name: "Galpones",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: true),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    SuperficieM2 = table.Column<decimal>(type: "numeric", nullable: true),
                    Ubicacion = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galpones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    GalponId = table.Column<Guid>(type: "uuid", nullable: true),
                    Raza = table.Column<string>(type: "text", nullable: true),
                    FechaIngreso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CantidadInicial = table.Column<int>(type: "integer", nullable: false),
                    PesoInicialGramos = table.Column<decimal>(type: "numeric", nullable: true),
                    FechaSalidaPrevista = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FechaSalidaReal = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostoPolluelo = table.Column<decimal>(type: "numeric", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Galpones_GalponId",
                        column: x => x.GalponId,
                        principalSchema: "avicola",
                        principalTable: "Galpones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Alimentacion",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TipoAlimento = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CantidadKg = table.Column<decimal>(type: "numeric", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "numeric", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alimentacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alimentacion_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Despachos",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PesoTotalKg = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "numeric", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Despachos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Despachos_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mortalidad",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Descartes = table.Column<int>(type: "integer", nullable: true),
                    Causa = table.Column<string>(type: "text", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mortalidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mortalidad_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pesos",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PesoPromedioGramos = table.Column<decimal>(type: "numeric", nullable: false),
                    CantidadMuestra = table.Column<int>(type: "integer", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pesos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pesos_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Sanidad",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Producto = table.Column<string>(type: "text", nullable: false),
                    Dosis = table.Column<string>(type: "text", nullable: true),
                    ViaAplicacion = table.Column<string>(type: "text", nullable: true),
                    Costo = table.Column<decimal>(type: "numeric", nullable: true),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sanidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sanidad_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alimentacion_LoteId",
                schema: "avicola",
                table: "Alimentacion",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_ClienteId",
                schema: "avicola",
                table: "Despachos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Despachos_LoteId",
                schema: "avicola",
                table: "Despachos",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_Estado",
                schema: "avicola",
                table: "Lotes",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_GalponId",
                schema: "avicola",
                table: "Lotes",
                column: "GalponId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_ProveedorId",
                schema: "avicola",
                table: "Lotes",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Mortalidad_LoteId",
                schema: "avicola",
                table: "Mortalidad",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pesos_LoteId",
                schema: "avicola",
                table: "Pesos",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sanidad_LoteId",
                schema: "avicola",
                table: "Sanidad",
                column: "LoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alimentacion",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Despachos",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Mortalidad",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Pesos",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Sanidad",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Lotes",
                schema: "avicola");

            migrationBuilder.DropTable(
                name: "Galpones",
                schema: "avicola");
        }
    }
}
