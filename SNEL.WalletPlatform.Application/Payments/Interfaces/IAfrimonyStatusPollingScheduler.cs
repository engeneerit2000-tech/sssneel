namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Schedules Afrimony TransactionEnquiry status polling as a webhook fallback (not Quartz).
/// </summary>
public interface IAfrimonyStatusPollingScheduler
{
    void Schedule(Guid paymentId);

    void Cancel(Guid paymentId);
}
