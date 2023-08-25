using AutoMapper;
using Chatify.Application.Common.Contracts;
using Chatify.Domain.Entities;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Mappings;
using Chatify.Infrastructure.Data.Extensions;
using Chatify.Infrastructure.Data.Services;
using Chatify.Shared.Abstractions.Queries;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class NotificationRepository(
        IMapper mapper,
        Mapper dbMapper,
        IEntityChangeTracker changeTracker,
        IPagingCursorHelper pagingCursorHelper,
        string? idColumn = default)
    : BaseCassandraRepository<UserNotification, Models.UserNotification, Guid>(mapper, dbMapper, changeTracker,
            idColumn),
        INotificationRepository
{
    public async Task<CursorPaged<UserNotification>> GetPaginatedForUserAsync(
        Guid userId,
        int pageSize,
        string? pagingCursor,
        CancellationToken cancellationToken = default)
    {
        var notificationsPage = await DbMapper.FetchPageAsync<Models.UserNotification>(
            pageSize, pagingCursorHelper.ToPagingState(pagingCursor), "WHERE user_id = ?",
            new object[] { userId });

        var newCursor = pagingCursorHelper.ToPagingCursor(notificationsPage.PagingState);
        return new CursorPaged<UserNotification>(
            notificationsPage
                .Select(_ => _.To<UserNotification>(Mapper))
                .ToList(),
            newCursor);
    }

    public async Task<List<UserNotification>> AllForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await DbMapper
            .FetchListAsync<Models.UserNotification>(" WHERE user_id = ?", userId);

        return notifications.Select(_ => _.To<UserNotification>(Mapper)).ToList();
    }
}