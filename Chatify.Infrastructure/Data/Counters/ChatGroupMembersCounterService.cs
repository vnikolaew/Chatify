using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Counters;

public class ChatGroupMembersCounterService : BaseCounterService<ChatGroupMembersCount, Guid>
{
    public ChatGroupMembersCounterService(IMapper mapper)
        : base(c => c.MembersCount, mapper)
    {
    }
}