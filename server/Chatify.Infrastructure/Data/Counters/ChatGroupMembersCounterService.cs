using Cassandra.Mapping;
using Chatify.Infrastructure.Data.Models;

namespace Chatify.Infrastructure.Data.Counters;

public sealed class ChatGroupMembersCounterService(IMapper mapper)
    : BaseCounterService<ChatGroupMembersCount, Guid>(c => c.MembersCount, mapper);