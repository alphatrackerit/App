import { useRef, useState, type ChangeEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { FileText, Paperclip, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  createDocumento,
  deleteDocumento,
  searchDocumentos,
  TIPO_DOCUMENTO,
  type DocumentoOrigen,
  type TipoDocumento,
} from "@/api/avicola";
import { Visibility } from "@/api/files";
import { useFileUpload } from "@/hooks/use-file-upload";
import { Button } from "@/components/ui/button";
import { Combobox, Field } from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

/**
 * Reusable documents panel for an Avícola entity (flock, shed, order…). Uploads via the Files
 * presigned flow (Public so the stored URL is durable) and persists an Avícola Documento reference.
 */
export function DocumentosSection({
  origen,
  origenId,
  defaultTipo = "Otro",
  title = "Documentos",
}: {
  origen: DocumentoOrigen;
  origenId: string;
  defaultTipo?: TipoDocumento;
  title?: string;
}) {
  const queryClient = useQueryClient();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [tipo, setTipo] = useState<TipoDocumento>(defaultTipo);

  const key = ["avicola", "documentos", origen, origenId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchDocumentos({ origen, origenId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: origenId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => queryClient.invalidateQueries({ queryKey: key });

  const { upload, isUploading, progress } = useFileUpload({
    ownerType: "AvicolaDocumento",
    ownerId: origenId,
    category: "Document",
    visibility: Visibility.Public,
  });

  const del = useMutation({
    mutationFn: (id: string) => deleteDocumento(id),
    onSuccess: () => { toast.success("Documento borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const onPick = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (inputRef.current) inputRef.current.value = "";
    if (!file) return;
    try {
      const dto = await upload(file);
      await createDocumento({
        tipo,
        origen,
        origenId,
        fileAssetId: dto.id,
        url: dto.publicUrl ?? "",
        nombreArchivo: file.name,
        contentType: file.type || null,
      });
      toast.success("Documento adjuntado");
      invalidate();
    } catch (err) {
      toast.error("Error al adjuntar", { description: describe(err) });
    }
  };

  const items = q.data?.items ?? [];

  return (
    <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)]">
      <header className="flex flex-wrap items-center justify-between gap-2 border-b border-[var(--color-border)] px-4 py-3">
        <div className="flex items-center gap-2">
          <Paperclip className="size-4 text-[var(--color-muted-foreground)]" />
          <h2 className="text-[14px] font-semibold text-[var(--color-foreground)]">{title}</h2>
          {q.data?.totalCount !== undefined && (
            <span className="rounded-full bg-[var(--color-muted)] px-2 py-0.5 text-[11px] tabular-nums text-[var(--color-muted-foreground)]">{q.data.totalCount}</span>
          )}
        </div>
        <div className="flex items-end gap-2">
          <Field id={`doc-tipo-${origenId}`} label="Tipo">
            <div className="w-48">
              <Combobox
                id={`doc-tipo-${origenId}`}
                label="Tipo"
                variant="field"
                value={tipo}
                onChange={(v) => setTipo((v ?? "Otro") as TipoDocumento)}
                options={TIPO_DOCUMENTO.map((t) => ({ value: t.value, label: t.label }))}
              />
            </div>
          </Field>
          <input ref={inputRef} type="file" className="hidden" onChange={onPick} />
          <Button
            variant="outline"
            onClick={() => inputRef.current?.click()}
            disabled={isUploading}
            className="h-9 gap-1.5 rounded-lg px-3 text-[12.5px]"
          >
            <Paperclip className="size-3.5" />
            {isUploading ? `Subiendo… ${progress?.percent ?? 0}%` : "Adjuntar"}
          </Button>
        </div>
      </header>

      <div className="divide-y divide-[var(--color-border)]">
        {items.length === 0 ? (
          <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">Sin documentos adjuntos.</div>
        ) : items.map((d) => (
          <div key={d.id} className="group flex items-center justify-between gap-3 px-4 py-2.5">
            <a
              href={d.url || undefined}
              target="_blank"
              rel="noreferrer"
              className="flex min-w-0 items-center gap-2 text-[13px] text-[var(--color-foreground)] hover:text-[var(--color-primary)]"
            >
              <FileText className="size-4 shrink-0 text-[var(--color-muted-foreground)]" />
              <span className="truncate">{d.nombreArchivo}</span>
            </a>
            <div className="flex items-center gap-3 text-[12px] text-[var(--color-muted-foreground)] tabular-nums">
              <span>{TIPO_DOCUMENTO.find((t) => t.value === d.tipo)?.label ?? d.tipo}</span>
              <span>{formatDate(d.fecha)}</span>
            </div>
            <button
              type="button"
              aria-label="Borrar documento"
              onClick={() => del.mutate(d.id)}
              className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] group-hover:opacity-100"
            >
              <Trash2 className="size-3.5" />
            </button>
          </div>
        ))}
      </div>
    </section>
  );
}
