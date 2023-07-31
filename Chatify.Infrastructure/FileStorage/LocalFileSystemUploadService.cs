using Chatify.Application.Common.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Hosting;

namespace Chatify.Infrastructure.FileStorage;

public class LocalFileSystemUploadService : IFileUploadService
{
    private readonly string _fileStorageBaseFolder;
    private const long MaxFileUploadSizeLimit = 50 * 1024 * 1024;
    private static readonly System.Collections.Generic.HashSet<string> AllowedFileTypes = new()
    {
        "jpg", "png", "webp", "jpeg"
    };

    public LocalFileSystemUploadService(IWebHostEnvironment environment)
        => _fileStorageBaseFolder = Path.Combine(environment.ContentRootPath, "Files");

    public async Task<Either<FileUploadResult, Error>> UploadAsync(
        FileUploadRequest fileUploadRequest,
        CancellationToken cancellationToken = default)
    {
        if (fileUploadRequest.SizeInBytes >= MaxFileUploadSizeLimit) return Error.New("File exceeds size limit of 50 MB.");
        
        var fileExtension = Path.GetExtension(fileUploadRequest.FileName)[1..];
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileUploadRequest.FileName);
        
        if (!AllowedFileTypes.Contains(fileExtension))
        {
            return Error.New($"Files with extension {fileExtension} are not allowed.");
        }

        var newFileId = Guid.NewGuid();
        var newFileName = fileUploadRequest.UserId.HasValue
            ? $"{fileUploadRequest.UserId}_{newFileId}_{fileNameWithoutExtension}.{fileExtension}"
            : $"{newFileId}_{fileNameWithoutExtension}.{fileExtension}";

        if (!Directory.Exists(_fileStorageBaseFolder)) Directory.CreateDirectory(_fileStorageBaseFolder);
        
        await using var fileStream = File.Open(Path.Combine(_fileStorageBaseFolder, newFileName), FileMode.CreateNew, FileAccess.Write);
        await fileUploadRequest.Data.CopyToAsync(fileStream, cancellationToken);
        
        return new FileUploadResult
        {
            FileId = newFileId,
            FileUrl = $"/Files/{newFileName}"
        };
    }
}
