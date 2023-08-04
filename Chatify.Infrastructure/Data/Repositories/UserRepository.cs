using AutoMapper.QueryableExtensions;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Common.Caching.Extensions;
using Chatify.Infrastructure.Data.Models;
using StackExchange.Redis;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository
    : BaseCassandraRepository<Domain.Entities.User, ChatifyUser, Guid>, IUserRepository
{
    private readonly IDatabase _cache;

    public UserRepository(IMapper mapper, Mapper dbMapper, IDatabase cache)
        : base(mapper, dbMapper)
    {
        _cache = cache;
    }

    public Task<Domain.Entities.User?> GetByUsername(string usernameQuery,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Domain.Entities.User>?> AllByUserIds(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var cacheUsers = await _cache.GetAsync<ChatifyUser>(userIds.Select(id => $"user:{id.ToString()}"));
        return cacheUsers
            .AsQueryable()
            .ProjectTo<Domain.Entities.User>(Mapper.ConfigurationProvider)
            .ToList();
    }
}