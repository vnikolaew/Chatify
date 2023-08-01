using AutoMapper;
using Chatify.Domain.Entities;
using Mapper = Cassandra.Mapping.Mapper;

namespace Chatify.Infrastructure.Data.Repositories;

public sealed class ChatMessageRepository :
    BaseCassandraRepository<ChatMessage, Models.ChatMessage, Guid>
{
    public ChatMessageRepository(
        IMapper mapper, Mapper dbMapper)
        : base(mapper, dbMapper)
    {
    }
}