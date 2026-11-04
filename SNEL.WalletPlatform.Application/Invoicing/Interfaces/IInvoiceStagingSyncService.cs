namespace SNEL.WalletPlatform.Application.Invoicing.Interfaces;

/// <summary>
/// Projects Oracle invoice staging rows into the second staging table (invoice-shaped payloads with <c>JobKey</c> and <c>Updated</c>),
/// then applies them to <c>Invoices</c> (SQL on PostgreSQL; EF apply for InMemory).
/// </summary>
public interface IInvoiceStagingSyncService
{
    /// <summary>
    /// For each staging row: if no matching invoice exists, insert a secondary row with <c>Updated = false</c>;
    /// if a match exists, insert with <c>Updated = true</c> only when staging effective invoice update date is greater than the invoice business <c>InvoiceUpdateDate</c>;
    /// otherwise skip. Clears processed rows from <c>InvoiceStagings</c> when done.
    /// </summary>
    Task SyncInvoicesFromStagingAsync(
        string jobKey,
        string sourceSystem,
        bool registerNotificationOnPull = false,
        TimeSpan? notificationTriggerTime = null);
}
