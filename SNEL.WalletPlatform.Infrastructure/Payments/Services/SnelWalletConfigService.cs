using SNEL.WalletPlatform.Application.Payments.Models.Internal;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Services;

public sealed class SnelWalletConfigService : PspConfigServiceBase
{
    public SnelWalletConfigModel GetConfig(string config) =>
        DeserializeConfig<SnelWalletConfigModel>(config);

    public string GetConfirmationMessageTemplate(string config, string? languageCode)
    {
        const string fallbackMessage = "Please confirm this payment of '{0}' to continue.";

        var configModel = GetConfig(config);
        var messages = configModel.Confirmation?.Message;
        if (messages is null)
        {
            return fallbackMessage;
        }

        if (TryGetLocalizedMessage(messages, languageCode, out var localizedMessage))
        {
            return localizedMessage;
        }
        
        return fallbackMessage;
    }

    private static bool TryGetLocalizedMessage(
        SnelWalletConfirmationMessageConfigModel messageConfig,
        string? languageCode,
        out string message)
    {
        message = languageCode switch
        {
            "fr" => messageConfig.Fr ?? string.Empty,
            "en" => messageConfig.En ?? string.Empty,
            _ => messageConfig.En ?? string.Empty,
        };

        return !string.IsNullOrWhiteSpace(message);
    }
}
