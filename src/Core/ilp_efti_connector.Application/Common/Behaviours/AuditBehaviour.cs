using ilp_efti_connector.Application.Common.Interfaces;
using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Interfaces.Repositories;
using MediatR;

namespace ilp_efti_connector.Application.Common.Behaviours;

/// <summary>
/// Pipeline MediatR che scrive automaticamente una riga di <see cref="AuditLog"/>
/// dopo l'esecuzione di qualsiasi comando che implementa <see cref="IAuditableCommand"/>.
/// L'audit è scritto solo a fronte di operazioni andate a buon fine.
/// <para>
/// Risoluzione dell'<c>EntityId</c>:
/// <list type="number">
///   <item>Se il comando implementa <see cref="IAuditableCommandWithEntityId"/> → usa <c>command.EntityId</c>.</item>
///   <item>Se la risposta implementa <see cref="IAuditableResult"/> → usa <c>result.AuditEntityId</c>.</item>
///   <item>Altrimenti → <c>Guid.Empty</c> (audit comunque scritto).</item>
/// </list>
/// </para>
/// </summary>
public sealed class AuditBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogRepository  _auditLogs;
    private readonly IUnitOfWork          _uow;
    private readonly ICurrentUserService  _currentUser;

    public AuditBehaviour(
        IAuditLogRepository auditLogs,
        IUnitOfWork         uow,
        ICurrentUserService currentUser)
    {
        _auditLogs   = auditLogs;
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest                          request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken                 ct)
    {
        var response = await next();

        if (request is not IAuditableCommand cmd)
            return response;

        var entityId = cmd is IAuditableCommandWithEntityId withId
            ? withId.EntityId
            : response is IAuditableResult result
                ? result.AuditEntityId
                : Guid.Empty;

        var entry = new AuditLog
        {
            Id                = Guid.NewGuid(),
            EntityType        = cmd.EntityType,
            EntityId          = entityId,
            ActionType        = cmd.ActionType,
            PerformedByUserId = _currentUser.UserId,
            Description       = cmd.AuditDescription,
            CreatedAt         = DateTime.UtcNow
        };

        await _auditLogs.AddAsync(entry, ct);
        await _uow.SaveChangesAsync(ct);

        return response;
    }
}
