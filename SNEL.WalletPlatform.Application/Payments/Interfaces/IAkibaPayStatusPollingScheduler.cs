namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Schedules AkibaPay management-API status polling as a webhook fallback (not Quartz).
/// </summary>
public interface IAkibaPayStatusPollingScheduler
{
    void Schedule(Guid paymentId);

    void Cancel(Guid paymentId);
}
