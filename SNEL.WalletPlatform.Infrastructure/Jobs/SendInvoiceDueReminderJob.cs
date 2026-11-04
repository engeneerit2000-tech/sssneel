using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Sends a payment reminder to customers whose invoices are due within <c>DaysBefore</c> days.
/// Joins <c>Invoices</c> with <c>CustomerSubscriptions</c> via BillingRef, then to <c>Customers</c>
/// to resolve users and schedule a push notification using <see cref="NotificationService"/>.
/// </summary>
[DisallowConcurrentExecution]
public sealed class SendInvoiceDueReminderJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SendInvoiceDueReminderJob> logger) : IJob
{
    private const int DefaultPageSize = 500;
    private const int DefaultDaysBefore = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var ct = context.CancellationToken;

        var pageSize = SendInvoiceDueReminderJobData.ReadPageSize(context.MergedJobDataMap, DefaultPageSize);
        var daysBefore = SendInvoiceDueReminderJobData.ReadDaysBefore(context.MergedJobDataMap, DefaultDaysBefore);

        logger.LogInformation(
            "SendInvoiceDueReminderJob started (pageSize={PageSize}, daysBefore={DaysBefore}).",
            pageSize, daysBefore);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var nowUtc = DateTimeOffset.UtcNow;
            var windowEnd = nowUtc.AddDays(daysBefore);

            var issuedStatus = InvoiceStatus.Issued.ToString();
            var partiallyPaidStatus = InvoiceStatus.PartiallyPaid.ToString();

            var dueInvoices = await (from invoice in db.Invoices.AsNoTracking()
                                     join subscription in db.CustomerSubscriptions.AsNoTracking()
                                         on invoice.BillingRef.ToLower() equals subscription.BillingRefValue.ToLower()
                                     join customer in db.Customers.AsNoTracking()
                                         on subscription.CustomerId equals customer.Id
                                     where invoice.DueDate != null
                                           && invoice.DueDate > nowUtc
                                           && invoice.DueDate <= windowEnd
                                           && (invoice.Status == issuedStatus || invoice.Status == partiallyPaidStatus)
                                           && invoice.AmountOutstanding > 0
                                     select new
                                     {
                                         InvoiceId = invoice.Id,
                                         invoice.InvoiceRef,
                                         invoice.AmountOutstanding,
                                         invoice.DueDate,
                                         SubscriptionId = subscription.Id,
                                         customer.PartnerId
                                     })
                .Take(pageSize)
                .ToListAsync(ct);

            if (dueInvoices.Count == 0)
            {
                logger.LogInformation("SendInvoiceDueReminderJob: no invoices due in the next {DaysBefore} day(s).", daysBefore);
                return;
            }

            var requestedChannels = new List<NotificationChannel> { NotificationChannel.Push };
            var enabledChannels = notificationService.GetTargettedChannels(requestedChannels);
            if (enabledChannels.Count == 0)
            {
                logger.LogInformation("SendInvoiceDueReminderJob skipped: no enabled channels for push notifications.");
                return;
            }

            var partnerIds = dueInvoices.Select(x => x.PartnerId).Distinct().ToList();
            var users = await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync(partnerIds);
            var usersByPartnerId = users
                .Where(u => u.PartnerId.HasValue)
                .GroupBy(u => u.PartnerId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var sentCount = 0;
            var skippedNoUser = 0;
            var failed = 0;
            foreach (var invoice in dueInvoices)
            {
                if (!usersByPartnerId.TryGetValue(invoice.PartnerId, out var user))
                {
                    skippedNoUser++;
                    continue;
                }

                var amount = invoice.AmountOutstanding.ToString("0.##");

                var model = new NotificationDetailsModel
                {
                    IsTransactional = false,
                    NotificationCode = NotificationCodeEnum.PAYMENT_REMINDER,
                    NotificationType = NotificationTypeEnum.BILL,
                    Params =
                    [
                        new ParameterDataModel
                        {
                            Language = LanguageEnum.EN,
                            Params = [amount]
                        },
                        new ParameterDataModel
                        {
                            Language = LanguageEnum.FR,
                            Params = [amount]
                        }
                    ],
                    SenderId = null,
                    ReferenceType = ReferenceTypeEnum.SUBSCRIPTION,
                    ReferenceId = invoice.SubscriptionId.ToString(),
                    OverrideReceiverChannelsPreferences = false,
                    RequestedChannels = enabledChannels,
                    Receivers = [user]
                };

                var sent = await notificationService.SendNotificationsAsync([model]);
                if (sent)
                    sentCount++;
                else
                    failed++;
            }

            sw.Stop();
            logger.LogInformation(
                "SendInvoiceDueReminderJob finished in {ElapsedMs} ms. Total={Total}, sent={Sent}, skippedNoUser={SkippedNoUser}, failed={Failed}.",
                sw.ElapsedMilliseconds, dueInvoices.Count, sentCount, skippedNoUser, failed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "SendInvoiceDueReminderJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
