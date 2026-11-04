using SNEL.WalletPlatform.Application.Payments.Models.Internal;

namespace SNEL.WalletPlatform.Application.Payments;

/// <summary>
/// Defers treating PSP "transaction not found" (HTTP 404 with body) as a final payment failure
/// until enough status polling attempts have elapsed.
/// </summary>
public static class PspTransactionStatusNotFoundPolicy
{
    public static bool ShouldMarkFailed(PaymentTransactionStatusContextInternalModel context)
    {
        if (!context.StatusPollingAttempt.HasValue)
        {
            return false;
        }

        var attempt = context.StatusPollingAttempt.Value;
        var totalAttempts = context.StatusPollingTotalAttempts ?? attempt;
        var failAfterAttempts = context.NotFoundFailAfterAttempts
            ?? PaymentMethodConfigReader.DefaultNotFoundFailAfterAttempts;

        return attempt >= failAfterAttempts || attempt >= totalAttempts;
    }
}
