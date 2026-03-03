using ilp_efti_connector.Application.AuditLogs.Queries.GetAuditLogs;
using ilp_efti_connector.Domain.Entities;
using ilp_efti_connector.Domain.Enums;
using ilp_efti_connector.Domain.Interfaces.Repositories;

namespace ilp_efti_connector.Application.Tests.AuditLogs.Queries;

public sealed class GetAuditLogsQueryHandlerTests
{
    private readonly Mock<IAuditLogRepository> _repo = new();
    private readonly GetAuditLogsQueryHandler  _sut;

    public GetAuditLogsQueryHandlerTests()
        => _sut = new GetAuditLogsQueryHandler(_repo.Object);

    // ─── Mappatura ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldMap_AllFields_ToDto()
    {
        var id  = Guid.NewGuid();
        var log = new AuditLog
        {
            Id                  = id,
            EntityType          = AuditEntityType.TransportOperation,
            EntityId            = Guid.NewGuid(),
            ActionType          = AuditActionType.Create,
            PerformedByUserId   = null,
            PerformedBySourceId = Guid.NewGuid(),
            Description         = "Operazione creata",
            OldValueJson        = null,
            NewValueJson        = "{}",
            IpAddress           = "127.0.0.1",
            UserAgent           = null,
            CreatedAt           = DateTime.UtcNow
        };
        SetupRepo([log], 1);

        var (items, total) = await _sut.Handle(new GetAuditLogsQuery(null, null, null, null, null, null), default);

        total.Should().Be(1);
        items.Should().ContainSingle();
        var dto = items[0];
        dto.Id.Should().Be(id);
        dto.EntityType.Should().Be(AuditEntityType.TransportOperation);
        dto.ActionType.Should().Be(AuditActionType.Create);
        dto.Description.Should().Be("Operazione creata");
        dto.IpAddress.Should().Be("127.0.0.1");
    }

    // ─── Paginazione ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldClamp_PageSize_ToMax100()
    {
        SetupRepo([], 0);

        await _sut.Handle(new GetAuditLogsQuery(null, null, null, null, null, null, PageSize: 500), default);

        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<AuditEntityType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<AuditActionType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int>(),
            100,
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldClamp_Page_ToMin1()
    {
        SetupRepo([], 0);

        await _sut.Handle(new GetAuditLogsQuery(null, null, null, null, null, null, Page: -5), default);

        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<AuditEntityType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<AuditActionType?>(),
            It.IsAny<Guid?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            1,
            It.IsAny<int>(),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturn_EmptyList()
    {
        SetupRepo([], 0);

        var (items, total) = await _sut.Handle(new GetAuditLogsQuery(null, null, null, null, null, null), default);

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    // ─── Passaggio filtri ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldPass_Filters_ToRepository()
    {
        var entityId  = Guid.NewGuid();
        var userId    = Guid.NewGuid();
        var from      = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to        = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        SetupRepo([], 0);

        await _sut.Handle(new GetAuditLogsQuery(
            EntityType:        AuditEntityType.Customer,
            EntityId:          entityId,
            ActionType:        AuditActionType.Update,
            PerformedByUserId: userId,
            From:              from,
            To:                to), default);

        _repo.Verify(r => r.GetPagedAsync(
            AuditEntityType.Customer,
            entityId,
            AuditActionType.Update,
            userId,
            from,
            to,
            1,
            20,
            default), Times.Once);
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private void SetupRepo(IReadOnlyList<AuditLog> items, int total)
        => _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<AuditEntityType?>(),
                It.IsAny<Guid?>(),
                It.IsAny<AuditActionType?>(),
                It.IsAny<Guid?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                default))
            .ReturnsAsync((items, total));
}
