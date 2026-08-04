import { apiFetch } from "@/lib/api-client";

// ───────────────────────────────────────────────────────────────────────
//  VERI*FACTU (AEAT) — base /api/v1/cashflow/verifactu + acciones de factura
// ───────────────────────────────────────────────────────────────────────

export type VerifactuEnvironment = "Pruebas" | "Produccion";

/** Estado VERI*FACTU de una factura. OJO: no confundir con el check interno "Verificada". */
export type VerifactuStatus =
  | "NoAplica"
  | "PendienteEnvio"
  | "Enviada"
  | "Aceptada"
  | "AceptadaConErrores"
  | "Rechazada"
  | "ErrorTecnico";

export type VerifactuSettingsDto = {
  companyId: string;
  environment: VerifactuEnvironment;
  enabled: boolean;
  softwareName: string | null;
  softwareVersion: string | null;
  installationNumber: string | null;
  hasCertificate: boolean;
  lastChainHash: string | null;
  lastChainAtUtc: string | null;
};

export type VerifactuSettingsInput = {
  companyId: string;
  environment: VerifactuEnvironment;
  enabled: boolean;
  softwareName?: string | null;
  softwareVersion?: string | null;
  installationNumber?: string | null;
};

export type VerifactuRecordDto = {
  id: string;
  invoiceId: string;
  previousHash: string | null;
  hash: string;
  generatedAt: string;
  qrPayload: string;
  status: VerifactuStatus;
  aeatResponseCode: string | null;
  retryCount: number;
};

export function getVerifactuSettings(companyId: string): Promise<VerifactuSettingsDto> {
  return apiFetch<VerifactuSettingsDto>(`/api/v1/cashflow/verifactu/settings/${encodeURIComponent(companyId)}`);
}

export function upsertVerifactuSettings(input: VerifactuSettingsInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/verifactu/settings", {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

// Sube el certificado PFX/P12 + contraseña; el backend lo valida y lo guarda cifrado.
export async function setVerifactuCertificate(companyId: string, file: File, password: string): Promise<void> {
  const form = new FormData();
  form.append("file", file);
  form.append("password", password);
  await apiFetch<void>(`/api/v1/cashflow/verifactu/settings/${encodeURIComponent(companyId)}/certificate`, {
    method: "POST",
    body: form,
  });
}

/** IRREVERSIBLE: genera el registro encadenado; la factura deja de ser editable/borrable. */
export function issueVerifactu(invoiceId: string): Promise<VerifactuRecordDto> {
  return apiFetch<VerifactuRecordDto>(
    `/api/v1/cashflow/invoices/${encodeURIComponent(invoiceId)}/verifactu/issue`,
    { method: "POST" },
  );
}

// La descarga del PDF (con o sin QR VeriFactu) vive en api/facturas.ts → downloadFacturaPdf.
