﻿namespace Chatify.Shared.Abstractions.Contexts;

public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    
    public Guid Id { get; }
    
    public string Username { get; }
    
    string Role { get; }
    
    Dictionary<string, IEnumerable<string>> Claims { get; }
}