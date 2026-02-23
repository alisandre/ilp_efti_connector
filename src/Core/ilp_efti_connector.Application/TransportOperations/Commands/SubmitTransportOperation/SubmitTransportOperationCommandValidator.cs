using FluentValidation;

namespace ilp_efti_connector.Application.TransportOperations.Commands.SubmitTransportOperation;

public sealed class SubmitTransportOperationCommandValidator
    : AbstractValidator<SubmitTransportOperationCommand>
{
    public SubmitTransportOperationCommandValidator()
    {
        RuleFor(x => x.SourceId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.OperationCode)
            .NotEmpty().WithMessage("Il codice operazione (eCMRID) è obbligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.DatasetType)
            .NotEmpty()
            .Must(v => v is "ECMR" or "EDDT")
            .WithMessage("DatasetType deve essere 'ECMR' o 'EDDT'.");

        // Almeno un vettore con targa obbligatoria (regola MILOS)
        RuleFor(x => x.Carriers)
            .NotEmpty().WithMessage("È richiesto almeno un vettore.");

        RuleForEach(x => x.Carriers).ChildRules(carrier =>
        {
            carrier.RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
            carrier.RuleFor(c => c.TractorPlate)
                .NotEmpty().WithMessage("La targa del vettore è obbligatoria.")
                .MaximumLength(20);
        });

        // Consignee: se presente, nome obbligatorio
        When(x => x.Consignee is not null, () =>
        {
            RuleFor(x => x.Consignee!.Name).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Consignee!.CityName).NotEmpty();
            RuleFor(x => x.Consignee!.CountryCode)
                .NotEmpty().Length(2).WithMessage("CountryCode deve essere ISO 3166-1 alpha-2.");
        });
    }
}
