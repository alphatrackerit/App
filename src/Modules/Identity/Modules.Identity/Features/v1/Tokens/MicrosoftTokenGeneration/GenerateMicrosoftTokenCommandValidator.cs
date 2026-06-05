using FluentValidation;
using FSH.Modules.Identity.Contracts.v1.Tokens.MicrosoftTokenGeneration;

namespace FSH.Modules.Identity.Features.v1.Tokens.MicrosoftTokenGeneration;

public sealed class GenerateMicrosoftTokenCommandValidator : AbstractValidator<GenerateMicrosoftTokenCommand>
{
    public GenerateMicrosoftTokenCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}
