using System.ComponentModel.DataAnnotations;
using Chatify.Application.Common.Models;
using Chatify.Shared.Abstractions.Commands;

namespace Chatify.Application.Messages.Commands.Common;

public record SendChatMessageBase<TResponse>(
    [Required, MinLength(1), MaxLength(500)]
    string Content,
    IEnumerable<InputFile>? Attachments = default
) : ICommand<TResponse>;