using Chatify.Application.Common.Models;
using LanguageExt;
using LanguageExt.Common;
using OneOf;

namespace Chatify.Application.Common.Contracts;

public interface IFileUploadService
{
    Task<OneOf<Error, FileUploadResult>> UploadAsync(
        SingleFileUploadRequest singleFileUploadRequest,
        CancellationToken cancellationToken = default);
    
    Task<OneOf<Error, Unit>> DeleteAsync(
        SingleFileDeleteRequest singleFileDeleteRequest,
        CancellationToken cancellationToken = default);
    
    Task<List<OneOf<Error, FileUploadResult>>> UploadManyAsync(
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

    public string FileName { get; set; } = default!;

    public string FileType { get; set; } = default!;
    
    public string FileUrl { get; set; } = default!;
}