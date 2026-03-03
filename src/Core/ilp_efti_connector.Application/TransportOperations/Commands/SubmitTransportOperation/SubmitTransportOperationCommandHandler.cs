using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Exceptions;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using ilp_efti_connector.Domain.ValueObjects;
using MediatR;

namespace ilp_efti_connector.Application.TransportOperations.Commands.SubmitTransportOperation;

public sealed class SubmitTransportOperationCommandHandler
    : IRequestHandler<SubmitTransportOperationCommand, SubmitTransportOperationResult>
{
    private readonly ITransportOperationRepository _operations;
    private readonly IEftiMessageRepository _messages;
    private readonly IUnitOfWork _uow;

    public SubmitTransportOperationCommandHandler(
        ITransportOperationRepository operations,
        IEftiMessageRepository messages,
        IUnitOfWork uow)
    {
        _operations = operations;
        _messages   = messages;
        _uow        = uow;
    }

    public async Task<SubmitTransportOperationResult> Handle(SubmitTransportOperationCommand cmd, CancellationToken ct)
    {
        if (await _operations.ExistsByCodeAsync(cmd.OperationCode, ct))
            throw new DuplicateOperationCodeException(cmd.OperationCode);

        var now = DateTime.UtcNow;

        var operation = new TransportOperation
        {
            Id             = cmd.TransportOperationId ?? Guid.NewGuid(),
            SourceId       = cmd.SourceId,
            CustomerId     = cmd.CustomerId,
            DestinationId  = cmd.DestinationId,
            OperationCode  = cmd.OperationCode,
            DatasetType    = cmd.DatasetType,
            Status         = TransportOperationStatus.VALIDATED,
            RawPayloadJson = cmd.RawPayloadJson,
            CreatedAt      = now,
            UpdatedAt      = now
        };

        // Consignee
        if (cmd.Consignee is { } c)
        {
            operation.Consignee = new TransportConsignee
            {
                Id                    = Guid.NewGuid(),
                TransportOperationId  = operation.Id,
                Name                  = c.Name,
                PlayerType            = c.PlayerType,
                TaxRegistration       = c.TaxRegistration,
                EoriCode              = c.EoriCode,
                PostalAddress         = new PostalAddress
                {
                    StreetName   = c.StreetName,
                    PostCode     = c.PostCode,
                    CityName     = c.CityName,
                    CountryCode  = c.CountryCode,
                    CountryName  = c.CountryName
                }
            };
        }

        // Carriers
        foreach (var carrier in cmd.Carriers)
        {
            operation.Carriers.Add(new TransportCarrier
            {
                Id                   = Guid.NewGuid(),
                TransportOperationId = operation.Id,
                SortOrder            = carrier.SortOrder,
                Name                 = carrier.Name,
                PlayerType           = carrier.PlayerType,
                TractorPlate         = carrier.TractorPlate,
                TaxRegistration      = carrier.TaxRegistration,
                EoriCode             = carrier.EoriCode,
                EquipmentCategory    = carrier.EquipmentCategory,
                PostalAddress        = new PostalAddress
                {
                    StreetName  = carrier.StreetName,
                    PostCode    = carrier.PostCode,
                    CityName    = carrier.CityName,
                    CountryCode = carrier.CountryCode,
                    CountryName = carrier.CountryName
                }
            });
        }

        // Transport detail
        if (cmd.Detail is { } d)
        {
            operation.Detail = new TransportDetail
            {
                Id                   = Guid.NewGuid(),
                TransportOperationId = operation.Id,
                CargoType            = d.CargoType,
                Incoterms            = d.Incoterms,
                AcceptanceDate       = d.AcceptanceDate,
                AcceptanceAddress    = new PostalAddress
                {
                    StreetName  = d.AcceptanceStreetName,
                    PostCode    = d.AcceptancePostCode,
                    CityName    = d.AcceptanceCityName    ?? string.Empty,
                    CountryCode = d.AcceptanceCountryCode ?? string.Empty,
                    CountryName = d.AcceptanceCountryName
                },
                ReceiptAddress = new PostalAddress
                {
                    StreetName  = d.ReceiptStreetName,
                    PostCode    = d.ReceiptPostCode,
                    CityName    = d.ReceiptCityName    ?? string.Empty,
                    CountryCode = d.ReceiptCountryCode ?? string.Empty,
                    CountryName = d.ReceiptCountryName
                }
            };
        }

        // Consignment + packages
        if (cmd.ConsignmentItem is { } goods)
        {
            var consignment = new TransportConsignmentItem
            {
                Id                   = Guid.NewGuid(),
                TransportOperationId = operation.Id,
                TotalItemQuantity    = goods.TotalItemQuantity,
                TotalWeight          = goods.TotalWeight,
                TotalVolume          = goods.TotalVolume
            };

            foreach (var pkg in goods.Packages)
            {
                consignment.Packages.Add(new TransportPackage
                {
                    Id                = Guid.NewGuid(),
                    ConsignmentItemId = consignment.Id,
                    SortOrder         = pkg.SortOrder,
                    ShippingMarks     = pkg.ShippingMarks,
                    ItemQuantity      = pkg.ItemQuantity,
                    TypeCode          = pkg.TypeCode,
                    GrossWeight       = pkg.GrossWeight,
                    GrossVolume       = pkg.GrossVolume
                });
            }

            operation.ConsignmentItem = consignment;
        }

        await _operations.AddAsync(operation, ct);

        // Crea il primo EftiMessage in stato PENDING
        var message = new EftiMessage
        {
            Id                   = Guid.NewGuid(),
            SourceId             = cmd.SourceId,
            TransportOperationId = operation.Id,
            CorrelationId        = Guid.NewGuid(),
            GatewayProvider      = cmd.GatewayProvider,
            Direction            = MessageDirection.OUTBOUND,
            DatasetType          = cmd.DatasetType,
            Status               = MessageStatus.PENDING,
            PayloadJson          = cmd.RawPayloadJson ?? string.Empty,
            RetryCount           = 0,
            CreatedAt            = now
        };

        await _messages.AddAsync(message, ct);
        await _uow.SaveChangesAsync(ct);

        return new SubmitTransportOperationResult(operation.Id, message.Id);
    }
}
