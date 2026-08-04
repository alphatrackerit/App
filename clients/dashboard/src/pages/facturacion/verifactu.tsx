import { useEffect, useMemo, useRef, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { BadgeCheck, FileKey2, Link2, Loader2, ShieldCheck, UploadCloud } from "lucide-react";
import { toast } from "sonner";
import {
  getVerifactuSettings,
  setVerifactuCertificate,
  upsertVerifactuSettings,
  type VerifactuEnvironment,
} from "@/api/verifactu";
import { companyCatalog, type CompanyRow } from "@/api/administration";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Combobox, EntityStatusBadge, Field, PageHero } from "@/components/list";
import { describe } from "@/lib/list-helpers";

export function VerifactuSettingsPage() {
  const queryClient = useQueryClient();

  const companiesQ = useQuery({
    queryKey: ["administration", "companies", "rich"],
    queryFn: () => companyCatalog.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const companies = useMemo(() => companiesQ.data?.items ?? [], [companiesQ.data]);

  const [companyId, setCompanyId] = useState<string | null>(null);
  // Autoselección de la primera empresa al cargar.
  useEffect(() => {
    if (!companyId && companies.length > 0) setCompanyId(companies[0].id);
  }, [companies, companyId]);
  const company: CompanyRow | undefined = companies.find((c) => c.id === companyId);

  const settingsQ = useQuery({
    queryKey: ["verifactu", "settings", companyId],
    queryFn: () => getVerifactuSettings(companyId!),
    enabled: !!companyId,
  });
  const settings = settingsQ.data;

  // ── Formulario de ajustes (hand-rolled, sin RHF) ──
  const [environment, setEnvironment] = useState<VerifactuEnvironment>("Pruebas");
  const [enabled, setEnabled] = useState(false);
  const [softwareName, setSoftwareName] = useState("");
  const [softwareVersion, setSoftwareVersion] = useState("");
  const [installationNumber, setInstallationNumber] = useState("");
  const [nif, setNif] = useState("");

  useEffect(() => {
    if (settings) {
      setEnvironment(settings.environment);
      setEnabled(settings.enabled);
      setSoftwareName(settings.softwareName ?? "");
      setSoftwareVersion(settings.softwareVersion ?? "");
      setInstallationNumber(settings.installationNumber ?? "");
    }
  }, [settings]);
  useEffect(() => setNif(company?.nif ?? ""), [company]);

  const saveSettings = useMutation({
    mutationFn: async (vars: {
      companyId: string;
      environment: VerifactuEnvironment;
      enabled: boolean;
      softwareName: string | null;
      softwareVersion: string | null;
      installationNumber: string | null;
      nif: string | null;
      company: CompanyRow;
    }) => {
      // El NIF vive en la empresa (registro AEAT por NIF) — round-trip completo para no pisar campos.
      if ((vars.nif ?? "") !== (vars.company.nif ?? "")) {
        await companyCatalog.update(vars.companyId, {
          name: vars.company.name,
          code: vars.company.code,
          legalName: vars.company.legalName,
          taxRegistration: vars.company.taxRegistration,
          showInProjects: vars.company.showInProjects,
          nif: vars.nif,
          address: vars.company.address,
          postalCode: vars.company.postalCode,
          city: vars.company.city,
          phone: vars.company.phone,
          email: vars.company.email,
        });
      }
      return upsertVerifactuSettings({
        companyId: vars.companyId,
        environment: vars.environment,
        enabled: vars.enabled,
        softwareName: vars.softwareName,
        softwareVersion: vars.softwareVersion,
        installationNumber: vars.installationNumber,
      });
    },
    onSuccess: () => {
      toast.success("Ajustes VeriFactu guardados");
      queryClient.invalidateQueries({ queryKey: ["verifactu"] });
      queryClient.invalidateQueries({ queryKey: ["administration", "companies"] });
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  // ── Certificado ──
  const certInputRef = useRef<HTMLInputElement>(null);
  const [certFile, setCertFile] = useState<File | null>(null);
  const [certPassword, setCertPassword] = useState("");
  const uploadCert = useMutation({
    mutationFn: (vars: { companyId: string; file: File; password: string }) =>
      setVerifactuCertificate(vars.companyId, vars.file, vars.password),
    onSuccess: () => {
      toast.success("Certificado guardado (cifrado)");
      setCertFile(null);
      setCertPassword("");
      queryClient.invalidateQueries({ queryKey: ["verifactu"] });
    },
    onError: (err) => toast.error("No se pudo guardar el certificado", { description: describe(err) }),
  });

  const hasError = companiesQ.isError || settingsQ.isError;

  return (
    <div className="space-y-5">
      <PageHero
        eyebrow="Facturación"
        title="VeriFactu"
        subtitle="Registro de facturación verificable ante la AEAT (RD 1007/2023) — configuración por empresa."
      />

      <div className="flex flex-wrap items-center gap-3">
        <div className="w-72">
          <Combobox
            id="vf-company"
            label="Empresa"
            variant="field"
            value={companyId}
            onChange={(v) => setCompanyId(v)}
            options={companies.map((c) => ({ value: c.id, label: c.name, hint: c.nif ?? undefined }))}
            searchable
            placeholder={companiesQ.isLoading ? "Cargando…" : "Selecciona empresa"}
          />
        </div>
        {settings && (
          <>
            <EntityStatusBadge tone={settings.enabled ? "success" : "default"} withDot>
              {settings.enabled ? "Envío activo" : "Envío desactivado"}
            </EntityStatusBadge>
            <EntityStatusBadge tone={settings.hasCertificate ? "success" : "warning"} withDot>
              {settings.hasCertificate ? "Certificado guardado" : "Sin certificado"}
            </EntityStatusBadge>
            <EntityStatusBadge tone={settings.environment === "Produccion" ? "danger" : "info"}>
              {settings.environment === "Produccion" ? "Producción" : "Pruebas"}
            </EntityStatusBadge>
          </>
        )}
      </div>

      {hasError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(companiesQ.error ?? settingsQ.error)}
        </div>
      ) : !companyId || settingsQ.isLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-5 py-16 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {companiesQ.isLoading ? "Cargando empresas…" : "Cargando ajustes…"}
        </div>
      ) : settings ? (
        <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
          {/* ── Ajustes ── */}
          <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs">
            <div className="mb-3 flex items-center gap-2 px-1">
              <ShieldCheck className="size-4 text-[var(--color-muted-foreground)]" />
              <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Ajustes de la empresa</h2>
            </div>
            <form
              className="space-y-4"
              onSubmit={(e) => {
                e.preventDefault();
                if (!company) return;
                saveSettings.mutate({
                  companyId: company.id,
                  environment,
                  enabled,
                  softwareName: softwareName.trim() || null,
                  softwareVersion: softwareVersion.trim() || null,
                  installationNumber: installationNumber.trim() || null,
                  nif: nif.trim() || null,
                  company,
                });
              }}
            >
              <Field id="vf-nif" label="NIF de la empresa" hint="Identifica el registro ante la AEAT; sin NIF no se puede emitir." required>
                <Input id="vf-nif" value={nif} onChange={(e) => setNif(e.target.value)} placeholder="B12345678" maxLength={20} />
              </Field>

              <div className="grid grid-cols-2 gap-4">
                <Field id="vf-env" label="Entorno" hint="Empieza en Pruebas; Producción solo tras verificar.">
                  <Combobox
                    id="vf-env"
                    label="Entorno"
                    variant="field"
                    value={environment}
                    onChange={(v) => setEnvironment((v as VerifactuEnvironment) ?? "Pruebas")}
                    options={[
                      { value: "Pruebas", label: "Pruebas (preproducción AEAT)" },
                      { value: "Produccion", label: "Producción" },
                    ]}
                    placeholder="Entorno"
                  />
                </Field>
                <Field id="vf-install" label="Nº instalación">
                  <Input id="vf-install" value={installationNumber} onChange={(e) => setInstallationNumber(e.target.value)} placeholder="1" maxLength={64} />
                </Field>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <Field id="vf-sw" label="Nombre del software (SIF)">
                  <Input id="vf-sw" value={softwareName} onChange={(e) => setSoftwareName(e.target.value)} placeholder="FSH Starter Cashflow" maxLength={128} />
                </Field>
                <Field id="vf-swv" label="Versión">
                  <Input id="vf-swv" value={softwareVersion} onChange={(e) => setSoftwareVersion(e.target.value)} placeholder="1.0" maxLength={64} />
                </Field>
              </div>

              <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
                <div>
                  <p className="text-[13px] text-[var(--color-foreground)]">Envío a la AEAT activo</p>
                  <p className="text-[11.5px] text-[var(--color-muted-foreground)]">
                    {settings.hasCertificate
                      ? "Las facturas emitidas con VeriFactu se remiten automáticamente."
                      : "Requiere subir antes el certificado de la empresa."}
                  </p>
                </div>
                <Switch
                  checked={enabled}
                  onCheckedChange={setEnabled}
                  disabled={!settings.hasCertificate}
                  aria-label="Envío a la AEAT activo"
                />
              </div>

              <div className="flex justify-end">
                <Button type="submit" disabled={saveSettings.isPending}>
                  {saveSettings.isPending ? "Guardando…" : "Guardar ajustes"}
                </Button>
              </div>
            </form>
          </section>

          <div className="space-y-5">
            {/* ── Certificado ── */}
            <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs">
              <div className="mb-3 flex items-center gap-2 px-1">
                <FileKey2 className="size-4 text-[var(--color-muted-foreground)]" />
                <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Certificado de la empresa</h2>
              </div>
              <p className="mb-3 px-1 text-[12.5px] text-[var(--color-muted-foreground)]">
                Certificado de representante (PFX/P12). Se valida con la contraseña y se guarda cifrado —
                nunca sale del servidor. Sirve para pruebas y producción.
              </p>
              <div className="space-y-3">
                <button
                  type="button"
                  onClick={() => certInputRef.current?.click()}
                  className="flex w-full cursor-pointer items-center gap-3 rounded-lg border border-dashed border-[var(--color-border)] px-3 py-3 text-left transition-colors hover:border-[var(--color-primary)]"
                >
                  <UploadCloud className="size-5 shrink-0 text-[var(--color-primary)]" />
                  <span className="min-w-0">
                    <span className="block truncate text-[13px] font-medium text-[var(--color-foreground)]">
                      {certFile ? certFile.name : settings.hasCertificate ? "Reemplazar certificado" : "Seleccionar fichero PFX/P12"}
                    </span>
                    <span className="block text-[11px] text-[var(--color-muted-foreground)]">
                      {settings.hasCertificate ? "Ya hay un certificado guardado para esta empresa." : "Aún no hay certificado."}
                    </span>
                  </span>
                </button>
                <input
                  ref={certInputRef}
                  type="file"
                  accept=".pfx,.p12,application/x-pkcs12"
                  className="hidden"
                  onChange={(e) => {
                    setCertFile(e.target.files?.[0] ?? null);
                    e.target.value = "";
                  }}
                />
                <Field id="vf-pass" label="Contraseña del certificado">
                  <Input
                    id="vf-pass"
                    type="password"
                    value={certPassword}
                    onChange={(e) => setCertPassword(e.target.value)}
                    placeholder="••••••••"
                    maxLength={256}
                    autoComplete="off"
                  />
                </Field>
                <div className="flex justify-end">
                  <Button
                    disabled={!certFile || !certPassword || uploadCert.isPending || !company}
                    onClick={() => company && certFile && uploadCert.mutate({ companyId: company.id, file: certFile, password: certPassword })}
                  >
                    {uploadCert.isPending ? (
                      <>
                        <Loader2 className="mr-1.5 size-4 animate-spin" />
                        Subiendo…
                      </>
                    ) : (
                      "Guardar certificado"
                    )}
                  </Button>
                </div>
              </div>
            </section>

            {/* ── Estado de la cadena ── */}
            <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs">
              <div className="mb-3 flex items-center gap-2 px-1">
                <Link2 className="size-4 text-[var(--color-muted-foreground)]" />
                <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Cadena de huellas</h2>
              </div>
              {settings.lastChainHash ? (
                <div className="space-y-1.5 px-1">
                  <p className="flex items-center gap-1.5 text-[12.5px] text-[var(--color-muted-foreground)]">
                    <BadgeCheck className="size-3.5 text-[var(--color-success)]" />
                    Última huella{settings.lastChainAtUtc ? ` · ${settings.lastChainAtUtc.slice(0, 19).replace("T", " ")}` : ""}
                  </p>
                  <code className="block break-all rounded-md bg-[var(--color-muted)] px-2.5 py-1.5 font-mono text-[11px] text-[var(--color-foreground)]">
                    {settings.lastChainHash}
                  </code>
                </div>
              ) : (
                <p className="px-1 py-4 text-center text-[13px] text-[var(--color-muted-foreground)]">
                  Sin registros todavía — la cadena empieza con la primera factura emitida con VeriFactu.
                </p>
              )}
            </section>
          </div>
        </div>
      ) : null}
    </div>
  );
}
