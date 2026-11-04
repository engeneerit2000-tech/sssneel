namespace SNEL.WalletPlatform.Application.Jobs.Interfaces;

/// <summary>
/// Triggers background jobs manually (e.g. from an admin API).
/// </summary>
public interface IManualJobTriggerService
{
    /// <summary>
    /// Triggers the Sync GCO En Cours Factures job (Oracle pull-invoices → staging → sync) to run once immediately
    /// for the given date range. Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerSyncGcoEnCoursFacturesAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the Sync Biller Customers job (Oracle raccordements → staging → merge) once immediately. No date range.
    /// Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerSyncBillerCustomersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the Expire Stale Pending Payments job once immediately.
    /// Optional <paramref name="pageSize"/> and <paramref name="staleAfterMinutes"/> override job defaults for this run.
    /// Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerExpireStalePendingPaymentsAsync(
        int? pageSize = null,
        int? staleAfterMinutes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the Send New Invoice Notifications job once immediately.
    /// Optional <paramref name="pageSize"/> overrides job default for this run.
    /// Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerSendNewInvoiceNotificationsAsync(
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the Send Invoice Due Reminder job once immediately.
    /// Sends a payment reminder for invoices whose DueDate is within the next
    /// <paramref name="daysBefore"/> days. Optional parameters override job defaults for this run.
    /// Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerSendInvoiceDueReminderAsync(
        int? pageSize = null,
        int? daysBefore = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers the biller rollover job (apply-payment → open journal → Oracle pull-customer → Oracle pull-invoices → staging → sync) to run once immediately
    /// for the given date range. Returns true if the job was scheduled; false if the job is not registered.
    /// </summary>
    Task<bool> TriggerBillerRolloverAsync(
        DateTime start,
        DateTime end,
        string regions,
        bool registerNotificationOnPull,
        CancellationToken cancellationToken = default);

    Task<bool> TriggerConnectAfricaEOS(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<bool> TriggerSyncInPendingPaymentsStatusJobAsync(int? pageSize = null, int? staleAfterMinutes = null, CancellationToken cancellationToken = default);
}
