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
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class SendNewInvoiceNotificationsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SendNewInvoiceNotificationsJob> logger) : IJob
{
    private const int DefaultPageSize = 500;

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var ct = context.CancellationToken;
        var pageSize = SendNewInvoiceNotificationsJobData.ReadPageSize(
            context.MergedJobDataMap,
            DefaultPageSize);

        logger.LogInformation("SendNewInvoiceNotificationsJob started (pageSize={PageSize}).", pageSize);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var deletedLegacy = await db.NewInvoiceNotifications
                .Where(n => n.Processed)
                .ExecuteDeleteAsync(ct);
            if (deletedLegacy > 0)
            {
                logger.LogInformation(
                    "SendNewInvoiceNotificationsJob removed {DeletedLegacy} legacy processed notification row(s).",
                    deletedLegacy);
            }

            var now = DateTimeOffset.UtcNow;
            var dueNotifications = await db.NewInvoiceNotifications
                .Include(n => n.Invoice)
                .Where(n => !n.Processed && n.NotificationTriggerTime <= now.TimeOfDay)
                .OrderBy(n => n.CreatedAtUtc)
                .Take(pageSize)
                .ToListAsync(ct);

            if (dueNotifications.Count == 0)
            {
                logger.LogInformation("SendNewInvoiceNotificationsJob: no due notifications found.");
                return;
            }

            var normalizedBillingRefs = dueNotifications
                .Select(n => Normalize(n.Invoice.BillingRef))
                .Distinct()
                .ToList();

            var subscriptions = await (from subscription in db.CustomerSubscriptions.AsNoTracking()
                                       join customer in db.Customers.AsNoTracking()
                                           on subscription.CustomerId equals customer.Id
                                       let normalizedBillingRef = subscription.BillingRefValue.ToLower()
                                       where normalizedBillingRefs.Contains(normalizedBillingRef)
                                       select new
                                       {
                                           SubscriptionId = subscription.Id,
                                           BillingRef = normalizedBillingRef,
                                           customer.PartnerId
                                       })
                .ToListAsync(ct);

            var subscriptionsByBillingRef = subscriptions
                .GroupBy(x => x.BillingRef)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => (x.PartnerId, x.SubscriptionId)).Distinct().ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var requestedChannels = new List<NotificationChannel> { NotificationChannel.Push };
            var enabledChannels = notificationService.GetTargettedChannels(requestedChannels);
            if (enabledChannels.Count == 0)
            {
                logger.LogInformation("SendNewInvoiceNotificationsJob skipped: no enabled channels for push notifications.");
                return;
            }

            var sent = 0;
            var skippedNoReceivers = 0;
            var failed = 0;
            foreach (var notification in dueNotifications)
            {
                var normalizedBillingRef = Normalize(notification.Invoice.BillingRef);
                if (!subscriptionsByBillingRef.TryGetValue(normalizedBillingRef, out var subscriptionEntries)
                    || subscriptionEntries.Count == 0)
                {
                    db.NewInvoiceNotifications.Remove(notification);
                    skippedNoReceivers++;
                    continue;
                }

                var partnerIds = subscriptionEntries.Select(x => x.PartnerId).Distinct().ToList();
                var users = await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync(partnerIds);
                var usersByPartnerId = users
                    .Where(u => u.PartnerId.HasValue)
                    .GroupBy(u => u.PartnerId!.Value)
                    .ToDictionary(g => g.Key, g => g.First());

                var anyReceiverFound = false;
                var allSendsSucceeded = true;
                foreach (var (partnerId, subscriptionId) in subscriptionEntries)
                {
                    if (!usersByPartnerId.TryGetValue(partnerId, out var user))
                        continue;

                    anyReceiverFound = true;
                    var model = new NotificationDetailsModel
                    {
                        IsTransactional = false,
                        NotificationCode = NotificationCodeEnum.NEW_BILL_GENERATED,
                        NotificationType = NotificationTypeEnum.BILL,
                        Params =
                        [
                            new ParameterDataModel
                            {
                                Language = LanguageEnum.EN,
                                Params =
                                [
                                    notification.Invoice.AmountOutstanding.ToString("0.##")
                                ]
                            },
                            new ParameterDataModel
                            {
                                Language = LanguageEnum.FR,
                                Params =
                                [
                                    notification.Invoice.AmountOutstanding.ToString("0.##")
                                ]
                            }
                        ],
                        SenderId = null,
                        ReferenceType = ReferenceTypeEnum.SUBSCRIPTION,
                        ReferenceId = subscriptionId.ToString(),
                        OverrideReceiverChannelsPreferences = false,
                        RequestedChannels = enabledChannels,
                        Receivers = [user],
                        SendingAt = BuildSendingAtUtc(now, notification.NotificationTriggerTime)
                    };

                    var notificationSent = await notificationService.SendNotificationsAsync([model]);
                    if (notificationSent)
                        sent++;
                    else
                        allSendsSucceeded = false;
                }

                if (!anyReceiverFound)
                {
                    db.NewInvoiceNotifications.Remove(notification);
                    skippedNoReceivers++;
                    continue;
                }

                if (allSendsSucceeded)
                    db.NewInvoiceNotifications.Remove(notification);
                else
                    failed++;
            }

            await db.SaveChangesAsync(ct);

            sw.Stop();
            logger.LogInformation(
                "SendNewInvoiceNotificationsJob finished in {ElapsedMs} ms. Total={Total}, sent={Sent}, skippedNoReceivers={SkippedNoReceivers}, failed={Failed}.",
                sw.ElapsedMilliseconds,
                dueNotifications.Count,
                sent,
                skippedNoReceivers,
                failed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "SendNewInvoiceNotificationsJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static DateTime BuildSendingAtUtc(DateTimeOffset nowUtc, TimeSpan triggerTime)
    {
        var candidate = nowUtc.Date.Add(triggerTime);
        return candidate > nowUtc.UtcDateTime ? candidate : nowUtc.UtcDateTime;
    }
}
