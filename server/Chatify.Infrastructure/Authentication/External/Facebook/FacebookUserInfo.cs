using System.Text.Json.Serialization;

namespace Chatify.Infrastructure.Authentication.External.Facebook;

public class FacebookUserInfo
{
    [JsonPropertyName("id")] public string Id { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("picture")] public Picture Picture { get; set; }

    [JsonPropertyName("first_name")] public string FirstName { get; set; }

    [JsonPropertyName("last_name")] public string LastName { get; set; }
}

public class PictureData
{
    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("is_silhouette")] public bool IsSilhouette { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("width")] public int Width { get; set; }
}

public class Picture
{
    [JsonPropertyName("data")] public PictureData Data { get; set; }
}