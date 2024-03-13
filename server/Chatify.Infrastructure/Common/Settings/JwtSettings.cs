using System.Text;

namespace Chatify.Infrastructure.Common.Settings;

public class JwtSettings
{
    public string Audience { get; set; } = default!;

    public string Issuer { get; set; } = default!;

    public string Key { get; set; } = default!;

    public byte[] KeyBytes => Encoding.UTF8.GetBytes(Key);
}