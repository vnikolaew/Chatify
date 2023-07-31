using LanguageExt;
using LanguageExt.Common;

namespace Chatify.Application.Common.Contracts;

public interface IFileUploadService
{
    Task<Either<FileUploadResult, Error>> UploadAsync(
        FileUploadRequest fileUploadRequest,
        CancellationToken cancellationToken = default);
}

public class FileUploadRequest
{
    public Stream Data { get; set; } = default!;
    
    public Guid? UserId { get; set; }
    
    public string FileName { get; set; } = default!;

    public long SizeInBytes => Data.Length;
}

public class FileUploadResult
{
    public Guid FileId { get; set; }

    public string FileUrl { get; set; } = default!;
}