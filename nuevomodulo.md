# Módulo `cashflow` — Especificación técnica para construir desde cero

> **Para quien lo va a implementar (Claude Code):** este documento es la **fuente única de verdad** para construir, desde cero, un módulo autocontenido llamado **`cashflow`** (Flujo de Caja) que gestiona **proyectos**, sus **ingresos** (cobros) y **gastos/pagos** (a proveedores), y los visualiza en un **calendario anual de flujo de caja**. No necesitas otro contexto. Los nombres de dominio van en español a propósito (Proyecto, Ingreso, Pago…).

---

## 0. Objetivo y stack destino

Construir un módulo de negocio financiero para una empresa de estructuras fotovoltaicas. Alrededor del **Proyecto** se registran ingresos y pagos, y se consolidan en un calendario diario que muestra, por día: dinero que entra, dinero que sale y saldo acumulado.

**Stack destino recomendado: FullStackHero v10 (.NET 10).** Convenciones clave a seguir:

- **Modular monolith**: un módulo = **proyecto runtime** + **proyecto `.Contracts`** (única superficie pública; DTOs/contratos que otros módulos/el front consumen).
- **Vertical slice**: una feature vive en **una carpeta** (Command/Query + Handler + Validator + Endpoint + Tests).
- **CQRS con Mediator source-generated** (Martinothamar/Mediator), **no** MediatR.
- **Minimal APIs** para endpoints; **EF Core 10 + PostgreSQL**, un `DbContext` por módulo; migraciones vía **DbMigrator** (consola aparte; sin auto-migrate al arrancar).
- **JWT + permisos por recurso/acción**.
- **Frontend React 19 + Vite + TypeScript + Tailwind + shadcn/ui + TanStack Query v5** (el kit v10 ya no usa Blazor).

> El **núcleo** de este documento (modelo de datos, reglas, contrato de API, algoritmo del reporte, comportamiento de la UI) es **independiente del stack**. Si se construye sobre otra base, solo cambia el "plumbing" (§ marcadas como *convención v10*).

---

## 1. Alcance del módulo

**Incluido (núcleo del módulo `cashflow`):**
- Entidades transaccionales: **Proyecto**, **Ingreso**, **Pago**.
- Catálogo de estados **Estado** (polimórfico por `Tipo`).
- Maestros de apoyo necesarios para operar: **Cliente**, **Sociedad**, **Proveedor**, **Empresa**, **Pais**, **Prefijo** (grupo/categoría), **Nota**.
- **Reporte de flujo de caja diario** + su **calendario** con filtros.
- **Confirmar / Validar** movimientos (control de facturación).

**Opcional (fuera del núcleo; añadir solo si se pide):** importación de extractos bancarios (Banco, MovimientoBanco) y reportes gráficos secundarios (comparativa diaria, facturación por cliente).

---

## 2. Modelo de dominio

Todas las entidades derivan de la base de auditoría del framework: `Id` (GUID, PK), `Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`, y `TenantId` (multi-tenant). Abajo se listan solo los **campos propios**. Cada entidad expone métodos de fábrica `Create(...)` y `Update(...)` (no setters públicos libres).

### 2.1 Núcleo transaccional

**Proyecto** — entidad central
| Campo | Tipo | Notas |
|---|---|---|
| Nombre | string | **obligatorio** |
| PrecioVenta, VentaPrevista, Coste, CostePrevisto, Beneficio | decimal? | importes; Beneficio = PrecioVenta − Coste |
| ClienteId | GUID? → Cliente | **obligatorio** en validación |
| SociedadId | GUID? → Sociedad | opcional |
| PaisId | GUID? → Pais | **obligatorio** |
| EmpresaId | GUID? → Empresa | eje de seguridad/scoping |
| EstadoId | GUID? → Estado (Tipo=PROYECTO) | **obligatorio** |
| PrefijoCategoriaId | GUID? → Prefijo (Tipo=CATEGORIA) | **obligatorio** |
| Notas | 1—N Nota | colección |

**Ingreso** — cobro
| Campo | Tipo | Notas |
|---|---|---|
| Importe | decimal | **obligatorio** > 0 |
| Descripcion | string? | |
| Fecha | DateTime? | día del cobro (ver regla de zona horaria) |
| Porcentaje | decimal? | 0–100 |
| ProyectoId | GUID? → Proyecto | **obligatorio** |
| EstadoId | GUID? → Estado (Tipo=INGRESO) | **obligatorio** |
| Confirmado | bool | cobro realizado |
| Validado | bool | existe fecha + factura |

**Pago** — gasto (a proveedor)
| Campo | Tipo | Notas |
|---|---|---|
| Importe | decimal | **obligatorio** > 0 |
| Descripcion | string? | |
| Fecha | DateTime? | día del pago |
| Porcentaje | decimal? | 0–100 |
| ProveedorId | GUID? → Proveedor | **obligatorio**; aporta color al calendario |
| ProyectoId | GUID? → Proyecto | **obligatorio** |
| EstadoId | GUID? → Estado (Tipo=PAGO) | **obligatorio** |
| Confirmado | bool | pago realizado |
| Validado | bool | existe fecha + factura |

**Métodos de dominio de Ingreso y Pago (importante):**
- `Create(...)` recibe y **respeta** el parámetro `confirmado` (no lo fuerces a false).
- `Update(...)` de instancia actualiza los campos editables.
- **`SetConfirmado(bool confirmado, Guid? estadoId)`**: cambia `Confirmado` y, si viene, el `EstadoId`. Encola evento *Updated*.
- **`SetValidado(bool validado)`**: cambia `Validado`. Encola evento *Updated*.
- El `Update` **genérico NO debe tocar `Confirmado`/`Validado`** (los preserva); esos flags solo cambian por `SetConfirmado`/`SetValidado`.

### 2.2 Estado (polimórfico)
| Campo | Tipo | Notas |
|---|---|---|
| Tipo | string | `PROYECTO` \| `INGRESO` \| `PAGO` (discrimina) |
| Nombre | string | p. ej. PREVISTO, PENDIENTE, CONFIRMADO |
| ColorHex | string? | |

Un único catálogo sirve a proyectos, ingresos y pagos. Las pantallas/autocompletes filtran por `Tipo`.

### 2.3 Maestros de apoyo

**Cliente**: Nombre (req), NifCif (req), Direccion (req), TipoCliente (req), Contacto?, RazonSocial?, Telefono?, Email?, FechaAlta?, ColorHex?; **1—N Sociedad**.
**Sociedad**: Nombre, NifCif, Direccion, CodigoPostal, Ciudad, Pais (string), **ClienteId → Cliente**.
**Proveedor**: Nombre (req), NifCif (req), Direccion (req), TipoProveedor (req), Contacto?, RazonSocial?, Telefono?, Email?, FechaAlta?, **ColorHex?**, **PrioridadVisual (int)**.
**Empresa**: Nombre, RazonSocial, RegistroFiscal.
**Pais**: Nombre, CodigoIso, Descripcion?.
**Prefijo** (jerárquico): Nombre, Descripcion, **PrefijoGrupoId → Prefijo? (auto-referencia)**, Tipo (`GRUPO` \| `CATEGORIA`), Activo (bool); 1—N Proyecto. Regla: no desactivar un Prefijo con proyectos asociados.
**Nota**: Fecha, Titulo, Descripcion?, **ProyectoId → Proyecto**.

### 2.4 Relaciones (resumen)
- `Proyecto` N→1 `Cliente`, `Sociedad?`, `Pais`, `Empresa`, `Estado`, `Prefijo (categoría)`; `Proyecto` 1→N `Nota`.
- `Cliente` 1→N `Sociedad`.
- `Ingreso` N→1 `Proyecto`, `Estado(INGRESO)`.
- `Pago` N→1 `Proyecto`, `Proveedor`, `Estado(PAGO)`.
- `Prefijo` N→1 `Prefijo` (CATEGORIA→GRUPO); `Prefijo` 1→N `Proyecto`.
- FKs a catálogos con **`DeleteBehavior.Restrict`** (no borrar en cascada).

---

## 3. Reglas de negocio (obligatorias)

1. **Confirmado ≠ Validado.** Son dos banderas independientes. "Confirmado" = el dinero se movió; "Validado" = hay fecha + factura. Solo se cambian por los endpoints dedicados de facturación (con permiso); una edición normal las conserva.
2. **Estado compartido por Tipo.** Un solo catálogo `Estado`; filtrar por `Tipo` (PROYECTO/INGRESO/PAGO) en cada contexto.
3. **Color del calendario por proveedor.** Cada día con pagos toma el color del **proveedor predominante** = el de **mayor `PrioridadVisual`** entre los pagos de ese día. `ColorHex`/`PrioridadVisual` viajan en el reporte.
4. **Saldo acumulado corrido.** Por proyecto y día: `acumulado += (ingresosDia − pagosDia)`. La columna "Totales" del calendario muestra el acumulado combinado de los proyectos seleccionados.
5. **Campos obligatorios del proyecto.** No crear proyecto sin Cliente, País, Estado y Categoría (validar en servidor **y** en el formulario).
6. **Estados por defecto al crear.** Ingreso/Pago nuevos → Estado `PENDIENTE` de su Tipo si no se indica otro; Proyecto nuevo → `PREVISTO` (Tipo=PROYECTO).
7. **Normalización de fecha.** Al **crear y editar** ingresos/pagos, fijar la fecha a medianoche `Unspecified` (sin zona horaria) para que no se desplace un día al serializar a UTC.
8. **Recálculo venta/coste/beneficio.** El detalle de proyecto puede recomputar `PrecioVenta` (suma de ingresos) y `Coste` (suma de pagos) **solo para mostrar**; **no** persistir automáticamente en cada carga (solo al Guardar explícito).
9. **Aislamiento por empresa/tenant.** Datos bajo un tenant; los permisos pueden acotarse por empresa (`Permissions.Empresas.{empresaId}.{Action}` allow/deny) resolviendo la empresa desde el proyecto.

---

## 4. Persistencia

- Un `DbContext` del módulo con `DbSet` para cada entidad; esquema lógico `cashflow` (o `catalog`).
- Configuración EF: claves, `IsMultiTenant()` en las entidades tenant, FKs a catálogos con `DeleteBehavior.Restrict`, auto-referencia de `Prefijo` con `Restrict`.
- Repositorio genérico con proyección a DTO (evitar traer entidades completas cuando se devuelve un response).
- **Migraciones vía DbMigrator** (consola), no auto-migrate. *(convención v10)*
- **Datos semilla obligatorios** (sin ellos el flujo no opera): estados
  - Tipo `PROYECTO`: `PREVISTO`, y los que uses (EN CURSO, CERRADO…).
  - Tipo `INGRESO`: `PENDIENTE`, `CONFIRMADO`.
  - Tipo `PAGO`: `PENDIENTE`, `CONFIRMADO`.
  - Además al menos un País y una Empresa para poder crear proyectos.

---

## 5. Contrato de API REST

Prefijo sugerido: **`/api/v1/cashflow`** (limpio; no repitas segmentos de grupo). Todos los endpoints requieren JWT y el permiso indicado. Los `search` son POST con `PaginationFilter` (pageNumber, pageSize, keyword, advancedSearch/advancedFilter, orderBy).

### 5.1 Proyectos
| Método | Ruta | Permiso |
|---|---|---|
| POST | `/proyectos` | `Permissions.Proyectos.Create` |
| GET | `/proyectos/{id}` | `Permissions.Proyectos.View` |
| POST | `/proyectos/search` | `Permissions.Proyectos.View` |
| PUT | `/proyectos/{id}` | `Permissions.Proyectos.Update` |
| DELETE | `/proyectos/{id}` | `Permissions.Proyectos.Delete` |

`CreateProyectoCommand`: `{ Nombre, PrecioVenta?, VentaPrevista?, Coste?, CostePrevisto?, Beneficio?, ClienteId?, PaisId?, EmpresaId?, EstadoId?, SociedadId?, PrefijoCategoriaId? }` — validar Nombre, ClienteId, PaisId, EstadoId, PrefijoCategoriaId no vacíos.
`ProyectoResponse`: incluye los ids **y** los nombres desnormalizados: `{ Id, Nombre, PrecioVenta, VentaPrevista, Coste, CostePrevisto, Beneficio, ClienteId, ClienteNombre, SociedadId, SociedadNombre, PaisId, PaisNombre, PrefijoCategoriaId, PrefijoCategoriaNombre, EmpresaId, EmpresaNombre, EstadoId, EstadoNombre }`.

### 5.2 Ingresos
CRUD+search igual que proyectos bajo `/ingresos` con `Permissions.Ingresos.*`, **más**:
| Método | Ruta | Permiso |
|---|---|---|
| POST | `/ingresos/{id}/confirmar` | `Permissions.Facturacion.ConfirmIngreso` |
| POST | `/ingresos/{id}/validar` | `Permissions.Facturacion.ValidateIngreso` |

`CreateIngresoCommand`: `{ Importe, Descripcion?, Fecha?, Porcentaje?, ProyectoId?, EstadoId?, Confirmado }` (validar Importe>0, Fecha not null, Porcentaje 0–100, ProyectoId/EstadoId no vacíos).
`ConfirmIngresoCommand`: `{ Id, Confirmado, EstadoId? }` → handler llama `SetConfirmado` + invalida caché del ingreso.
`ValidateIngresoCommand`: `{ Id, Validado }` → `SetValidado` + invalida caché.
`IngresoResponse`: `{ Id, Importe, Descripcion, Fecha, Porcentaje, ProyectoId, EstadoId, Confirmado, Validado }`.

### 5.3 Pagos
CRUD+search bajo `/pagos` con `Permissions.Pagos.*`, más `/pagos/{id}/confirmar` (`Facturacion.ConfirmPago`) y `/pagos/{id}/validar` (`Facturacion.ValidatePago`).
`CreatePagoCommand`: `{ Importe, Descripcion?, Fecha?, Porcentaje?, ProveedorId?, ProyectoId?, EstadoId?, Confirmado }` (validar además ProveedorId no vacío).
`PagoResponse`: `{ Id, Importe, Descripcion, Fecha, Porcentaje, ProveedorId, ProyectoId, EstadoId, Confirmado, Validado }`.

### 5.4 Maestros de apoyo
CRUD+search estándar para: `/estados`, `/clientes`, `/sociedades`, `/proveedores`, `/empresas`, `/paises`, `/prefijos`, `/notas`, con permisos `Permissions.{Recurso}.{Create|View|Update|Delete}`.

### 5.5 Reporte de flujo de caja (clave)
| Método | Ruta | Permiso |
|---|---|---|
| GET | `/reportes/resumen-diario` | `Permissions.Reportes.View` |

**Query params (todos opcionales; sin ellos = todo el histórico):**
`desde` (date), `hasta` (date), `empresaIds` (guid[]), `proyectoIds` (guid[]), `estadoIds` (guid[]), `soloConfirmados` (bool), `soloValidados` (bool).

**Respuesta** `List<ResumenFinancieroPorProyectoDto>`:
```
ResumenFinancieroPorProyectoDto {
  Guid ProyectoId; string NombreProyecto, NombreCliente, NombreEmpresa, NombrePais, NombreEstado;
  Guid? EstadoId;
  List<ResumenFinancieroDiarioDto> Resumenes;
}
ResumenFinancieroDiarioDto {
  DateTime Fecha;
  decimal TotalIngresos, TotalPagos;
  decimal Resultado;          // = TotalIngresos - TotalPagos
  decimal Acumulado;          // running sum por proyecto
  string ColorHex;            // default "#ffffff00" (transparente)
  int    PrioridadVisual;     // default int.MaxValue
  List<IngresoDetalleDto> DetallesIngresos;
  List<PagoDetalleDto>    DetallesPagos;
}
IngresoDetalleDto { Guid Id; decimal Importe; decimal Porcentaje; string Estado; string Descripcion; bool Confirmado; bool Validado; }
PagoDetalleDto    { Guid Id; decimal Importe; decimal Porcentaje; string Estado; string NombreProveedor; string Descripcion; bool Confirmado; bool Validado; }
```

---

## 6. Algoritmo del reporte (servidor)

`GetResumenFinancieroDiarioHandler` (o su equivalente vertical-slice). **Filtrar en la base de datos** (no traer todo a memoria):

```
1. Cargar ingresos y pagos aplicando los filtros:
   - Fecha >= desde  y  Fecha < hasta+1día
   - Proyecto.EmpresaId ∈ empresaIds        (si viene)
   - ProyectoId ∈ proyectoIds               (si viene)
   - Proyecto.EstadoId ∈ estadoIds          (estado del PROYECTO; si viene)
   - Confirmado == true                      (si soloConfirmados)
   - Validado == true                        (si soloValidados)
2. proyectosIds = distinct(ingresos.ProyectoId ∪ pagos.ProyectoId)
3. Cargar proyectos (con Cliente/Empresa/Pais/Estado) y proveedores y estados de esos ids.
4. Por cada proyecto:
   a. ingresosPorDia = group ingresos by Fecha.Date -> sum(Importe)
   b. pagosPorDia    = group pagos    by Fecha.Date -> { Total=sum(Importe), Proveedores=[...] }
   c. fechas = union(keys) ordenadas asc
   d. acumulado = 0
      por cada fecha:
         ingDia = ingresosPorDia[fecha] ?? 0
         pagDia = pagosPorDia[fecha].Total ?? 0
         acumulado += (ingDia - pagDia)
         proveedorPredominante = pagos del día con MAYOR PrioridadVisual
         ColorHex = proveedorPredominante?.ColorHex ?? "#ffffff00"
         PrioridadVisual = proveedorPredominante?.PrioridadVisual ?? int.MaxValue
         DetallesIngresos/DetallesPagos = mapear los movimientos del día
         añadir ResumenFinancieroDiarioDto
   e. añadir ResumenFinancieroPorProyectoDto (con nombres desnormalizados)
5. devolver la lista.
```

> Nota: el acumulado se calcula **por proyecto**. El acumulado combinado del calendario se arma en el cliente sumando los proyectos seleccionados (ver §7).

---

## 7. UI del flujo de caja — especificación de comportamiento (destino React)

Pantalla **"Flujo de caja"** (ruta sugerida `/cashflow/report`). Consume el endpoint §5.5 vía TanStack Query. **Reescribir en React**; abajo el comportamiento a replicar (no portar componentes Blazor).

### 7.1 Filtros
- **Empresa / Proyecto / Estado**: selectores multi-selección. Determinan qué proyectos se muestran (y se envían como `empresaIds/proyectoIds/estadoIds` solo si el usuario deseleccionó algo).
- **Año**: acota `desde`/`hasta` a ese año.
- **Solo confirmados / Solo validados**: switches → `soloConfirmados`/`soloValidados`.
- Botón **Aplicar**: dispara la recarga del reporte (no una llamada por cada clic).

### 7.2 Calendario (tabla año → mes → día)
- Estructura de datos en cliente: `matriz[año][mes][día] = { TotalIngresos, TotalPagos, Acumulado, DetallesIngresos[], DetallesPagos[], ColorProveedor }`.
- **Combinar proyectos seleccionados**: por cada proyecto incluido, sumar sus `ResumenFinancieroDiarioDto` a la celda del día (`+=` totales, arrastrar acumulado); al deseleccionar, restar. El color del día lo fija el proveedor de mayor prioridad presente.
- **Render**: una fila por día (1–31) y, por cada mes visible, **3 columnas**: `Pagos`, `Ingresos`, `Totales` (acumulado). Fila final "Total" por mes (Σ pagos, Σ ingresos, Σ ingresos−pagos).
- **Desglose por categoría**: bajo el calendario, tabla de totales de pagos agrupados por **categoría de Prefijo** (GRUPO→CATEGORIA); clic abre el detalle del mes.
- **Detalle por celda**: clic en una celda de Pagos/Ingresos abre un diálogo con la lista de movimientos de ese día (importe, %, estado, proveedor/descripcion), con enlace al detalle del proyecto.
- **Pistas visuales**: celda "Pagos" pintada con el color del proveedor predominante; fines de semana sombreados; borde en el día "hoy".
- **Navegación**: scroll horizontal; carga de años anterior/siguiente al llegar a los bordes (scroll "infinito" por año); botón "Hoy" que centra el mes actual.
- **Responsive**: la **columna "Día" y las cabeceras deben quedar fijas (sticky)** al hacer scroll; columnas compactas en móvil; **usar variables de color del tema** (no hardcodear grises) para soportar modo oscuro.

---

## 8. Permisos

Formato: `Permissions.{Recurso}.{Accion}`. Sembrar a los roles (Admin = todo; Basic = solo View/Search de lectura).

- **Recursos**: `Proyectos`, `Ingresos`, `Pagos`, `Estados`, `Reportes`, **`Facturacion`**, y apoyo `Clientes`, `Sociedades`, `Proveedores`, `Empresas`, `Paises`, `Prefijos`, `Notas`.
- **Acciones estándar**: `View`, `Search`, `Create`, `Update`, `Delete`, `Export`.
- **Acciones custom de facturación** (sobre el recurso `Facturacion`): `ConfirmIngreso`, `ValidateIngreso`, `ConfirmPago`, `ValidatePago`.
- **Enforcement en el servidor**: los endpoints `/confirmar` y `/validar` exigen el permiso de `Facturacion` correspondiente. **No** dejar la comprobación solo en el cliente. `Reportes.View` protege el reporte.

---

## 9. Recorrido end-to-end (para verificar la construcción)

1. **Crear proyecto**: elegir Empresa → nuevo Proyecto (Cliente, País, Estado, Categoría obligatorios; estado inicial PREVISTO). Sin obligatorios → **400**.
2. **Registrar ingreso** y **pago** en el proyecto (fecha correcta, sin desplazamiento de día).
3. **Confirmar/Validar** con permiso de Facturación → cambia el flag; **sin** ese permiso → **403**. El `PUT` genérico **no** altera los flags.
4. **Ver flujo de caja**: el calendario muestra pagos/ingresos/acumulado del proyecto; probar cada filtro (año, empresa, proyecto, estado, solo confirmados).
5. **Móvil**: columna Día y cabeceras fijas; sin desbordes; modo oscuro correcto.

---

## 10. Decisiones y correcciones a incorporar (lecciones ya aprendidas)

Construir el módulo ya con estas decisiones (evitan bugs conocidos del sistema original):
- `Ingreso.Create` **respeta** el flag `confirmado`.
- Normalización de fecha a `Unspecified` **también al crear** (no solo al editar).
- Endpoints dedicados **confirmar/validar** con permiso server-side; el update genérico **preserva** `Confirmado`/`Validado`.
- **Invalidar la caché** del ingreso/pago (`cache:ingreso:{id}` / `cache:pago:{id}`) al confirmar/validar, si el `GET by id` se cachea.
- Validadores backend de campos obligatorios del proyecto (no confiar solo en el formulario).
- Calendario **responsive** con columna/cabeceras sticky y **colores del tema** (soporta dark mode).
- No recrear componentes muertos ni rutas de prueba; no duplicar el segmento de ruta del reporte.

---

## 11. Plan de construcción por fases (sugerido)

1. **Dominio + persistencia**: entidades, `DbContext`, configuración EF, migración inicial (DbMigrator), seed de estados/país/empresa.
2. **Maestros**: CRUD+search de Estado, Cliente, Sociedad, Proveedor, Empresa, Pais, Prefijo, Nota (vertical slices).
3. **Transaccional**: Proyecto, Ingreso, Pago (CRUD+search) + confirmar/validar + permisos de Facturación.
4. **Reporte**: endpoint `/reportes/resumen-diario` + algoritmo §6 + DTOs.
5. **Frontend React**: maestros básicos, alta de proyecto/ingreso/pago, y la pantalla de flujo de caja (§7) con TanStack Query.
6. **Verificación**: recorrer §9 de punta a punta.

Verificar cada fase compilando y ejecutando; el reporte y el enforcement de permisos son los puntos de mayor riesgo — probarlos con datos reales.
