using FluentValidation;

namespace ilp_efti_connector.Application.Customers.Commands.UpsertCustomer;

public sealed class UpsertCustomerCommandValidator : AbstractValidator<UpsertCustomerCommand>
{
    public UpsertCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerCode)
            .NotEmpty().WithMessage("Il codice cliente è obbligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("La ragione sociale è obbligatoria.")
            .MaximumLength(300);

        RuleFor(x => x.VatNumber)
            .MaximumLength(50).When(x => x.VatNumber is not null);

        RuleFor(x => x.EoriCode)
            .MaximumLength(20).When(x => x.EoriCode is not null);

        RuleFor(x => x.SourceId)
            .NotEmpty().WithMessage("L'ID sorgente è obbligatorio.");

        // Validazione destinazione: se fornita, città e paese sono obbligatori
        When(x => !string.IsNullOrWhiteSpace(x.DestinationCode), () =>
        {
            RuleFor(x => x.City)
                .NotEmpty().WithMessage("La città della destinazione è obbligatoria.");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Il codice paese della destinazione è obbligatorio.")
                .Length(2).WithMessage("Il codice paese deve essere ISO 3166-1 alpha-2 (2 caratteri).");
        });
    }
}
