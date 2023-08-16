using System.Text.Json.Serialization;
using Chatify.Application.Common.Mappings;

namespace Chatify.Infrastructure.Authentication.External.Github;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class GithubUserInfo : IMapFrom<Octokit.User>
{
    [JsonPropertyName("updatedAt")] public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("avatarUrl")] public string AvatarUrl { get; set; }

    [JsonPropertyName("company")] public string Company { get; set; }

    [JsonPropertyName("createdAt")] public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("email")] public string Email { get; set; }

    [JsonPropertyName("htmlUrl")] public string HtmlUrl { get; set; }

    [JsonPropertyName("id")] public int? Id { get; set; }

    [JsonPropertyName("nodeId")] public string NodeId { get; set; }

    [JsonPropertyName("location")] public string Location { get; set; }

    [JsonPropertyName("login")] public string Login { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }
}