using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FSH.Starter.Migrations.PostgreSQL.Cashflow
{
    /// <inheritdoc />
    public partial class InitialCashflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cashflow");

            migrationBuilder.CreateTable(
                name: "Clientes",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Paises",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                schema: "cashflow",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proyectos",
                schema: "cashflow",
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
                schema: "cashflow",
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
                        principalSchema: "cashflow",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notas",
                schema: "cashflow",
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
                        principalSchema: "cashflow",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                schema: "cashflow",
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
                        principalSchema: "cashflow",
                        principalTable: "Proyectos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nombre",
                schema: "cashflow",
                table: "Clientes",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Nombre",
                schema: "cashflow",
                table: "Empresas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Estados_Nombre",
                schema: "cashflow",
                table: "Estados",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_EstadoId",
                schema: "cashflow",
                table: "Ingresos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_ProyectoId",
                schema: "cashflow",
                table: "Ingresos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notas_ProyectoId",
                schema: "cashflow",
                table: "Notas",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_EstadoId",
                schema: "cashflow",
                table: "Pagos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ProveedorId",
                schema: "cashflow",
                table: "Pagos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ProyectoId",
                schema: "cashflow",
                table: "Pagos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_Paises_Nombre",
                schema: "cashflow",
                table: "Paises",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Nombre",
                schema: "cashflow",
                table: "Proveedores",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_ClienteId",
                schema: "cashflow",
                table: "Proyectos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EmpresaId",
                schema: "cashflow",
                table: "Proyectos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_EstadoId",
                schema: "cashflow",
                table: "Proyectos",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_PaisId",
                schema: "cashflow",
                table: "Proyectos",
                column: "PaisId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_PrefijoId",
                schema: "cashflow",
                table: "Proyectos",
                column: "PrefijoId");

            migrationBuilder.CreateIndex(
                name: "IX_Proyectos_SociedadId",
                schema: "cashflow",
                table: "Proyectos",
                column: "SociedadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Empresas",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Estados",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Ingresos",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Notas",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Pagos",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Paises",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Proveedores",
                schema: "cashflow");

            migrationBuilder.DropTable(
                name: "Proyectos",
                schema: "cashflow");
        }
    }
}
