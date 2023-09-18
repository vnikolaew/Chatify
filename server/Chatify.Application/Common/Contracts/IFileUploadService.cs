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

public static class FolderConstants
{
    public static class Users
    {
        public const string Root = nameof(Users);

        public static class ProfilePictures
        {
            public static readonly string Root = Path.Combine(Users.Root, nameof(ProfilePictures));
        }
    }

    public static class ChatGroups
    {
        public const string Root = nameof(ChatGroups);

        public static class Media
        {
            public static readonly string Root = Path.Combine(ChatGroups.Root, nameof(Media));
        }
    }
}

public static class Extensions
{
    public static Task<OneOf<Error, FileUploadResult>> UploadUserProfilePictureAsync(
        this IFileUploadService fileUploadService,
        SingleFileUploadRequest request,
        CancellationToken cancellationToken = default)
        => fileUploadService.UploadAsync(new SingleFileUploadRequest
        {
            File = request.File,
            UserId = request.UserId,
            Location = FolderConstants.Users.ProfilePictures.Root
        }, cancellationToken);

    public static Task<List<OneOf<Error, FileUploadResult>>> UploadManyUserProfilePicturesAsync(
        this IFileUploadService fileUploadService,
        MultipleFileUploadRequest request,
        CancellationToken cancellationToken = default)
        => fileUploadService.UploadManyAsync(new MultipleFileUploadRequest
        {
            Files = request.Files,
            UserId = request.UserId,
            Location = FolderConstants.Users.ProfilePictures.Root
        }, cancellationToken);

    public static Task<OneOf<Error, FileUploadResult>> UploadChatGroupMediaAsync(
        this IFileUploadService fileUploadService,
        SingleFileUploadRequest request,
        CancellationToken cancellationToken = default)
        => fileUploadService.UploadAsync(new SingleFileUploadRequest
        {
            File = request.File,
            UserId = request.UserId,
            Location = FolderConstants.ChatGroups.Media.Root
        }, cancellationToken);

    public static Task<List<OneOf<Error, FileUploadResult>>> UploadManyChatGroupMediaAsync(
        this IFileUploadService fileUploadService,
        MultipleFileUploadRequest request,
        CancellationToken cancellationToken = default)
        => fileUploadService.UploadManyAsync(new MultipleFileUploadRequest
        {
            Files = request.Files,
            UserId = request.UserId,
            Location = FolderConstants.ChatGroups.Media.Root
        }, cancellationToken);
}

public class SingleFileDeleteRequest
{
    public string FileUrl { get; set; } = default!;

    public Guid? UserId { get; set; }
}

public class SingleFileUploadRequest
{
    public InputFile File { get; set; } = default!;

    public string? Location { get; set; } = default!;

    public Guid? UserId { get; set; }
}

public class MultipleFileUploadRequest
{
    public IEnumerable<InputFile> Files { get; set; } = new List<InputFile>();

    public string? Location { get; set; } = default!;

    public Guid? UserId { get; set; }
}

public class FileUploadResult
{
    public Guid FileId { get; set; }

    public string FileName { get; set; } = default!;

    public string FileType { get; set; } = default!;

    public string FileUrl { get; set; } = default!;
}