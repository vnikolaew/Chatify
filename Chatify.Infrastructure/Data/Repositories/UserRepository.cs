using Cassandra.Mapping;
using Chatify.Domain.Repositories;
using Chatify.Infrastructure.Data.Models;
using Chatify.Shared.Abstractions.Serialization;
using StackExchange.Redis;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository
    : BaseCassandraRepository<Domain.Entities.User, ChatifyUser, Guid>, IUserRepository
{
    private readonly IDatabase _cache;
    private readonly ISerializer _serializer;

    public UserRepository(IMapper mapper, Mapper dbMapper, IDatabase cache, ISerializer serializer)
        : base(mapper, dbMapper)
    {
        _cache = cache;
        _serializer = serializer;
    }

    public Task<Domain.Entities.User?> GetByUsername(string usernameQuery,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Domain.Entities.User>?> AllByUserIds(
        IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var users = await _cache.StringGetAsync(
            userIds.Select(id => new RedisKey($"user:{id.ToString()}")).ToArray()
        );

        return Mapper.Map<List<Domain.Entities.User>>(
            users
                .Select(u =>
                    _serializer.Deserialize<ChatifyUser>(u.ToString())));
    }
}