using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ExtractInvoice;

public sealed class ExtractInvoiceCommandValidator : AbstractValidator<ExtractInvoiceCommand>
{
    private const int MaxBytes = 10 * 1024 * 1024; // storage FileType.Pdf cap

    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf", "image/png", "image/jpeg", "image/webp", "image/gif",
    ];

    public ExtractInvoiceCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El documento está vacío.")
            .Must(c => c.Length <= MaxBytes).WithMessage("El documento supera el máximo de 10 MB.");
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Formato no soportado: usa PDF, PNG, JPG, WEBP o GIF.");
    }
}
