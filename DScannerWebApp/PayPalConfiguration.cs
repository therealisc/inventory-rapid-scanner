using PayPal.Api;

namespace RCommerce.WebApp;

public static class PayPalConfiguration
{
    public static Dictionary<string, string> GetConfig(IConfiguration configuration)
    {
        return new Dictionary<string, string>
        {
            { "mode", configuration["PayPal:Mode"] },
            { "Key", configuration["PayPal:Key"] },
            { "Secret", configuration["PayPal:Secret"] }
        };
    }

    public static APIContext GetAPIContext(IConfiguration configuration)
    {
        var config = GetConfig(configuration);
        var accessToken = GetAccessToken(config);
        return new APIContext(accessToken) { Config = config };
    }

    private static string GetAccessToken(Dictionary<string, string> config)
    {
        return new OAuthTokenCredential(
            config["Key"],
            config["Secret"],
            config
        ).GetAccessToken();
    }
}
