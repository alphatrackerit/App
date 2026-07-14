# Tutorial: cómo dar permisos a un usuario (a prueba de tontos)

> Guía paso a paso para crear usuarios con acceso limitado (por ejemplo, "solo el módulo
> Avícola") usando únicamente el dashboard. No hace falta tocar código ni la base de datos.

---

## 1. Los tres conceptos (en 3 líneas)

1. **Permiso** — la unidad mínima de acceso. Tiene la forma `Permissions.{Módulo}.{Recurso}.{Acción}`.
   Ejemplo: `Permissions.Avicola.Lotes.View` = "puede *ver* los lotes del módulo Avícola".
2. **Rol** — una bolsa de permisos con nombre (ej.: "Avicola Operario"). Los permisos nunca se dan
   directamente a una persona: siempre se meten en un rol.
3. **Usuario** — una persona con email y contraseña. Se le asignan uno o más **roles**, y hereda
   todos los permisos de esos roles.

> ⚠️ **Nunca uses los roles `Admin` ni `Basic` para acceso limitado.** Son roles del sistema:
> `Admin` recibe *todos* los permisos de *todos* los módulos y `Basic` recibe los permisos básicos
> de *todos* los módulos, y el sistema se los vuelve a sincronizar solo. Para dar acceso a un solo
> módulo, crea siempre un **rol personalizado**.

---

## 2. Crear un rol nuevo (dashboard)

1. Entra al dashboard (`http://localhost:5174`) con un usuario administrador del tenant
   (en desarrollo: `admin@acme.com` / `Password123!`).
2. En el menú lateral abre la sección **Identity → Roles** (ruta `/identity/roles`).
3. Pulsa el botón **New role** (arriba a la derecha).
4. En el diálogo **"Create a role"** escribe:
   - **Name**: el nombre del rol, ej. `Avicola Operario`.
     *(Los nombres `Admin` y `Basic` están reservados — el sistema los rechaza.)*
   - **Description**: para qué sirve, ej. `Captura diaria del módulo Avícola`.
5. Guarda. El rol se crea **sin ningún permiso** — el siguiente paso es dárselos.

## 3. Darle permisos al rol

1. En la lista de roles, haz clic sobre el rol recién creado para abrir su **detalle**.
2. Verás el **editor de permisos**: todos los permisos del sistema agrupados por recurso
   (Avicola.Lotes, Avicola.Galpones, Catalog.Products, etc.), con un buscador y filtros.
   - Escribe `avicola` en el buscador para ver solo los permisos del módulo Avícola.
   - Cada grupo tiene un interruptor para marcar/desmarcar el grupo completo.
3. Marca los permisos que quieras (ver los recetarios de la sección 5).
4. Pulsa **Save changes**. Los cambios aplican de inmediato a todos los usuarios que tengan el rol.

## 4. Crear el usuario y asignarle el rol

1. Menú lateral → **Identity → Users** (ruta `/identity/users`).
2. Pulsa **Register user** y completa el diálogo **"Register a member"**
   (nombre, apellido, email, nombre de usuario y contraseña).
3. **Confirma su email**: abre el detalle del usuario recién creado; junto al estado del email
   pulsa **Confirm email** (requiere el permiso `Permissions.Users.ConfirmEmail`).
   *Sin este paso el usuario no puede iniciar sesión* ("Authentication failed").
4. En el mismo detalle, baja a la tarjeta **Role assignment**:
   - **Activa** el interruptor del rol que quieras darle (ej. `Avicola Operario`).
   - **Desactiva** el interruptor de `Basic` si aparece activado (el registro lo asigna
     automáticamente y trae permisos básicos de *todos* los módulos).
   - Pulsa **Save changes**.
5. Listo. El usuario ya puede iniciar sesión y solo verá en el menú lo que sus permisos permiten.

> 🔁 Si el usuario ya tenía la sesión abierta cuando cambiaste sus roles/permisos, que cierre
> sesión y vuelva a entrar para refrescar el menú.

---

## 5. Recetarios listos para Avícola

### Rol "Avicola Admin" — gestión completa del módulo (46 permisos)

Marca **todos** los grupos que empiezan con `Avicola.` En detalle:

| Grupo | Acciones a marcar |
|---|---|
| Avicola.Galpones | View, Create, Update, Delete |
| Avicola.Lotes | View, Create, Update, Delete, Cerrar |
| Avicola.Mortalidad | View, Create, Update, Delete |
| Avicola.Alimentacion | View, Create, Update, Delete |
| Avicola.Pesos | View, Create, Update, Delete |
| Avicola.Sanidad | View, Create, Update, Delete |
| Avicola.Despachos | View, Create, Update, Delete |
| Avicola.Documentos | View, Create, Delete |
| Avicola.Pedidos | View, Create, Update, Delete |
| Avicola.Movimientos | View, Create, Update, Delete |
| Avicola.Contabilidad | View |
| Avicola.Preparaciones | View, Create, Update, Delete, Completar |

### Rol "Avicola Operario" — captura diaria, sin gestión (17 permisos)

Puede **ver** la operación y **registrar** los datos del día. No puede borrar nada, ni tocar
pedidos, movimientos o contabilidad, ni crear/cerrar lotes.

| Grupo | Acciones a marcar |
|---|---|
| Avicola.Galpones | View |
| Avicola.Lotes | View |
| Avicola.Mortalidad | View, Create |
| Avicola.Alimentacion | View, Create |
| Avicola.Pesos | View, Create |
| Avicola.Sanidad | View, Create |
| Avicola.Despachos | View |
| Avicola.Documentos | View, Create |
| Avicola.Preparaciones | View, Create, Update, Completar |

**Qué significa cada acción**: `View` = ver/listar · `Create` = crear registros nuevos ·
`Update` = editar existentes · `Delete` = eliminar · `Cerrar` (lotes) = cerrar el ciclo del lote ·
`Completar` (preparaciones) = dar por terminada la preparación de la nave.

---

## 6. Alternativa por API (para scripts / automatización)

<details>
<summary>Ver los comandos curl equivalentes</summary>

> ⚠️ En PowerShell/Windows, los JSON con acentos se corrompen al enviarlos con curl
> (se codifican Latin-1). Guarda el body en un archivo UTF-8 y envíalo con
> `--data-binary @archivo.json`, o escribe los textos sin acentos.

```bash
API=https://localhost:7030
TENANT=acme

# 1) Token de un admin del tenant
curl -sk -X POST "$API/api/v1/identity/token/issue" \
  -H "Content-Type: application/json" -H "tenant: $TENANT" \
  -d '{"email":"admin@acme.com","password":"Password123!"}'
# → guarda el accessToken en $TOKEN

# 2) Crear el rol (id vacio = crear)
curl -sk -X POST "$API/api/v1/identity/roles" \
  -H "Content-Type: application/json" -H "tenant: $TENANT" -H "Authorization: Bearer $TOKEN" \
  -d '{"id":"","name":"Avicola Operario","description":"Captura diaria Avicola"}'
# → devuelve {"id":"<ROLE_ID>", ...}

# 3) Asignarle permisos (REEMPLAZA el set completo del rol)
curl -sk -X PUT "$API/api/v1/identity/<ROLE_ID>/permissions" \
  -H "Content-Type: application/json" -H "tenant: $TENANT" -H "Authorization: Bearer $TOKEN" \
  -d '{"roleId":"<ROLE_ID>","permissions":["Permissions.Avicola.Lotes.View","Permissions.Avicola.Mortalidad.Create"]}'

# 4) Registrar el usuario
curl -sk -X POST "$API/api/v1/identity/register" \
  -H "Content-Type: application/json" -H "tenant: $TENANT" -H "Authorization: Bearer $TOKEN" \
  -d '{"firstName":"Juan","lastName":"Perez","email":"juan@acme.com","userName":"juan.perez","password":"Password123!","confirmPassword":"Password123!"}'
# → devuelve {"userId":"<USER_ID>"}

# 5) Confirmar su email (si no, no puede iniciar sesion)
curl -sk -X POST "$API/api/v1/identity/users/<USER_ID>/confirm-email" \
  -H "tenant: $TENANT" -H "Authorization: Bearer $TOKEN"

# 6) Asignar el rol (y quitar el Basic que el registro agrega solo)
curl -sk -X POST "$API/api/v1/identity/users/<USER_ID>/roles" \
  -H "Content-Type: application/json" -H "tenant: $TENANT" -H "Authorization: Bearer $TOKEN" \
  -d '{"userId":"<USER_ID>","userRoles":[{"roleId":"<BASIC_ROLE_ID>","roleName":"Basic","enabled":false},{"roleId":"<ROLE_ID>","roleName":"Avicola Operario","enabled":true}]}'
```

Los IDs de roles del tenant se consultan con `GET /api/v1/identity/roles`.

</details>

---

## 7. Problemas frecuentes

| Síntoma | Causa y solución |
|---|---|
| El usuario no puede iniciar sesión ("Authentication failed") | Su email no está confirmado. Detalle del usuario → **Confirm email**. |
| No ve la sección Avícola en el menú | Le falta el permiso `View` correspondiente (el menú oculta lo que no puede ver), o no ha vuelto a iniciar sesión tras el cambio. |
| Ve secciones de otros módulos (Proyectos, Catalog…) | El menú oculta cada ítem si falta su permiso `View` (definido en `nav-data.ts`). Si aún las ve, que cierre sesión y vuelva a entrar. Excepciones sin permiso: Overview, Chat, My Files, Live activity, Health y Settings. |
| Recibe 403 al guardar/crear | Su rol tiene `View` pero le falta `Create`/`Update` en ese recurso. Edita los permisos del rol. |
| "Cannot create a role using a system role's name" | `Admin` y `Basic` están reservados. Usa otro nombre. |
| Cambió permisos y "no pasa nada" | Los permisos aplican al instante en la API; el menú del usuario se refresca al volver a iniciar sesión. |
| El usuario nuevo tiene más acceso del esperado | El registro le asignó el rol `Basic` automáticamente. Desactívalo en **Role assignment**. |

---

*Referencias técnicas: los permisos de Avícola se definen en
`src/Modules/Avicola/Modules.Avicola.Contracts/Authorization/AvicolaPermissions.cs`;
el menú del dashboard se protege en `clients/dashboard/src/components/layout/nav-data.ts`.*
