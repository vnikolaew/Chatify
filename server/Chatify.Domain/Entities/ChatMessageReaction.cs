﻿namespace Chatify.Domain.Entities;

public class ChatMessageReaction
{
    public Guid Id { get; set; }
    
    public Guid MessageId { get; set; }
    
    public ChatMessage Message { get; set; }
    
    public Guid ChatGroupId { get; set; }
    
    public ChatGroup ChatGroup { get; set; }
    
    public Guid UserId { get; set; }
    
    public string Username { get; set; } = default!;
    
    public long ReactionCode { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
}