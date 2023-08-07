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

    public async Task<Either<Error, FileUploadResult>> UploadAsync(
        SingleFileUploadRequest singleFileUploadRequest,
        CancellationToken cancellationToken = default)
    {
        var file = singleFileUploadRequest.File;

        if ( file.SizeInBytes >= MaxFileUploadSizeLimit ) return Error.New("File exceeds size limit of 50 MB.");

        var fileExtension = Path.GetExtension(file.FileName)[1..];
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

        if ( !AllowedFileTypes.Contains(fileExtension) )
        {
            return Error.New($"Files with extension {fileExtension} are not allowed.");
        }

        var newFileId = Guid.NewGuid();
        var newFileName = singleFileUploadRequest.UserId.HasValue
            ? $"{singleFileUploadRequest.UserId}_{newFileId}_{fileNameWithoutExtension}.{fileExtension}"
            : $"{newFileId}_{fileNameWithoutExtension}.{fileExtension}";

        if ( !Directory.Exists(_fileStorageBaseFolder) ) Directory.CreateDirectory(_fileStorageBaseFolder);

        await using var fileStream = File.Open(Path.Combine(_fileStorageBaseFolder, newFileName), FileMode.CreateNew,
            FileAccess.Write);
        await file.Data.CopyToAsync(fileStream, cancellationToken);

        return new FileUploadResult
        {
            FileId = newFileId,
            FileUrl = newFileName
        };
    }

    public async Task<Either<Error, bool>> DeleteAsync(
        SingleFileDeleteRequest singleFileDeleteRequest,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_fileStorageBaseFolder, singleFileDeleteRequest.FileUrl);
        if ( !File.Exists(filePath) ) return Error.New("Specified file not found.");

        File.Delete(filePath);
        return true;
    }

    public async Task<List<Either<Error, FileUploadResult>>> UploadManyAsync(
        MultipleFileUploadRequest multipleFileUploadRequest,
        CancellationToken cancellationToken = default)
    {
        var uploadTasks = multipleFileUploadRequest
            .Files
            .Select(f => UploadAsync(new SingleFileUploadRequest
            {
                File = f,
                UserId = multipleFileUploadRequest.UserId
            }, cancellationToken)).ToList();

        var uploadResults = await Task.WhenAll(uploadTasks);
        return uploadResults.ToList();
    }
}