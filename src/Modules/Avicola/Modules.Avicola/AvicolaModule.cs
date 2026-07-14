using Asp.Versioning;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Constants;
using FSH.Framework.Web.Modules;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Features.v1.Alimentacion.CreateAlimentacion;
using FSH.Modules.Avicola.Features.v1.Alimentacion.DeleteAlimentacion;
using FSH.Modules.Avicola.Features.v1.Alimentacion.GetAlimentacionById;
using FSH.Modules.Avicola.Features.v1.Alimentacion.SearchAlimentacion;
using FSH.Modules.Avicola.Features.v1.Alimentacion.UpdateAlimentacion;
using FSH.Modules.Avicola.Features.v1.Despachos.CreateDespacho;
using FSH.Modules.Avicola.Features.v1.Despachos.DeleteDespacho;
using FSH.Modules.Avicola.Features.v1.Despachos.GetDespachoById;
using FSH.Modules.Avicola.Features.v1.Despachos.SearchDespachos;
using FSH.Modules.Avicola.Features.v1.Despachos.UpdateDespacho;
using FSH.Modules.Avicola.Features.v1.Galpones.CreateGalpon;
using FSH.Modules.Avicola.Features.v1.Galpones.DeleteGalpon;
using FSH.Modules.Avicola.Features.v1.Galpones.GetGalponById;
using FSH.Modules.Avicola.Features.v1.Galpones.SearchGalpones;
using FSH.Modules.Avicola.Features.v1.Galpones.UpdateGalpon;
using FSH.Modules.Avicola.Features.v1.Indicadores.GetIndicadoresLote;
using FSH.Modules.Avicola.Features.v1.Indicadores.GetResumenAvicola;
using FSH.Modules.Avicola.Features.v1.Lotes.CerrarLote;
using FSH.Modules.Avicola.Features.v1.Lotes.CreateLote;
using FSH.Modules.Avicola.Features.v1.Lotes.DeleteLote;
using FSH.Modules.Avicola.Features.v1.Lotes.GetLoteById;
using FSH.Modules.Avicola.Features.v1.Lotes.SearchLotes;
using FSH.Modules.Avicola.Features.v1.Lotes.UpdateLote;
using FSH.Modules.Avicola.Features.v1.Mortalidad.CreateMortalidad;
using FSH.Modules.Avicola.Features.v1.Mortalidad.DeleteMortalidad;
using FSH.Modules.Avicola.Features.v1.Mortalidad.GetMortalidadById;
using FSH.Modules.Avicola.Features.v1.Mortalidad.SearchMortalidad;
using FSH.Modules.Avicola.Features.v1.Mortalidad.UpdateMortalidad;
using FSH.Modules.Avicola.Features.v1.Pesos.CreatePeso;
using FSH.Modules.Avicola.Features.v1.Pesos.DeletePeso;
using FSH.Modules.Avicola.Features.v1.Pesos.GetPesoById;
using FSH.Modules.Avicola.Features.v1.Pesos.SearchPesos;
using FSH.Modules.Avicola.Features.v1.Pesos.UpdatePeso;
using FSH.Modules.Avicola.Features.v1.Sanidad.CreateSanidad;
using FSH.Modules.Avicola.Features.v1.Sanidad.DeleteSanidad;
using FSH.Modules.Avicola.Features.v1.Sanidad.GetSanidadById;
using FSH.Modules.Avicola.Features.v1.Sanidad.SearchSanidad;
using FSH.Modules.Avicola.Features.v1.Sanidad.UpdateSanidad;
using FSH.Modules.Avicola.Features.v1.Documentos.CreateDocumento;
using FSH.Modules.Avicola.Features.v1.Documentos.DeleteDocumento;
using FSH.Modules.Avicola.Features.v1.Documentos.SearchDocumentos;
using FSH.Modules.Avicola.Features.v1.Documentos.UpdateDocumento;
using FSH.Modules.Avicola.Features.v1.Pedidos.CambiarEstadoPedido;
using FSH.Modules.Avicola.Features.v1.Pedidos.CreatePedido;
using FSH.Modules.Avicola.Features.v1.Pedidos.DeletePedido;
using FSH.Modules.Avicola.Features.v1.Pedidos.GetPedidoById;
using FSH.Modules.Avicola.Features.v1.Pedidos.SearchPedidos;
using FSH.Modules.Avicola.Features.v1.Pedidos.UpdatePedido;
using FSH.Modules.Avicola.Features.v1.Movimientos.CreateMovimiento;
using FSH.Modules.Avicola.Features.v1.Movimientos.DeleteMovimiento;
using FSH.Modules.Avicola.Features.v1.Movimientos.SearchMovimientos;
using FSH.Modules.Avicola.Features.v1.Movimientos.UpdateMovimiento;
using FSH.Modules.Avicola.Features.v1.Contabilidad.GetContabilidad;
using FSH.Modules.Avicola.Features.v1.Contabilidad.GetLiquidacionLote;
using FSH.Modules.Avicola.Features.v1.Preparaciones.CompletarPreparacion;
using FSH.Modules.Avicola.Features.v1.Preparaciones.CreatePreparacion;
using FSH.Modules.Avicola.Features.v1.Preparaciones.DeletePreparacion;
using FSH.Modules.Avicola.Features.v1.Preparaciones.GetPreparacionById;
using FSH.Modules.Avicola.Features.v1.Preparaciones.SearchPreparaciones;
using FSH.Modules.Avicola.Features.v1.Preparaciones.UpdatePreparacion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

[assembly: FshModule(typeof(FSH.Modules.Avicola.AvicolaModule), 900)]

namespace FSH.Modules.Avicola;

public sealed class AvicolaModule : IModule
{
    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        FSH.Framework.Shared.Constants.PermissionConstants.Register(
            FSH.Modules.Avicola.Contracts.Authorization.AvicolaPermissions.All);

        builder.Services.AddHeroDbContext<AvicolaDbContext>();
        builder.Services.AddScoped<IDbInitializer, AvicolaDbInitializer>();

        // Document attachments are stored via the Files module. This policy authorizes
        // attach/read/delete for the OwnerType named AvicolaDocumento.
        builder.Services.AddScoped<FSH.Modules.Files.Contracts.IFileAccessPolicy, Authorization.AvicolaFileAccessPolicy>();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AvicolaDbContext>(
                name: "db:avicola",
                failureStatus: HealthStatus.Unhealthy);
    }

    public void ConfigureMiddleware(IApplicationBuilder app)
    {
        // No custom middleware needed.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        var group = endpoints
            .MapGroup("api/v{version:apiVersion}/avicola")
            .WithTags("Avicola")
            .WithApiVersionSet(versionSet)
            // Named policy so .RequirePermission() gates run; a bare RequireAuthorization()
            // swaps in the authenticated-only default policy and skips permission checks.
            .RequireAuthorization(PermissionConstants.RequiredPermissionPolicyName);

        // Galpones (sheds)
        group.MapSearchGalponesEndpoint();
        group.MapCreateGalponEndpoint();
        group.MapGetGalponByIdEndpoint();
        group.MapUpdateGalponEndpoint();
        group.MapDeleteGalponEndpoint();

        // Lotes (flocks) — central aggregate
        group.MapSearchLotesEndpoint();
        group.MapCreateLoteEndpoint();
        group.MapGetLoteByIdEndpoint();
        group.MapUpdateLoteEndpoint();
        group.MapDeleteLoteEndpoint();
        group.MapCerrarLoteEndpoint();

        // Mortalidad (mortality)
        group.MapSearchMortalidadEndpoint();
        group.MapCreateMortalidadEndpoint();
        group.MapGetMortalidadByIdEndpoint();
        group.MapUpdateMortalidadEndpoint();
        group.MapDeleteMortalidadEndpoint();

        // Alimentacion (feed)
        group.MapSearchAlimentacionEndpoint();
        group.MapCreateAlimentacionEndpoint();
        group.MapGetAlimentacionByIdEndpoint();
        group.MapUpdateAlimentacionEndpoint();
        group.MapDeleteAlimentacionEndpoint();

        // Pesos (weights)
        group.MapSearchPesosEndpoint();
        group.MapCreatePesoEndpoint();
        group.MapGetPesoByIdEndpoint();
        group.MapUpdatePesoEndpoint();
        group.MapDeletePesoEndpoint();

        // Sanidad (health)
        group.MapSearchSanidadEndpoint();
        group.MapCreateSanidadEndpoint();
        group.MapGetSanidadByIdEndpoint();
        group.MapUpdateSanidadEndpoint();
        group.MapDeleteSanidadEndpoint();

        // Despachos (harvest / dispatch)
        group.MapSearchDespachosEndpoint();
        group.MapCreateDespachoEndpoint();
        group.MapGetDespachoByIdEndpoint();
        group.MapUpdateDespachoEndpoint();
        group.MapDeleteDespachoEndpoint();

        // Indicadores (KPIs / dashboard)
        group.MapGetIndicadoresLoteEndpoint();
        group.MapGetResumenAvicolaEndpoint();

        // Documentos (attachments via Files module)
        group.MapSearchDocumentosEndpoint();
        group.MapCreateDocumentoEndpoint();
        group.MapUpdateDocumentoEndpoint();
        group.MapDeleteDocumentoEndpoint();

        // Pedidos (purchase orders)
        group.MapSearchPedidosEndpoint();
        group.MapCreatePedidoEndpoint();
        group.MapGetPedidoByIdEndpoint();
        group.MapUpdatePedidoEndpoint();
        group.MapDeletePedidoEndpoint();
        group.MapCambiarEstadoPedidoEndpoint();

        // Movimientos contables (manual ledger)
        group.MapSearchMovimientosEndpoint();
        group.MapCreateMovimientoEndpoint();
        group.MapUpdateMovimientoEndpoint();
        group.MapDeleteMovimientoEndpoint();

        // Contabilidad + liquidación (accounting reports)
        group.MapGetContabilidadEndpoint();
        group.MapGetLiquidacionLoteEndpoint();

        // Preparación de nave (vacío sanitario entre camadas)
        group.MapSearchPreparacionesEndpoint();
        group.MapCreatePreparacionEndpoint();
        group.MapGetPreparacionByIdEndpoint();
        group.MapUpdatePreparacionEndpoint();
        group.MapDeletePreparacionEndpoint();
        group.MapCompletarPreparacionEndpoint();
    }
}
