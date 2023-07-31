using Cassandra;
using Cassandra.Mapping;
using Chatify.Domain.Common;
using Chatify.Domain.Entities;
using Chatify.Infrastructure.Data.Models;
using Guid = System.Guid;
using IMapper = AutoMapper.IMapper;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class UserRepository : IDomainRepository<User, Guid>
{
    private readonly IMapper _mapper;
    private readonly Mapper _dbMapper;

    public UserRepository(IMapper mapper, ISession session)
    {
        _mapper = mapper;
        _dbMapper = new Mapper(session);
    }

    public async Task<User> SaveAsync(
        User entity,
        CancellationToken cancellationToken = default)
    {
        var dataEntity = _mapper.Map<ChatifyUser>(entity);
        await _dbMapper.InsertAsync(dataEntity,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));

        return entity;
    }

    public async Task<User?> UpdateAsync(
        Guid id,
        Action<User> updateAction,
        CancellationToken cancellationToken = default)
    {
        var dataUser = await _dbMapper.FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?", id);
        if (dataUser is null) return default;

        var user = _mapper.Map<User>(dataUser);
        updateAction(user);
        _mapper.Map(user, dataUser);
        
        await _dbMapper.UpdateAsync(dataUser,
            new CqlQueryOptions()
                .SetConsistencyLevel(ConsistencyLevel.Quorum)
                .SetRetryPolicy(new DefaultRetryPolicy()));
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbMapper.DeleteAsync<ChatifyUser>("WHERE id = ?", id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbMapper.FirstOrDefaultAsync<ChatifyUser>("WHERE id = ?", id);
        return user is null ? default : _mapper.Map<User>(user);
    }
}