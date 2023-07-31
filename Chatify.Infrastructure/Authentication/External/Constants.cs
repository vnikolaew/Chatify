namespace Chatify.Infrastructure.Authentication.External;

public static class Constants
{
    public static class ClaimNames
    {
        public static readonly string Picture = nameof(Picture).ToLower();
        public static readonly string Locale = nameof(Locale).ToLower();
    }
    
    public static class AuthProviders
    {
        public static readonly string Google = nameof(Google).ToLower();
        public static readonly string Facebook = nameof(Facebook).ToLower();
        public static readonly string RegularLogin = nameof(RegularLogin).ToLower();
    }
}