using Chatify.Application.Common.Contracts;
using Chatify.Application.Common.Models;
using Chatify.Domain.Entities;
using Chatify.Shared.Abstractions.Commands;
using Chatify.Shared.Abstractions.Contexts;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Messages.Commands.Common;

internal abstract class SendChatMessageBaseHandler<TRequest, TResponse>(
        IFileUploadService fileUploadService,
        IIdentityContext identityContext)
    : ICommandHandler<TRequest, TResponse> where TRequest : class, ICommand<TResponse>
{
    public abstract Task<TResponse> HandleAsync(TRequest command,
        CancellationToken cancellationToken = default);

    protected static List<Media> GetMediae(IEnumerable<OneOf<Error, FileUploadResult>> fileUploadResults)
        => fileUploadResults
            .Where(r => r.IsT1)
            .Select(r => r.AsT1)
            .Select(r => new Media
            {
                Id = r.FileId,
                MediaUrl = r.FileUrl,
                Type = r.FileType,
                FileName = r.FileName
            }).ToList();

    protected async Task<List<OneOf<Error, FileUploadResult>>> HandleFileUploads(
        IEnumerable<InputFile>? inputFiles,
        CancellationToken cancellationToken)
    {
        if ( inputFiles is null || !inputFiles.Any() ) return new List<OneOf<Error, FileUploadResult>>();

        var uploadRequest = new MultipleFileUploadRequest
        {
            Files = inputFiles.ToList(),
            UserId = identityContext.Id
        };

        return await fileUploadService.UploadManyAsync(uploadRequest, cancellationToken);
    }
}