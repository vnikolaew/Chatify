﻿using Cassandra.Mapping.Attributes;
using Chatify.Application.Common.Mappings;
using Metadata = System.Collections.Generic.IDictionary<string, string>;
using ReactionCounts = System.Collections.Generic.IDictionary<short, long>;

namespace Chatify.Infrastructure.Data.Models;

public class ChatMessage : IMapFrom<Domain.Entities.ChatMessage>
{
    [SecondaryIndex] public Guid Id { get; set; }

    public Guid ChatGroupId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = default!;

    protected readonly HashSet<Media> _attachments = new();

    [FrozenKey]
    public IEnumerable<Media> Attachments => _attachments;

    public Metadata Metadata { get; set; } = new Dictionary<string, string>();

    public ReactionCounts ReactionCounts { get; set; } = new Dictionary<short, long>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool Updated => UpdatedAt.HasValue;
}