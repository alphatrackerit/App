import { useEffect, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, BadgeCheck, CheckCircle2, Plus, StickyNote, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  confirmIncome,
  confirmPayment,
  createIncome,
  createNote,
  createPayment,
  deleteIncome,
  deleteNote,
  deletePayment,
  getProject,
  searchIncomes,
  searchNotes,
  searchPayments,
  validateIncome,
  validatePayment,
  type PagedResponse,
} from "@/api/projects";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogBody,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { EntityStatusBadge, Field, PageHero } from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

function money(n: number | null | undefined): string {
  if (n === null || n === undefined) return "—";
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(n);
}

function toNum(s: string): number | null {
  const t = s.trim();
  if (t === "") return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
}

function dateToIso(s: string): string | null {
  const t = s.trim();
  if (t === "") return null;
  const d = new Date(t);
  return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

export function ProjectDetailPage() {
  const { projectId = "" } = useParams();

  const query = useQuery({
    queryKey: ["projects", "detail", projectId],
    queryFn: () => getProject(projectId),
    enabled: projectId.length > 0,
  });

  const p = query.data;

  return (
    <div className="space-y-6">
      <Link
        to="/projects"
        className="inline-flex items-center gap-1.5 text-[12.5px] font-medium text-[var(--color-muted-foreground)] transition-colors hover:text-[var(--color-foreground)]"
      >
        <ArrowLeft className="size-3.5" />
        Proyectos
      </Link>

      <PageHero
        eyebrow="Proyecto"
        title={p?.name ?? (query.isLoading ? "Cargando…" : "Proyecto")}
        subtitle={
          p ? (
            <span className="flex flex-wrap gap-x-6 gap-y-1 tabular-nums">
              <span>Venta: <strong className="text-[var(--color-foreground)]">{money(p.salePrice)}</strong></span>
              <span>Coste: <strong className="text-[var(--color-foreground)]">{money(p.cost)}</strong></span>
              <span>Beneficio: <strong className="text-[var(--color-foreground)]">{money(p.profit)}</strong></span>
            </span>
          ) : (
            "—"
          )
        }
      />

      {query.isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(query.error)}
        </div>
      ) : (
        <>
          <LedgerSection kind="income" projectId={projectId} title="Ingresos" />
          <LedgerSection kind="payment" projectId={projectId} title="Pagos" />
          <NotesSection projectId={projectId} />
        </>
      )}
    </div>
  );
}

// ───────────────────────────────────────────────────────────────────────
//  Ledger (Incomes / Payments) — same shape, different endpoints.
// ───────────────────────────────────────────────────────────────────────

type LedgerItem = {
  id: string;
  amount: number;
  description: string | null;
  date: string | null;
  confirmed: boolean;
  validated: boolean;
};

const LEDGER = {
  income: {
    queryKey: "incomes",
    search: searchIncomes,
    create: (projectId: string, amount: number, description: string | null, date: string | null) =>
      createIncome({ projectId, amount, description, date }),
    confirm: confirmIncome,
    validate: validateIncome,
    remove: deleteIncome,
  },
  payment: {
    queryKey: "payments",
    search: searchPayments,
    create: (projectId: string, amount: number, description: string | null, date: string | null) =>
      createPayment({ projectId, amount, description, date }),
    confirm: confirmPayment,
    validate: validatePayment,
    remove: deletePayment,
  },
} as const;

function LedgerSection({ kind, projectId, title }: { kind: "income" | "payment"; projectId: string; title: string }) {
  const cfg = LEDGER[kind];
  const queryClient = useQueryClient();
  const [adding, setAdding] = useState(false);
  const key = ["projects", "detail", projectId, cfg.queryKey];

  const query = useQuery({
    queryKey: key,
    queryFn: () => cfg.search({ projectId, pageSize: 100, sortBy: "date", sortDir: "desc" }) as Promise<PagedResponse<LedgerItem>>,
    enabled: projectId.length > 0,
    placeholderData: keepPreviousData,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: key });

  const confirm = useMutation({
    mutationFn: (id: string) => cfg.confirm(id),
    onSuccess: () => { toast.success("Confirmado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const validate = useMutation({
    mutationFn: (id: string) => cfg.validate(id),
    onSuccess: () => { toast.success("Validado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const remove = useMutation({
    mutationFn: (id: string) => cfg.remove(id),
    onSuccess: () => { toast.success("Borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const items = query.data?.items ?? [];
  const total = items.reduce((sum, i) => sum + i.amount, 0);

  return (
    <section className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
      <div className="flex items-center justify-between border-b border-[var(--color-border)] px-5 py-3.5">
        <div className="flex items-baseline gap-2">
          <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">{title}</h2>
          <span className="font-mono text-[11px] text-[var(--color-muted-foreground)] tabular-nums">
            {items.length} · total {money(total)}
          </span>
        </div>
        <Button variant="soft" onClick={() => setAdding(true)} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
          <Plus className="size-3.5" />
          Añadir
        </Button>
      </div>

      {items.length === 0 ? (
        <p className="px-5 py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {query.isLoading ? "Cargando…" : `Sin ${title.toLowerCase()} todavía.`}
        </p>
      ) : (
        <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.4)]">
          {items.map((it) => (
            <li key={it.id} className="flex items-center gap-3 px-5 py-3">
              <div className="min-w-0 flex-1">
                <div className="flex items-center gap-2">
                  <span className="text-[14px] font-medium text-[var(--color-foreground)] tabular-nums">{money(it.amount)}</span>
                  {it.confirmed && <EntityStatusBadge tone="success" withDot>Confirmado</EntityStatusBadge>}
                  {it.validated && <EntityStatusBadge tone="info" withDot>Validado</EntityStatusBadge>}
                </div>
                <p className="mt-0.5 truncate text-[12px] text-[var(--color-muted-foreground)]">
                  {formatDate(it.date)}{it.description ? ` · ${it.description}` : ""}
                </p>
              </div>
              <div className="flex shrink-0 items-center gap-1">
                {!it.confirmed && (
                  <button
                    type="button"
                    title="Confirmar"
                    aria-label="Confirmar"
                    disabled={confirm.isPending}
                    onClick={() => confirm.mutate(it.id)}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-success)]"
                  >
                    <CheckCircle2 className="size-4" />
                  </button>
                )}
                {!it.validated && (
                  <button
                    type="button"
                    title="Validar"
                    aria-label="Validar"
                    disabled={validate.isPending}
                    onClick={() => validate.mutate(it.id)}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-info)]"
                  >
                    <BadgeCheck className="size-4" />
                  </button>
                )}
                <button
                  type="button"
                  title="Borrar"
                  aria-label="Borrar"
                  disabled={remove.isPending}
                  onClick={() => remove.mutate(it.id)}
                  className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
                >
                  <Trash2 className="size-3.5" />
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      <AddAmountDialog
        open={adding}
        title={title}
        onClose={() => setAdding(false)}
        onSubmit={(amount, description, date) =>
          cfg.create(projectId, amount, description, date).then(() => {
            toast.success("Creado");
            invalidate();
            setAdding(false);
          })
        }
      />
    </section>
  );
}

function AddAmountDialog({
  open,
  title,
  onClose,
  onSubmit,
}: {
  open: boolean;
  title: string;
  onClose: () => void;
  onSubmit: (amount: number, description: string | null, date: string | null) => Promise<unknown>;
}) {
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [date, setDate] = useState("");
  const [pending, setPending] = useState(false);

  useEffect(() => {
    if (open) {
      setAmount("");
      setDescription("");
      setDate("");
    }
  }, [open]);

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const amt = toNum(amount);
    if (amt === null) return;
    setPending(true);
    onSubmit(amt, description.trim() || null, dateToIso(date))
      .catch((err) => toast.error("Error al crear", { description: describe(err) }))
      .finally(() => setPending(false));
  };

  return (
    <Dialog open={open} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <form onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Añadir a {title}</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <Field id="a-amount" label="Importe" required>
              <Input id="a-amount" type="number" step="any" value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0" autoFocus required />
            </Field>
            <Field id="a-date" label="Fecha">
              <Input id="a-date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
            </Field>
            <Field id="a-desc" label="Descripción">
              <Input id="a-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Anticipo, factura…" />
            </Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={pending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={pending || toNum(amount) === null}>
              {pending ? "Guardando…" : "Añadir"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ───────────────────────────────────────────────────────────────────────
//  Notes
// ───────────────────────────────────────────────────────────────────────

function NotesSection({ projectId }: { projectId: string }) {
  const queryClient = useQueryClient();
  const [adding, setAdding] = useState(false);
  const key = ["projects", "detail", projectId, "notes"];

  const query = useQuery({
    queryKey: key,
    queryFn: () => searchNotes({ projectId, pageSize: 100, sortBy: "date", sortDir: "desc" }),
    enabled: projectId.length > 0,
    placeholderData: keepPreviousData,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: key });

  const remove = useMutation({
    mutationFn: (id: string) => deleteNote(id),
    onSuccess: () => { toast.success("Nota borrada"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const items = query.data?.items ?? [];

  return (
    <section className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
      <div className="flex items-center justify-between border-b border-[var(--color-border)] px-5 py-3.5">
        <div className="flex items-baseline gap-2">
          <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Notas</h2>
          <span className="font-mono text-[11px] text-[var(--color-muted-foreground)]">{items.length}</span>
        </div>
        <Button variant="soft" onClick={() => setAdding(true)} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
          <Plus className="size-3.5" />
          Añadir
        </Button>
      </div>

      {items.length === 0 ? (
        <p className="px-5 py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {query.isLoading ? "Cargando…" : "Sin notas todavía."}
        </p>
      ) : (
        <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.4)]">
          {items.map((n) => (
            <li key={n.id} className="flex items-start gap-3 px-5 py-3">
              <StickyNote className="mt-0.5 size-4 shrink-0 text-[var(--color-muted-foreground)]" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{n.title}</p>
                {n.description && <p className="mt-0.5 text-[12.5px] text-[var(--color-muted-foreground)]">{n.description}</p>}
              </div>
              <button
                type="button"
                title="Borrar"
                aria-label="Borrar nota"
                disabled={remove.isPending}
                onClick={() => remove.mutate(n.id)}
                className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
              >
                <Trash2 className="size-3.5" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <AddNoteDialog
        open={adding}
        onClose={() => setAdding(false)}
        onSubmit={(title, description) =>
          createNote({ projectId, title, description }).then(() => {
            toast.success("Nota creada");
            invalidate();
            setAdding(false);
          })
        }
      />
    </section>
  );
}

function AddNoteDialog({
  open,
  onClose,
  onSubmit,
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (title: string, description: string | null) => Promise<unknown>;
}) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [pending, setPending] = useState(false);

  useEffect(() => {
    if (open) {
      setTitle("");
      setDescription("");
    }
  }, [open]);

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const t = title.trim();
    if (!t) return;
    setPending(true);
    onSubmit(t, description.trim() || null)
      .catch((err) => toast.error("Error al crear", { description: describe(err) }))
      .finally(() => setPending(false));
  };

  return (
    <Dialog open={open} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <form onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Añadir nota</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <Field id="n-title" label="Título" required>
              <Input id="n-title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Recordatorio…" autoFocus required />
            </Field>
            <Field id="n-desc" label="Descripción">
              <Input id="n-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Detalle de la nota" />
            </Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={pending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={pending || !title.trim()}>
              {pending ? "Guardando…" : "Añadir"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
