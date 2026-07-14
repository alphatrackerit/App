# Plan de construcción — Módulo `Cashflow` (consolidación)

> Plan derivado de `nuevomodulo.md` (spec del módulo `cashflow`) reconciliado con el estado real del
> repo. **Decisión tomada:** consolidar el dominio financiero de `Modules.Projects` + los catálogos de
> `Modules.Administration` en un **único bounded context nuevo `Modules.Cashflow`**, con el **reporte de
> flujo diario calculado en el servidor** (`/reportes/resumen-diario`, algoritmo §6 de la spec).

---

## 0. Contexto: qué ya existe (no es "desde cero")

El dominio de la spec ya está ~70% construido y **commiteado**, repartido en dos módulos + el dashboard:

| Spec (`cashflow`) | Estado actual | Acción en la consolidación |
|---|---|---|
| Proyecto | `Modules.Projects/Domain/Project.cs` (completo, soft-refs) | Mover; soft-refs → FK reales |
| Ingreso / Pago | `Income.cs` / `Payment.cs` + `Confirmed`/`Validated` + `SetConfirmed/SetValidated` | Mover; corregir reglas §10 |
| Nota | `Note.cs` | Mover |
| Confirmar/Validar | endpoints `/confirm` `/validate` con permiso server-side | Mover; reagrupar bajo permiso `Facturacion` |
| Estado (polimórfico `Tipo`) | `Administration/Domain/Status.cs` **plano** (Name/Code) | Mover + añadir `Tipo` + `ColorHex` |
| Cliente / Empresa / País | existen "delgados" (Name/Code) | Mover; enriquecer (campos ricos, opcional) |
| Proveedor | `Supplier` (Name/Code) | Mover + añadir `ColorHex` + `PrioridadVisual` |
| **Sociedad** | ❌ no existe | Crear (hija de Cliente) |
| **Prefijo (GRUPO→CATEGORIA + Activo)** | ❌ no existe | Crear (jerárquico autorreferenciado) |
| Reporte diario (§5/§6) | `/api/v1/cashflow` devuelve **ledger crudo**; el cliente agrega | Nuevo endpoint server-side agregado |
| Migraciones | Projects **y** Administration tienen migración inicial | Reemplazar por una migración `Cashflow` combinada |
| Seed (Estados por Tipo, País, Empresa) | ❌ no hay | Sembrar en `CashflowDbInitializer` |
| Calendario §7 | `dashboard/pages/projects/cashflow.tsx` (año único, sticky, dark ✅) | Rehacer a §7 (color proveedor, fines de semana, categorías, scroll por año) |
| Form Pago | sin selector Proveedor, sin edición | Añadir |

**Radio de impacto de retirar Projects + Administration (verificado):** cero dependencias entrantes.
Ningún otro módulo backend los referencia; la app `clients/admin` no los usa; no hay proyectos de test
dedicados; `Architecture.Tests` escanea `src/Modules` dinámicamente (cubre el módulo nuevo sin editar).
Solo hay que tocar los host + solución + migraciones (checklist en §8).

---

## 1. Identidad del módulo destino

- Proyectos: `src/Modules/Cashflow/Modules.Cashflow/` (runtime) + `Modules.Cashflow.Contracts/` (API pública).
- Schema EF: `cashflow`. Un único `CashflowDbContext : BaseDbContext` con **todos** los `DbSet`
  (transaccionales + catálogos). `CashflowDbInitializer` (migrate + seed).
- `[assembly: FshModule(typeof(CashflowModule), 650)]` (entre Catalog 600 y Tickets 700).
- Route group: `api/v{version:apiVersion}/cashflow` → los recursos cuelgan como
  `…/cashflow/projects`, `…/cashflow/incomes`, `…/cashflow/payments`, `…/cashflow/reports/daily-summary`,
  `…/cashflow/clients`, `…/cashflow/suppliers`, etc. (coincide con el prefijo `/api/v1/cashflow` de la spec §5).
- **Convención de nombres:** clases internas en inglés (Project, Income, Payment, Supplier, Society, Prefix…),
  **tablas/columnas en español** (`Proyectos`, `Ingresos`, `Importe`, `Confirmado`…), como ya hace el repo.

---

## 2. Modelo de datos consolidado

### 2.1 Transaccionales (movidos desde Projects, casi intactos)
- **Project** — Name, SalePrice?, ForecastSale?, Cost?, ForecastCost?, Profit?, + FKs ClientId, SocietyId?,
  CountryId, CompanyId?, StatusId, PrefixId. 1→N Note.
- **Income** — Amount, Description?, Date, Percentage?, ProjectId(FK), StatusId(FK), Confirmed, Validated.
- **Payment** — igual + SupplierId(FK).
- **Note** — ProjectId(FK), Title, Description?, Date.

### 2.2 Catálogos (movidos desde Administration + enriquecidos)
- **Status** — Name, `Tipo` (enum PROYECTO|INGRESO|PAGO, discriminador), **`ColorHex?`**. Un solo catálogo; filtrar por Tipo.
- **Supplier** — Name (+ campos ricos opcionales), **`ColorHex?`**, **`PrioridadVisual` (int)**.
- **Client** — Name (+ NifCif, Direccion, TipoCliente, Contacto?, RazonSocial?, Telefono?, Email?, FechaAlta?, ColorHex? — opcional), 1→N Society.
- **Company** — Name (+ RazonSocial, RegistroFiscal — opcional).
- **Country** — Name, CodigoIso.
- **Society** *(nuevo)* — Nombre, NifCif, Direccion, CodigoPostal, Ciudad, Pais(string), **ClientId(FK)**.
- **Prefix** *(nuevo, jerárquico)* — Nombre, Descripcion?, **PrefixGroupId → Prefix? (auto-ref, Restrict)**,
  `Tipo` (GRUPO|CATEGORIA), **Activo (bool)**. 1→N Project. Regla: no desactivar con proyectos asociados.

### 2.3 Relaciones / EF (beneficio de la consolidación)
Al vivir todo en un schema, los **soft-ref Guid pasan a FK reales** con `DeleteBehavior.Restrict`
(spec §2.4): Project→Client/Society?/Country/Company/Status/Prefix; Income→Project(NoAction)/Status;
Payment→Project(NoAction)/Supplier/Status; Prefix→Prefix (auto-ref Restrict). Hijos de colección
(Note, Society) → `Property(x=>x.Id).ValueGeneratedNever()` en su config (regla `database.md`).

---

## 3. Reglas de negocio a corregir (lecciones §10 de la spec)

1. **`Payment.Create` debe respetar `confirmado`** (hoy siempre arranca `false`). Alinear con `Income.Create`.
2. **`Update` genérico preserva `Confirmado`/`Validado`** — verificar que ni Income.Update ni Payment.Update
   los tocan; solo `SetConfirmado`/`SetValidado` los cambian.
3. **Normalización de fecha a `Unspecified` (medianoche) al crear *y* editar.** ✅ **DECIDIDO:** migrar
   `DateTimeOffset?` → `DateTime?` `Unspecified` (fiel a la spec, mata el day-shift). Propaga a
   entidades (Income/Payment/Note.Date), DTOs (`*Dto`, `Cashflow*`, `Resumen*`), la migración inicial
   (columna `timestamp without time zone`) y el frontend (parseo de fecha).
4. **Estados por defecto al crear:** Income/Payment → `PENDIENTE` de su Tipo; Project → `PREVISTO`
   (Tipo=PROYECTO) si no se indica otro. Requiere seed + lógica en los handlers de creación.
5. **Validadores backend de obligatorios del proyecto** (Cliente, País, Estado, Categoría) — no confiar solo en el form.
6. **Invalidar caché** `cache:ingreso:{id}` / `cache:pago:{id}` al confirmar/validar (si el GET-by-id se cachea; comprobar).

---

## 4. Reporte server-side (§5.5 / §6) — decisión tomada

- **Endpoint:** `GET /api/v1/cashflow/reports/daily-summary`, permiso `Cashflow.Reports.View`.
- **Params (todos opcionales):** `desde`, `hasta`, `empresaIds[]`, `proyectoIds[]`, `estadoIds[]`,
  `soloConfirmados`, `soloValidados`.
- **DTOs (Contracts):** `ResumenFinancieroPorProyectoDto` → `List<ResumenFinancieroDiarioDto>`
  (`Fecha, TotalIngresos, TotalPagos, Resultado, Acumulado, ColorHex, PrioridadVisual, DetallesIngresos[], DetallesPagos[]`)
  + `IngresoDetalleDto` / `PagoDetalleDto`.
- **Handler (algoritmo §6):** filtrar en la BD; agrupar por día y proyecto; `acumulado += (ingDia − pagDia)`;
  color/prioridad del **proveedor predominante del día** (mayor `PrioridadVisual`); mapear detalles.
  Validator obligatorio (query paginada/analítica).
- **Ledger crudo actual `/cashflow`:** mantenerlo (renombrado bajo el grupo, p. ej. `reports/ledger`) para
  la página de **Gráficos**, que consume datos por-movimiento; o migrar Gráficos al nuevo endpoint. Coexisten.

---

## 5. Permisos (§8)

`CashflowPermissions` con recursos: `Projects, Incomes, Payments, Notes, Statuses, Clients, Societies,
Suppliers, Companies, Countries, Prefixes, Reports, Facturacion`. Acciones estándar: `View (IsBasic),
Search, Create, Update, Delete, Export`. Recurso **`Facturacion`** con acciones custom
`ConfirmIngreso, ValidateIngreso, ConfirmPago, ValidatePago` — los endpoints `/confirm` y `/validate`
pasan a exigir el permiso de `Facturacion` (hoy están bajo `Incomes.Confirm`/`Payments.Confirm`).
Registrar con `PermissionConstants.Register(CashflowPermissions.All)` en `ConfigureServices`.
**Avanzado / diferible:** scoping por empresa `Permissions.Empresas.{empresaId}.{Action}` (spec §3/§9).

---

## 6. Frontend dashboard (§7)

- Repuntar los módulos de `src/api/*` a `/api/v1/cashflow/…`.
- **Calendario** (`pages/.../cashflow.tsx`) consumiendo `daily-summary`: color por proveedor predominante,
  **sombreado de fines de semana**, borde "hoy", **desglose por categoría** (Prefix GRUPO→CATEGORIA),
  **scroll infinito por año** (cargar año anterior/siguiente en los bordes), botón "Hoy", diálogo de detalle
  por celda con enlace al proyecto. Mantener sticky Día/cabeceras y colores del tema (dark) que ya funcionan.
- **Formularios:** selector de Proveedor en el form de Pago; dropdowns de Sociedad + Categoría (Prefix) en el
  form de Proyecto; diálogos de **edición** de Ingreso/Pago (hoy solo alta+borrado).
- **Nuevas páginas de maestros:** Sociedad, Prefijo (jerárquico); enriquecer Proveedor (color/prioridad) y
  Estado (Tipo + color). Reorganizar `nav-data.ts` bajo una sección "Flujo de caja / Cashflow".

---

## 7. Plan por fases

### Fase 1 — Scaffold `Cashflow` + consolidación estructural (big-bang de frontera)
> **✅ COMPLETADA (2026-07-14, rama `feat/cashflow-consolidation`, commit `5fbdf437`).** Verificado end-to-end:
> compila (0 errores C#), migración `InitialCashflow` aplicada → schema `cashflow` con las 9 tablas en la BD;
> `/api/v1/cashflow/*` responde 200 (autenticado) y las rutas viejas dan 404; Architecture.Tests 48/49
> (la única falla es naming de endpoints de Avicola, preexistente). Pendiente aparte: audit NuGet (bloquea
> CI; local resuelto en `iniciar.bat` con `NuGetAudit=false`) y el 500 de `/openapi` (paquete Microsoft.OpenApi).

Mover no admite medias tintas: un `DbContext` se migra entero.
1. Crear `Modules.Cashflow` + `.Contracts` (copiar 2 `.csproj` de un módulo existente y renombrar).
2. Mover dominio/data/features/contracts de Projects **y** Administration al módulo nuevo; un solo
   `CashflowDbContext` (schema `cashflow`) con todos los `DbSet`; `CashflowDbInitializer`.
3. `CashflowPermissions` consolidado; `CashflowModule : IModule` con version-set group `cashflow`.
4. Convertir soft-refs → **FK reales (Restrict)**.
5. Rewire de los **9 sitios** (§8) y **borrar** los árboles `Modules.Projects` + `Modules.Administration`
   y las carpetas de migración `Projects/` + `Administration/`.
6. Migración inicial fresca `--context CashflowDbContext` en `…/Migrations.PostgreSQL/Cashflow/`.
7. `dotnet build` (0 warnings) + `dotnet test src/Tests/Architecture.Tests` verdes.
- **Checkpoint:** DbMigrator aplica; endpoints responden bajo `/api/v1/cashflow/…`.

### Fase 2 — Catálogos nuevos + enriquecimientos
- `Status` +Tipo +ColorHex; `Supplier` +ColorHex +PrioridadVisual; `Society` (nuevo); `Prefix` (nuevo
  jerárquico); campos ricos de `Client` (opcional). Slices CRUD+search de Society y Prefix; ampliar CRUD de
  Supplier/Status/Client. Migración de columnas/entidades nuevas. **Seed:** Estados por Tipo
  (PROYECTO: PREVISTO…; INGRESO: PENDIENTE/CONFIRMADO; PAGO: PENDIENTE/CONFIRMADO), ≥1 País, ≥1 Empresa.

### Fase 3 — Correcciones de reglas (§3 de este plan / §10 spec)
- Payment.Create respeta `confirmado`; Update preserva flags; normalización de fecha; estados por defecto;
  recurso `Facturacion` en confirm/validate; invalidación de caché; validadores de obligatorios del proyecto.

### Fase 4 — Reporte server-side
- DTOs + `GetDailySummaryQuery` + validator + handler (algoritmo §6) + endpoint `reports/daily-summary`
  (permiso `Reports.View`). Ajustar/renombrar el ledger crudo para Gráficos.

### Fase 5 — Frontend (§6 de este plan / §7 spec)
- Repuntar APIs; calendario a `daily-summary` con color/fines de semana/categorías/scroll por año/detalle;
  selector Proveedor en Pago; Sociedad+Categoría en Proyecto; edición de Ingreso/Pago; páginas Society/Prefix;
  enriquecer Supplier/Status; nav consolidado.

### Fase 6 — Verificación (§9 spec)
- Recorrer el end-to-end §9 (crear proyecto → 400 sin obligatorios; ingreso/pago sin shift de fecha;
  confirmar/validar → 403 sin permiso; calendario con cada filtro; móvil/dark). Añadir `Cashflow.Tests`
  (algoritmo del reporte + reglas de facturación) — si se crea, añadirlo a la lista de `.github/workflows/backend.yml` L99.
  Actualizar **docs repo + changelog** (golden rule #10).

---

## 8. Checklist de sitios a editar (retirada de los 2 módulos → 1)

1. `src/Host/FSH.Starter.Api/FSH.Starter.Api.csproj` L41–44 — 4 refs → 2 refs Cashflow.
2. `src/Host/FSH.Starter.DbMigrator/FSH.Starter.DbMigrator.csproj` L47–50 — 4 refs → 2.
3. `src/Host/FSH.Starter.Migrations.PostgreSQL/FSH.Starter.Migrations.PostgreSQL.csproj` L19–20 — 2 refs → 1.
4. `src/Host/FSH.Starter.Api/Program.cs` — markers L62–65 + assemblies L87–88.
5. `src/Host/FSH.Starter.DbMigrator/Program.cs` — markers L120–123 + assemblies L146–147.
6. `src/FSH.Starter.slnx` — carpetas/proyectos L15–17 (Administration) + L55–57 (Projects).
7. `…/Migrations.PostgreSQL/Projects/` (3 archivos) + `…/Administration/` (3 archivos) — borrar; nueva `Cashflow/`.
8. Mover árboles `src/Modules/Projects/**` + `src/Modules/Administration/**` → `src/Modules/Cashflow/**`.
9. Opcional: `.github/workflows/backend.yml` L99 solo si se añade `Cashflow.Tests`.

Sin cambios: otros módulos backend, proyectos de test (scan dinámico), `clients/admin`, seeders externos,
AppHost/appsettings/otros CI (todos falsos positivos verificados).

---

## 9. Decisiones (resueltas 2026-07-14)

1. **Datos existentes:** ✅ no hay datos financieros reales → recrear la BD de dev limpia vía DbMigrator
   (schemas `projects`/`administration` desaparecen; nace `cashflow`).
2. **Tipo de fecha:** ✅ migrar `DateTimeOffset` → `DateTime` `Unspecified`.
3. **Rutas:** ✅ recursos en inglés bajo el grupo (`/api/v1/cashflow/projects`, `/incomes`, …); el frontend se repunta.

## 10. Riesgos

- **Big-bang de frontera de módulo:** la Fase 1 no se puede partir a medias; hacerla en rama y mantener el
  build verde al cerrar la fase. El reset de migraciones implica recrear la BD de dev.
- **Cambio de tipo de fecha** (si se adopta) propaga a entidades/DTOs/migración/frontend — decidir en §9.1.
- **Rework del calendario** (transpuesto→año→mes→día con scroll infinito) es el mayor esfuerzo de frontend.
- **Permisos por empresa** (§5 avanzado) probablemente requiere apoyo del sistema de permisos; diferir.
