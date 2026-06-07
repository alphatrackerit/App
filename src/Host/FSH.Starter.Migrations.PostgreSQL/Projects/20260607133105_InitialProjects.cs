using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Projects
{
    /// <inheritdoc />
    public partial class InitialProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "projects");

            migrationBuilder.CreateTable(
                name: "Proyectos",
                schema: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric", nullable: true),
                    VentaPrevista = table.Column<decimal>(type: "numeric", nullable: true),
                    Coste = table.Column<decimal>(type: "numeric", nullable: true),
                    CostePrevisto = table.Column<decimal>(type: "numeric", nullable: true),
                    Beneficio = table.Column<decimal>(type: "numeric", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SociedadId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaisId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrefijoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proyectos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingresos",
                schema: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Porcentaje = table.Column<decimal>(type: "numeric", nullable: true),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    Validado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingresos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "projects",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notas",
                schema: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'-infinity'"),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notas_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "projects",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                schema: "projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Porcentaje = table.Column<decimal>(type: "numeric", nullable: true),
                    ProveedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: true),
                    EstadoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Confirmado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Validado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalSchema: "projects",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_EstadoId",
                schema: "projects",
                table: "Ingresos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_ProyectoId",
                schema: "projects",
                table: "Ingresos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_ProyectoId",
                schema: "projects",
                table: "Notas",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_EstadoId",
                schema: "projects",
                table: "Pagos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ProveedorId",
                schema: "projects",
                table: "Pagos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ProyectoId",
                schema: "projects",
                table: "Pagos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_ClienteId",
                schema: "projects",
                table: "Proyectos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EmpresaId",
                schema: "projects",
                table: "Proyectos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EstadoId",
                schema: "projects",
                table: "Proyectos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_PaisId",
                schema: "projects",
                table: "Proyectos",
                column: "PaisId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_PrefijoId",
                schema: "projects",
                table: "Proyectos",
                column: "PrefijoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_SociedadId",
                schema: "projects",
                table: "Proyectos",
                column: "SociedadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingresos",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "Notas",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "Pagos",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "Proyectos",
                schema: "projects");
        }
    }
}
