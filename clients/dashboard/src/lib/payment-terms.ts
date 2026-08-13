/**
 * Espejo en cliente de la gramática de `PaymentTerms` del backend
 * (`Modules.Cashflow/Domain/PaymentTerms.cs`). El servidor rechaza con 400 un
 * código desconocido, así que los formularios validan antes de enviar y el
 * usuario ve el motivo en el propio campo.
 *
 * Formas admitidas (los porcentajes deben sumar 100):
 *   60D            → 100 % a 60 días de la fecha de factura
 *   100PP          → 100 % por adelantado
 *   30PP70-60D     → 30 % adelantado + 70 % a 60 días
 *   40PP60PP       → 40 % + 60 % adelantados
 *   DOMICILIADO    → 100 % domiciliado al vencimiento
 */

const SINGLE_OFFSET = /^(\d+)D$/;
const SINGLE_PREPAID = /^(\d+)PP$/;
const PREPAID_THEN_OFFSET = /^(\d+)PP(\d+)-(\d+)D$/;
const PREPAID_THEN_PREPAID = /^(\d+)PP(\d+)PP$/;

export const PAYMENT_TERMS_HELP = "Códigos válidos: 60D, 100PP, 30PP70-60D, 40PP60PP o DOMICILIADO.";

/** `true` si el backend aceptaría el código. Una cadena vacía NO es válida
 *  (el campo es opcional: comprueba el vacío antes de llamar). */
export function isValidPaymentTerms(raw: string): boolean {
  const code = raw.trim().toUpperCase();
  if (!code) return false;
  if (code === "DOMICILIADO") return true;

  const twoMilestones = PREPAID_THEN_OFFSET.exec(code) ?? PREPAID_THEN_PREPAID.exec(code);
  if (twoMilestones) return Number(twoMilestones[1]) + Number(twoMilestones[2]) === 100;

  const prepaid = SINGLE_PREPAID.exec(code);
  if (prepaid) return Number(prepaid[1]) === 100;

  return SINGLE_OFFSET.test(code);
}

// ─── Port de PaymentTerms.GenerateMilestones (backend) ───

export type PaymentMilestone = {
  percentage: number;
  /** "Prepaid" = anticipo (sin fecha); "InvoiceDate" = fecha factura + offsetDays. */
  trigger: "Prepaid" | "InvoiceDate";
  offsetDays: number;
};

/** Hito materializado: importe redondeado a céntimos (el último absorbe el resto). */
export type GeneratedMilestone = {
  amount: number;
  /** ISO yyyy-mm-dd, o null para anticipos o sin fecha de factura. */
  dueDate: string | null;
  percentage: number;
};

function parseMilestones(code: string): PaymentMilestone[] | null {
  if (code === "DOMICILIADO") return [{ percentage: 100, trigger: "InvoiceDate", offsetDays: 0 }];

  const ppOff = PREPAID_THEN_OFFSET.exec(code);
  if (ppOff) {
    return [
      { percentage: Number(ppOff[1]), trigger: "Prepaid", offsetDays: 0 },
      { percentage: Number(ppOff[2]), trigger: "InvoiceDate", offsetDays: Number(ppOff[3]) },
    ];
  }
  const ppPp = PREPAID_THEN_PREPAID.exec(code);
  if (ppPp) {
    return [
      { percentage: Number(ppPp[1]), trigger: "Prepaid", offsetDays: 0 },
      { percentage: Number(ppPp[2]), trigger: "Prepaid", offsetDays: 0 },
    ];
  }
  const pp = SINGLE_PREPAID.exec(code);
  if (pp) return [{ percentage: Number(pp[1]), trigger: "Prepaid", offsetDays: 0 }];
  const off = SINGLE_OFFSET.exec(code);
  if (off) return [{ percentage: 100, trigger: "InvoiceDate", offsetDays: Number(off[1]) }];
  return null;
}

const round2 = (n: number) => Math.round(n * 100) / 100;

function addDays(isoDate: string, days: number): string {
  const d = new Date(`${isoDate.slice(0, 10)}T00:00:00Z`);
  d.setUTCDate(d.getUTCDate() + days);
  return d.toISOString().slice(0, 10);
}

/**
 * Materializa los hitos de una forma de pago para un total y una fecha de factura,
 * replicando el redondeo del backend (`PaymentTerms.GenerateMilestones`): céntimos,
 * y el último hito absorbe el resto para que las partes sumen exactamente el total.
 * Devuelve null si el código no es válido.
 */
export function generatePaymentMilestones(
  rawCode: string,
  total: number,
  invoiceDate: string | null,
): GeneratedMilestone[] | null {
  const code = rawCode.trim().toUpperCase();
  if (!isValidPaymentTerms(code)) return null;
  const milestones = parseMilestones(code);
  if (!milestones) return null;

  let allocated = 0;
  return milestones.map((m, i) => {
    const isLast = i === milestones.length - 1;
    const amount = isLast ? round2(total - allocated) : round2((total * m.percentage) / 100);
    allocated = round2(allocated + amount);
    const dueDate = m.trigger === "InvoiceDate" && invoiceDate ? addDays(invoiceDate, m.offsetDays) : null;
    return { amount, dueDate, percentage: m.percentage };
  });
}
