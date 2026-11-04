using System.Text.Json;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Services;

public abstract class PspConfigServiceBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected TConfig DeserializeConfig<TConfig>(string rawConfig)
    {
        if (string.IsNullOrWhiteSpace(rawConfig))
        {
            throw new InvalidOperationException("Payment method config is required.");
        }

        var deserializeConfig = JsonSerializer.Deserialize<TConfig>(rawConfig, SerializerOptions);
        if (deserializeConfig is null)
        {
            throw new InvalidOperationException("Payment method config is invalid.");
        }

        return deserializeConfig;
    }
}
