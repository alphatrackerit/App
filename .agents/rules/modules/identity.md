# Module: Identity

Auth (JWT + ASP.NET Identity), users, roles, permissions, sessions, impersonation, 2FA.

## Service shape

`IUserService` is a **facade** that delegates to focused single-responsibility services — change behavior in the specific service, not the facade:

| Interface | Concern |
|---|---|
| `IUserRegistrationService` | register, external-principal create, email/phone confirm |
| `IUserProfileService` | get/list/count, update profile, image, existence checks |
| `IUserStatusService` | activate/deactivate (`DeleteAsync` == deactivate), audited toggles |
| `IUserRoleService` | role assignment, admin-role guards |
| `IUserPasswordService` | forgot/reset/change password, history + expiry |
| `IUserPermissionService` | effective permissions, cache invalidation |

`ChangePassword`/`Update`/`Delete` etc. flow facade → service → EF/UserManager. `CancellationToken` is `= default` on these interfaces and propagated into EF sinks (note: `UserManager`/`RoleManager` have no CT overloads, so private helpers that only call them don't take one).

## Permission gating footgun

`RequiredPermissionAttribute` implements `FSH.Framework.Shared.Identity.Authorization.IRequiredPermissionMetadata`. **Never let a second/duplicate `IRequiredPermissionMetadata` appear** — it silently disables **all** `.RequirePermission()` gates across the app. Permission constants live in `Shared/Identity/*Permissions.cs`.

## Hosted services (background)

- `RolePermissionSyncHostedService` — best-effort sync of the permission catalog; loops, catches `Exception` *with* an `OperationCanceledException` filter, logs and continues.
- `SessionCleanupHostedService` — hourly expired-session purge; OCE handled by a preceding catch.

These are the model for background loops: stay alive, log with context, never swallow cancellation. See `api-conventions.md`.

## Tokens / sessions

Login `POST /api/v1/identity/token/issue` (header `X-FSH-App` enforces the operator/tenant app boundary). Refresh `POST /api/v1/identity/token/refresh` cross-checks subject. Session rows are written best-effort during login — failures log a warning and login still succeeds. Admin can't demote/deactivate the last admin or the root-tenant seed admin (guards in `UserRoleService`/`UserStatusService`).

Both flows converge on **`ITokenIssuanceService.IssueAndPersistAsync(subject, claims, email)`** — the shared "authenticated user → tokens" path (issue + refresh-token persist + session + token-issued audit + `TokenGeneratedIntegrationEvent`). Handlers own only credential validation and the login-succeeded/failed audit. Don't inline issuance in a new flow — reuse this.

## Microsoft sign-in (Entra ID) — dashboard only

`POST /api/v1/identity/token/microsoft` exchanges a Microsoft **ID token** (the dashboard obtains it via MSAL `loginPopup`) for an FSH token pair. **No `tenant` header** — the tenant is **derived from the verified email domain** via `ITenantDomainResolver` (Multitenancy.Contracts, backed by the global `TenantEmailDomain` catalog table; seeded `acme.com`/`globex.com` for demo tenants). **Link-only**: `IIdentityService.ValidateExternalLoginAsync` resolves an existing active user by email and links the identity (`FshUser.ObjectId` + `AspNetUserLogins`); it never auto-provisions. Root-tenant domains are rejected (operator-only).

The ID token is validated offline against Entra's JWKS by `IMicrosoftTokenValidator` (singleton, caches OIDC metadata). Config: `MicrosoftEntraOptions` (`Enabled`/`Authority`/`ClientId`), **off by default**. The handler resolves the tenant, then runs its tenant-scoped work in a **child scope** with `IMultiTenantContextSetter` set first (same pattern as `TenantService`/`DemoSeeder`) — constructor-injecting the Identity DbContext would capture the unresolved request context. Frontend: `clients/dashboard` only (`auth/msal.ts` wrapper, `loginWithMicrosoft` in auth-context, button gated on `env.msalClientId`).

## Tests

`Identity.Tests` is the largest unit suite. When asserting a forwarded `CancellationToken`, assert the specific token (see `testing.md`).
