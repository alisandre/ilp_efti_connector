using FluentValidation;
using ilp_efti_connector.Shared.Contracts.Dtos;

namespace ilp_efti_connector.FormInputService.Validators;

public sealed class SourcePayloadDtoValidator : AbstractValidator<SourcePayloadDto>
{
    private static readonly string[] ValidDatasetTypes =
        ["ECMR", "EDDT", "eAWB", "eBL", "eRSD", "eDAD"];

    public SourcePayloadDtoValidator()
    {
        RuleFor(x => x.OperationCode)
            .NotEmpty().WithMessage("OperationCode è obbligatorio.")
            .MaximumLength(100).WithMessage("OperationCode non può superare 100 caratteri.");

        RuleFor(x => x.DatasetType)
            .NotEmpty().WithMessage("DatasetType è obbligatorio.")
            .Must(t => ValidDatasetTypes.Contains(t))
            .WithMessage($"DatasetType non valido. Valori ammessi: {string.Join(", ", ValidDatasetTypes)}.");

        RuleFor(x => x.CustomerCode)
            .NotEmpty().WithMessage("CustomerCode è obbligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("CustomerName è obbligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Consignee)
            .NotNull().WithMessage("Destinatario (Consignee) è obbligatorio.");

        When(x => x.Consignee is not null, () =>
        {
            RuleFor(x => x.Consignee!.Name)
                .NotEmpty().WithMessage("Consignee.Name è obbligatorio.")
                .MaximumLength(200);

            RuleFor(x => x.Consignee!.CityName)
                .NotEmpty().WithMessage("Consignee.CityName è obbligatorio.");

            RuleFor(x => x.Consignee!.CountryCode)
                .NotEmpty()
                .Length(2).WithMessage("Consignee.CountryCode deve essere ISO 3166-1 alpha-2 (2 caratteri).");
        });

        RuleFor(x => x.Carriers)
            .NotEmpty().WithMessage("Almeno un vettore è obbligatorio.");

        RuleForEach(x => x.Carriers).ChildRules(c =>
        {
            c.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Carrier.Name è obbligatorio.")
                .MaximumLength(200);

            c.RuleFor(x => x.TractorPlate)
                .NotEmpty().WithMessage("Carrier.TractorPlate (targa trattore) è obbligatoria.")
                .MaximumLength(20);

            c.RuleFor(x => x.PlayerType)
                .NotEmpty().WithMessage("Carrier.PlayerType è obbligatorio.");
        });

        When(x => x.ConsignmentItems is not null, () =>
        {
            RuleFor(x => x.ConsignmentItems!.TotalItemQuantity)
                .GreaterThan(0).WithMessage("ConsignmentItems.TotalItemQuantity deve essere > 0.");

            RuleFor(x => x.ConsignmentItems!.TotalWeight)
                .GreaterThan(0m).WithMessage("ConsignmentItems.TotalWeight deve essere > 0.");
        });

        When(x => x.AcceptanceLocation is not null, () =>
        {
            RuleFor(x => x.AcceptanceLocation!.CityName)
                .NotEmpty().WithMessage("AcceptanceLocation.CityName è obbligatorio.");

            RuleFor(x => x.AcceptanceLocation!.CountryCode)
                .NotEmpty().Length(2).WithMessage("AcceptanceLocation.CountryCode deve essere ISO 3166-1 alpha-2.");
        });

        When(x => x.DeliveryLocation is not null, () =>
        {
            RuleFor(x => x.DeliveryLocation!.CityName)
                .NotEmpty().WithMessage("DeliveryLocation.CityName è obbligatorio.");

            RuleFor(x => x.DeliveryLocation!.CountryCode)
                .NotEmpty().Length(2).WithMessage("DeliveryLocation.CountryCode deve essere ISO 3166-1 alpha-2.");
        });
    }
}
