namespace Chatify.Application.User.Contracts;

public interface IUsersService
{
    Task<List<Domain.Entities.User>> GetByIds(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
}