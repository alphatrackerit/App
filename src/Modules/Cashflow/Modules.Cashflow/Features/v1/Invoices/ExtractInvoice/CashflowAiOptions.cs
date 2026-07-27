namespace FSH.Modules.Cashflow.Features.v1.Invoices.ExtractInvoice;

/// <summary>
/// Configuration for the AI-assisted invoice extraction. The API key is a real billable secret —
/// it comes from configuration/environment (<c>CashflowAi__ApiKey</c>, falling back to
/// <c>ANTHROPIC_API_KEY</c>), never from source control. With no key configured the extract
/// endpoint returns a clear 503 and the rest of the invoice form keeps working.
/// </summary>
public sealed class CashflowAiOptions
{
    public const string SectionName = "CashflowAi";

    public string? ApiKey { get; set; }

    /// <summary>Anthropic model id. Vision-capable; claude-opus-4-8 by default.</summary>
    public string Model { get; set; } = "claude-opus-4-8";

    public int MaxOutputTokens { get; set; } = 2048;
}
