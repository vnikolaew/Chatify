using Chatify.Application.Common.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Common.Contracts;

public interface IFileUploadService
{
    Task<Either<Error, FileUploadResult>> UploadAsync(
        SingleFileUploadRequest singleFileUploadRequest,
        CancellationToken cancellationToken = default);
    
    Task<Either<Error, bool>> DeleteAsync(
        SingleFileDeleteRequest singleFileDeleteRequest,
        CancellationToken cancellationToken = default);
    
    Task<List<Either<Error, FileUploadResult>>> UploadManyAsync(
        MultipleFileUploadRequest multipleFileUploadRequest,
        CancellationToken cancellationToken = default);
}

public class SingleFileDeleteRequest
{
    public string FileUrl { get; set; } = default!;
    
    public Guid? UserId { get; set; }
}

public class SingleFileUploadRequest
{
    public InputFile File { get; set; } = default!;
    
    public Guid? UserId { get; set; }
}

public class MultipleFileUploadRequest
{
    public IEnumerable<InputFile> Files { get; set; } = new List<InputFile>();
    
    public Guid? UserId { get; set; }
}

public class FileUploadResult
{
    public Guid FileId { get; set; }

    public string FileUrl { get; set; } = default!;
}