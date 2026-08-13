# Facturación — módulo `Cashflow` (FSH .NET 10)

> Estado: **implementado** en la rama `feat/cashflow-consolidation` (falta aplicar la migración EF, ver §11).
> Este documento describe lo que se construyó y cómo mapea al plan original. El diseño conceptual
> (Factura 1→N líneas, value object de forma de pago, conciliación) se conservó; los nombres y
> convenciones se **realinearon al código real** del módulo.

## Objetivo

Añadir la entidad `Invoice` (Factura) al módulo `Modules.Cashflow` y **vincular cada `Income` (Ingreso)
y cada `Payment` (Pago) con la factura fiscal que le corresponde**, para pasar de "importes sueltos"
a facturación real con documento, vencimientos y pendiente de cobro/pago.

Decisión de diseño (no reabrir): **una sola tabla `Facturas`** con discriminador `Tipo`
(`Emitida` | `Recibida`). No dos tablas.

---

## 0. Realineación con el código real (lo que cambió respecto al plan original)

El plan original estaba escrito contra un codebase hipotético en español y schema `catalog`. El módulo
real es distinto; se corrigió así:

| Plan original | Implementación real |
|---|---|
| schema `catalog` | schema **`cashflow`** (columnas de BD sí en español: `Facturas`, `Numero`, `Tipo`, `FacturaId`…) |
| `Factura`, `Ingreso`, `Pago` (C#) | `Invoice`, `Income`, `Payment` (entidades en inglés, columnas mapeadas con `.HasColumnName`) |
| `AuditableEntity, IAggregateRoot`, `Guid.NewGuid()` | `AggregateRoot<Guid>` + **`Guid.CreateVersion7()`** |
| **FKs duras** a Clientes/Proveedores/Empresas/Sociedades/Estados | **soft-refs** (Guid indexado, sin FK) como el resto del módulo; solo `Income/Payment → Invoice` es FK real |
| layout `Domain/Application/Infrastructure/Endpoints` | **vertical slice**: `Features/v1/Invoices/{Operación}/` + `Contracts` aparte |
| índice único `(Numero, Tipo)` | igual, pero Finbuckle lo scopa **por tenant** automáticamente (`AdjustUniqueIndexes`) |
| Importador Excel (§7-§8 originales) | **descartado como subsistema in-app** (ver §10) |

---

## 1. Modelo de relación

Factura **1 → N líneas** (Incomes o Payments). NO es 1:1.

```
Invoice (Emitida)  ──1:N──►  Income  ──► Project ──► Client
Invoice (Recibida) ──1:N──►  Payment ──► Project
                                      └─► Supplier
```

- El FK vive en el hijo: `Income.InvoiceId` / `Payment.InvoiceId`, **nullable** (un hito previsto existe
  antes de emitir la factura → respeta el ciclo forecast→real que ya modelan `Confirmed`/`Validated`).
- `ON DELETE SET NULL`: borrar una factura desvincula sus líneas y las devuelve a forecast; no borra su
  impacto en el proyecto.
- Cada línea lleva su propio `ProjectId`, así que una factura puede repartirse entre proyectos.

## 2. Regla de agregados (DDD)

`Invoice`, `Income` y `Payment` son **cada uno su propio aggregate root**. `Invoice` **no** expone
`List<Income>`/`List<Payment>` navegable; las líneas se referencian por identidad (`InvoiceId`) y se
cargan con un **query del slice** (`GetInvoiceLinesQuery`), nunca con `.Include()`.

## 3. Persistencia (schema `cashflow`)

Tabla `Facturas` (`InvoiceConfiguration`):
`Numero`, `NumeroDynamics`, `Tipo` (string, check `Emitida`/`Recibida` con contraparte no nula),
`FechaFactura`, `FechaVencimiento`, `ClienteId`/`ProveedorId`/`EmpresaId`/`SociedadId`/`EstadoId`
(soft-refs indexados), `BaseImponible`, `Iva`, `Total`, `FormaPago` (VO → string), `Banco`,
`Comprobada`, `Notas`. Índice único por-tenant `(Numero, Tipo)`.

Columnas añadidas: `Ingresos.FacturaId` y `Pagos.FacturaId` (FK real a `Facturas`, `ON DELETE SET NULL`).

**Unicidad de `Numero` — asimétrica** (aprendido de los ledgers reales 2026, migración
`AlignInvoiceNumberUniqueness`): la numeración **Emitida** es nuestra → único parcial
`(Numero, TenantId) WHERE Tipo='Emitida'`; la numeración **Recibida** es del proveedor —
proveedores distintos reutilizan el mismo número y los pares cargo/abono comparten número —
→ sin unicidad, solo índice de lookup. La dedupe de importación va por firma exacta
(Numero, Tipo, Total, Fecha), no por schema. Las notas de abono emitidas llevan su propia
serie que pisa la de facturas → se cargan con prefijo `NA-` (original en Notas).

## 4. Entidad de dominio

`Domain/Invoice.cs` — `AggregateRoot<Guid>`, factorías `Invoice.Issued(...)` / `Invoice.Received(...)`,
`Update(...)`, `MarkVerified()`, `SetStatus(...)`. `Total` admite negativos (abonos/rectificativas).
`Income`/`Payment` ganan `InvoiceId` + `LinkInvoice(Guid?)` (fuera de `Update`, para que un edit normal
nunca re-enlace en silencio).

## 5. Value object `PaymentTerms` (forma de pago) — implementado y testeado

`Domain/PaymentTerms.cs`. Traduce el código en hitos con `%`, disparador (`Prepaid`/`InvoiceDate`) y
offset de días. Gramática (regex por alternativa):

| Código | Hitos |
|---|---|
| `0D`/`30D`/`60D` | 100% a fecha factura + N |
| `100PP` | 100% prepago |
| `30PP70-60D` / `50PP50-60D` | a% prepago + b% a +N |
| `50PP50PP` | a% prepago + b% prepago |
| `DOMICILIADO` | 100% a fecha factura +0 (marca `IsDirectDebit`) |

`Parse`/`TryParse` (rechaza códigos desconocidos y sumas ≠100 ±0.01). `GenerateMilestones(total, fecha)`
reparte a céntimos y el último hito absorbe el redondeo (siempre re-suma al total). Persistido como su
`Code` string vía value converter EF. **23 tests** en `src/Tests/Cashflow.Tests` (verdes).

## 6. Slices (CQRS, `Features/v1/Invoices/`)

Endpoints bajo `api/v1/cashflow`:

| Slice | Verbo/ruta | Permiso |
|---|---|---|
| SearchInvoices | `GET /invoices` (filtros: type, client, supplier, company, search, paginado) | `Facturacion.View` |
| CreateInvoice | `POST /invoices` | `Facturacion.Create` |
| GetInvoiceById | `GET /invoices/{id}` | `Facturacion.View` |
| GetInvoiceLines | `GET /invoices/{id}/lines` (líneas + LinkedAmount/ValidatedAmount/Pending, §9) | `Facturacion.View` |
| UpdateInvoice | `PUT /invoices/{id}` | `Facturacion.Update` |
| DeleteInvoice | `DELETE /invoices/{id}` | `Facturacion.Delete` |
| LinkLineToInvoice | `POST /invoices/link-line` (`{lineId, kind, invoiceId}`; null = desvincular) | `Facturacion.Update` |
| GenerateMilestones | `POST /invoices/{id}/generate-milestones` (`{projectId?}`) | `Facturacion.Create` |
| MarkInvoiceVerified | `POST /invoices/{id}/verify` | `Facturacion.Update` |

`LinkLineToInvoice` valida el lado (Income↔Emitida, Payment↔Recibida). `GenerateMilestones` crea N
líneas forecast (`Confirmed=false`) desde `PaymentTerms`, idempotente (rechaza si ya hay líneas).

## 7. Permisos y estados

Recurso `Facturacion` ampliado con `View/Create/Update/Delete` (junto a los `Confirm*/Validate*` ya
existentes). `StatusType` gana `FacturaEmitida`/`FacturaRecibida`; `CashflowDbInitializer` siembra sus
estados (Borrador/Emitida/Cobrada… y Pendiente/Pagada…).

## 8. Calidad de datos (reglas conservadas)

`decimal` para todo importe (nunca `double`). Abonos = `Total` negativo. `ProjectId` nullable en las
líneas (overhead sin proyecto entra en caja, no en margen). El pendiente NO se persiste: se deriva como
`Total − Σ líneas validadas` (`GetInvoiceLinesQuery`).

## 9. Conciliación / facturación real

- `GetInvoiceLinesQuery` cierra el círculo factura ↔ líneas: devuelve importes vinculado/validado/pendiente.
- **Margen por proyecto** ya existe vía el reporte server-side de caja (Fase 4) porque `Income`/`Payment`
  ya llevan `ProjectId`. La factura añade la capa fiscal (nº, vencimiento, pendiente), no el margen.
- **Conciliación bancaria** (cruzar `Payment`/`Income` contra `BankMovement`): pendiente/opcional —
  `Bank`/`BankMovement` ya existen pero sin enganche a las líneas. Es una fase-2 acotada.

## 10. Importación de datos — HECHA (2026-07-15, solo cabeceras)

Cargadas las **2.602 facturas** de los dos Excels al tenant `alphatracker` con
`Downloads/cargar_facturas_v2.py` (script desechable, no código de aplicación):
**335 Emitida** (18,6 M€, 22 abonos `NA-*`) + **2.267 Recibida** (14,9 M€, 51 abonos),
17 re-listados idénticos omitidos (incl. el duplicado real `A26/44`), 2 rechazos de parseo,
36 clientes y 206 proveedores nuevos creados con `Tipo* = 'IMPORT'` (revisar/fusionar).
Los 8 códigos `FormaPago` almacenados son exactamente la gramática §5 (validados contra el
parser C# antes de persistir — un código inválido rompería el value-converter al leer).

**Decisión: NO se crearon `Ingresos`/`Pagos` desde los Excels.** Las 10.514 líneas legacy ya
cargadas solapan por importe (49-69%) pero no por fecha (2-12%) → crear líneas nuevas habría
duplicado caja. Vincular líneas existentes ↔ facturas es el paso pendiente (manual vía
`POST /invoices/link-line` desde la UI, o una pasada asistida por proyecto+importe).

## 11. Vinculación asistida — HECHA (2026-07-16)

Botón **Asistida** en la página Facturas: `GET /invoices/link-suggestions` calcula pares
inequívocos factura↔línea (contraparte + importe exacto, o los hitos de la FormaPago — p. ej.
30%+70% de `30PP70-60D`; para Emitida la contraparte se resuelve vía proyecto→cliente) y
`POST /invoices/link-suggestions/apply` los aplica en lote re-validando cada par (pares
desfasados se omiten, nunca abortan). Lo ambiguo (varias candidatas para el mismo hueco, o una
línea disputada por dos facturas) **no se propone** — precisión ante todo — y se resuelve a mano
con el diálogo 🔗. Motor puro testeable en `LinkSuggestionMatcher` (8 tests).
Rendimiento medido sobre alphatracker: **409 facturas resueltas de 2.602 (471 pares)** +
168 ambiguas marcadas; el resto no tiene match contraparte+importe (granularidad distinta) y
queda para vinculación manual.

## 12. Extracción por IA desde documento — HECHA (2026-07-16)

En **Nueva factura** hay un recuadro para arrastrar/subir el PDF o imagen de la factura:
`POST /invoices/extract` guarda el documento en el storage (MinIO, queda **adjunto** al registro
vía `Facturas.Documento` — migración `AddInvoiceDocument`) y se lo pasa a **Claude con visión**
(`claude-opus-4-8`, SDK oficial `Anthropic` 12.35.1, salida estructurada JSON garantizada por
schema). La propuesta (tipo Emitida/Recibida deducido contra el catálogo de Empresas, número,
fechas, base/IVA/total, forma de pago validada contra la gramática §5, contraparte cruzada contra
Clientes/Proveedores) **rellena el formulario y el usuario confirma** — nada se guarda sin revisar.
Al editar, botón "Ver documento adjunto" (URL pública del bucket).

Config: `CashflowAi__ApiKey` (env var de usuario, NUNCA en el repo; fallback `ANTHROPIC_API_KEY`);
`CashflowAi__Model` opcional. Sin clave → 503 claro y el formulario sigue funcionando.
E2E verificado con un PDF real: todos los campos extraídos correctos, documento adjunto y abrible.

**Digitalización posterior**: una factura creada sin documento puede recibirlo después —
`POST /invoices/{id}/document` (multipart) lo guarda y lo adjunta (y al reemplazar borra el objeto
anterior del storage, best-effort). En la UI: al editar una factura sin documento aparece
"Adjuntar documento digitalizado"; con documento, "Ver documento adjunto" + "Reemplazar".
Además, las filas de Ingresos/Pagos del detalle de proyecto muestran un icono 📄 (coloreado =
con factura vinculada, apagado = sin factura).

## 13. Proformas (1 proforma → N facturas) — HECHO (2026-07-31)

Nuevo agregado `Proforma`, **un nivel por encima de `Invoice`**, con la misma relación de diseño
que Invoice→Income/Payment: agregados independientes, referenciados por identidad
(`Invoice.ProformaId`, FK real intra-módulo `ON DELETE SET NULL`), sin navegación.

- **Modelo**: tabla `Proformas` (schema `cashflow`), espejo de `Facturas` — mismo check constraint
  asimétrico (`CK_Proformas_Tipo`), mismo índice único parcial (`Numero, TenantId WHERE
  Tipo='Emitida'`; Recibida solo lookup). Reutiliza `InvoiceType` y `PaymentTerms` tal cual.
  Estados propios sembrados (`StatusType.ProformaEmitida/ProformaRecibida`): BORRADOR, ENVIADA,
  ACEPTADA, RECHAZADA, FACTURADA PARCIAL, FACTURADA COMPLETA, CANCELADA. Migración `AddProformas`.
- **Slices** (`Features/v1/Proformas/`): Search/Create/GetById/Update/Delete +
  `GetProformaLines` (facturas vinculadas + facturado/pendiente **informativo**, nunca bloquea) +
  `GenerateInvoicesFromProforma` + `LinkInvoiceToProforma` + `AttachProformaDocument`/
  `GetProformaDocument` + `GetProformaPdf` (QuestPDF, sin QR — una proforma no es documento
  fiscal y NUNCA pasa por VeriFactu).
- **Numeración**: `Proforma.Number` texto libre (único asimétrico). Las facturas generadas se
  numeran `{Numero}-{i}` (borrador, renumerables); el handler valida colisiones y responde 409
  listándolas. La generación NO es idempotente a propósito (auto + manual se combinan).
  (Historial: la generación se retiró de la UI el 2026-08-03 y se RESTAURÓ el mismo día a
  petición del usuario, junto con la creación directa.)
- **Permisos**: resource `Proformas` (View/Create/Update/Delete/GenerateInvoices).
- **UI** (`/facturacion/proformas`): tabla con Total/Facturado/Pendiente (rojo si exceso), editor
  (mismo patrón que facturas, con adjunto), diálogo "Facturas" con TRES vías de asociación:
  (1) **"Crear factura"** — alta directa en borrador con los datos de la proforma (contraparte,
  empresa, sociedad, proyecto, FormaPago; base/IVA proporcionales al importe elegido, que por
  defecto es el pendiente; número `{Numero}-{n}` con primer sufijo libre; vinculada al crearse);
  (2) **"Generar facturas"** — una por plazo de la FormaPago, con confirmación de dos pasos si ya
  hay vinculadas; (3) manual desde el editor de facturas (selector "Proforma"). Más
  descarga de PDF. En el editor de facturas: selector "Proforma" (vinculación manual, filtrado
  por tipo); la vinculación viaja por `LinkInvoiceToProforma`, no por el update normal.

## 14. VERI*FACTU (AEAT, RD 1007/2023 + Orden HAC/1177/2024) — Fases 0-2 HECHAS (2026-07-31)

Emisión de facturas verificables para las **Emitidas** (las Recibidas quedan `NoAplica`). Por
`Company` (no por tenant): cadena de huellas, certificado y on/off viven en `VerifactuAjustes`
(1:1 con Empresas, FK real; certificado PFX + contraseña cifrados con DataProtection, key-ring
Redis). `Company.Nif` nuevo campo dedicado (no reutiliza `RegistroFiscal`). `Invoice.Number`
mapea 1:1 a `NumSerieFactura` (≤60 chars) — sin campo `Series`.

- **Huella encadenada** (`VerifactuChainService`, lógica pura): SHA-256 hex mayúsculas sobre la
  cadena `campo=valor&…` oficial — **pasa los vectores de prueba oficiales Caso 1 y 2** del doc
  de la AEAT (v0.1.2). Primer registro: `Huella=` vacía. QR: `ValidarQR` (prewww2/www2) con
  nif/numserie/fecha/importe URL-encoded, nivel M, "QR tributario:" + leyenda legal en el PDF
  (`InvoiceVerifactuPdfRenderer`, QuestPDF + QRCoder).
- **Emisión** (`POST /invoices/{id}/verifactu/issue`, permiso `Facturacion.IssueVerifactu`):
  advisory lock por empresa, registro `VerifactuRegistros` + avance de cadena en la misma
  transacción; **irreversible** — `Invoice.Update`/Delete lanzan 409 si el estado es
  PendienteEnvio/Enviada/Aceptada/AceptadaConErrores (Rechazada sigue editable). Rectificativas:
  trabajo futuro (tipos R1-R5, TipoRectificativa S/I, registro de anulación — ver doc AEAT).
- **Envío SOAP** (`AeatVerifactuClient`): `RegFactuSistemaFacturacion`, mTLS por empresa (PFX en
  memoria), endpoints oficiales pruebas/producción; ⚠️ el XML debe contrastarse end-to-end contra
  preproducción AEAT con certificado real antes de activar producción (plan §21).
- **Jobs**: `SubmitVerifactuRecordsJob` (batch por empresa ≤1000, programado +60 s tras emitir —
  control de flujo AEAT; `[AutomaticRetry(5)]`) y `VerifactuSweepJob` (recurrente horario,
  `cashflow:verifactu-sweep`) que reencola PendienteEnvio/ErrorTecnico por tenant/empresa.
- **API/UI**: `InvoiceDto.VerifactuStatus` + filtro `verifactuStatus` en búsqueda; página
  `/facturacion/verifactu` (ajustes por empresa, NIF, certificado, entorno, cadena); en facturas:
  punto de estado junto al número, banda de incidencias (Rechazada/ErrorTecnico), sección
  VeriFactu del editor (emitir con confirmación de dos pasos, PDF con QR, bloqueo de guardado).
  "VeriFactu" ≠ check interno "Verificada" — términos y badges separados a propósito.
- Migraciones: `AddVerifactu` (Nif, VerifactuEstado, VerifactuAjustes, VerifactuRegistros).

## 15. Factura presentable (PDF con identidad de empresa) — HECHO (2026-08-01)

- **Empresa** ampliada: `Direccion`, `CodigoPostal`, `Ciudad`, `Telefono`, `Email`, `Logo`
  (subida `POST /companies/{id}/logo`, imagen ≤1 MB, editable en `/admin/empresas`; el NIF y estos
  campos alimentan la cabecera del PDF).
- **Conceptos** (`FacturaConceptos`, hijo del agregado Invoice): descripción, cantidad, precio,
  importe derivado — detalle presentable del PDF; los importes fiscales siguen siendo
  base/IVA/total de la cabecera. Editables en el editor de facturas ("Conceptos (PDF)", con aviso
  si la suma no cuadra con la base imponible); bloqueados si la factura está registrada en
  VeriFactu (misma inmutabilidad).
- **PDF unificado** `GET /invoices/{id}/pdf` (endpoint "GetCashflowInvoicePdf" — "GetInvoicePdf"
  es de Billing): logo + bloques emisor/cliente (NIF y dirección) + tabla de conceptos + totales;
  si la factura está registrada en VeriFactu añade automáticamente el QR tributario + leyenda +
  huella. Sustituye a `/verifactu/pdf`. Botón "Descargar PDF" en el editor (todas las facturas).
- Migración: `AddInvoicePresentation`. Nota infra: NU1902/NU1903 pasan a warnings no bloqueantes
  (`WarningsNotAsErrors` en `src/Directory.Build.props`) + pin transitivo `MessagePack 2.5.301` —
  desbloquea `iniciar.bat`/CI tras la ola de advisories.

## 16. Pendiente

1. Vinculación manual del resto (~2.000 facturas sin match inequívoco) — diálogo 🔗 por factura.
2. Revisar los 242 clientes/proveedores `IMPORT` (fusionar variantes de nombre con los existentes).
3. (Opcional) conciliación bancaria (§9).

> Tip operativo: `dotnet ef migrations add` funciona con el stack levantado usando
> `--startup-project src/Host/FSH.Starter.DbMigrator` (lleva el paquete Design; el Api corriendo
> bloquea sus binarios). Recompilar el DbMigrator DESPUÉS de generar la migración y antes de `apply`.
