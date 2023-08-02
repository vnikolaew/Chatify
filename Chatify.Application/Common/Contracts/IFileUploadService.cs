using Chatify.Application.Common.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Common.Contracts;

public interface IFileUploadService
{
    Task<Either<FileUploadResult, Error>> UploadAsync(
        SingleFileUploadRequest singleFileUploadRequest,
        CancellationToken cancellationToken = default);
    
    Task<List<Either<FileUploadResult, Error>>> UploadManyAsync(
        MultipleFileUploadRequest multipleFileUploadRequest,
        CancellationToken cancellationToken = default);
}

public class SingleFileUploadRequest
{
    public InputFile File { get; set; } = default!;
    
    public Guid? UserId { get; set; }
}

public class MultipleFileUploadRequest
{
    public IEnumerable<InputFile> Files { get; set; }
    
    public Guid? UserId { get; set; }
}

public class FileUploadResult
{
    public Guid FileId { get; set; }

    public string FileUrl { get; set; } = default!;
}