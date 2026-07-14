using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Avicola
{
    /// <inheritdoc />
    public partial class AvicolaPreparacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Preparaciones",
                schema: "avicola",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GalponId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoteAnteriorId = table.Column<Guid>(type: "uuid", nullable: true),
                    FechaRetiro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FechaInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiradaCama = table.Column<bool>(type: "boolean", nullable: false),
                    Lavado = table.Column<bool>(type: "boolean", nullable: false),
                    Desinfeccion = table.Column<bool>(type: "boolean", nullable: false),
                    Desinsectacion = table.Column<bool>(type: "boolean", nullable: false),
                    CamaNueva = table.Column<bool>(type: "boolean", nullable: false),
                    Costo = table.Column<decimal>(type: "numeric", nullable: true),
                    Estado = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preparaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preparaciones_Galpones_GalponId",
                        column: x => x.GalponId,
                        principalSchema: "avicola",
                        principalTable: "Galpones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Preparaciones_Lotes_LoteAnteriorId",
                        column: x => x.LoteAnteriorId,
                        principalSchema: "avicola",
                        principalTable: "Lotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Preparaciones_Estado",
                schema: "avicola",
                table: "Preparaciones",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Preparaciones_GalponId",
                schema: "avicola",
                table: "Preparaciones",
                column: "GalponId");

            migrationBuilder.CreateIndex(
                name: "IX_Preparaciones_LoteAnteriorId",
                schema: "avicola",
                table: "Preparaciones",
                column: "LoteAnteriorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Preparaciones",
                schema: "avicola");
        }
    }
}
