using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Jobs.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

public sealed class ManualJobTriggerService : IManualJobTriggerService
{
    private const string BillerRolloverJobName = "BillerRolloverJob";
    private const string JobName = "SyncGcoEnCoursFacturesJob";
    private const string SyncBillerCustomersJobName = "SyncBillerCustomersJob";
    private const string ExpireStalePendingPaymentsJobName = "ExpireStalePendingPaymentsJob";
    private const string SendNewInvoiceNotificationsJobName = "SendNewInvoiceNotificationsJob";
    private const string SendInvoiceDueReminderJobName = "SendInvoiceDueReminderJob";
    private const string JobGroup = "default";
    private const string ConnectAfricaEosJobName = "ConnectAfricaEosJob";
    private const string SyncInPendingPaymentsStatusJobName = "SyncInPendingPaymentsStatusJob";

    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<ManualJobTriggerService> _logger;

    public ManualJobTriggerService(ISchedulerFactory schedulerFactory, ILogger<ManualJobTriggerService> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<bool> TriggerSyncGcoEnCoursFacturesAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(JobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", JobName);
            return false;
        }

        var jobData = new JobDataMap
        {
            { "StartTs", start.ToString("O") },
            { "EndTs", end.ToString("O") }
        };
        var triggers = scheduler.GetTriggersOfJob(jobKey, cancellationToken).Result.ToList();
        triggers.ForEach(t => scheduler.UnscheduleJob(t.Key));
        await scheduler.TriggerJob(jobKey, jobData, cancellationToken);
        _logger.LogInformation("Manually triggered job {JobName} with Start={Start:O}, End={End:O}.", JobName, start, end);
        return true;
    }

    public async Task<bool> TriggerSyncBillerCustomersAsync(CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(SyncBillerCustomersJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", SyncBillerCustomersJobName);
            return false;
        }

        var triggers = (await scheduler.GetTriggersOfJob(jobKey, cancellationToken)).ToList();
        foreach (var t in triggers)
            await scheduler.UnscheduleJob(t.Key, cancellationToken);

        await scheduler.TriggerJob(jobKey, cancellationToken);
        _logger.LogInformation("Manually triggered job {JobName}.", SyncBillerCustomersJobName);
        return true;
    }

    public async Task<bool> TriggerExpireStalePendingPaymentsAsync(
        int? pageSize = null,
        int? staleAfterMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(ExpireStalePendingPaymentsJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", ExpireStalePendingPaymentsJobName);
            return false;
        }

        var triggers = (await scheduler.GetTriggersOfJob(jobKey, cancellationToken)).ToList();
        foreach (var t in triggers)
            await scheduler.UnscheduleJob(t.Key, cancellationToken);

        JobDataMap? jobData = null;
        if (pageSize is > 0 || staleAfterMinutes is > 0)
            jobData = ExpireStalePendingPaymentsJobData.CreateMapForManualTrigger(pageSize, staleAfterMinutes);

        if (jobData is null)
            await scheduler.TriggerJob(jobKey, cancellationToken);
        else
            await scheduler.TriggerJob(jobKey, jobData, cancellationToken);

        _logger.LogInformation(
            "Manually triggered job {JobName} (PageSize={PageSize}, StaleAfterMinutes={StaleAfterMinutes}).",
            ExpireStalePendingPaymentsJobName,
            pageSize,
            staleAfterMinutes);
        return true;
    }

    public async Task<bool> TriggerSendNewInvoiceNotificationsAsync(
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(SendNewInvoiceNotificationsJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", SendNewInvoiceNotificationsJobName);
            return false;
        }

        var triggers = (await scheduler.GetTriggersOfJob(jobKey, cancellationToken)).ToList();
        foreach (var t in triggers)
            await scheduler.UnscheduleJob(t.Key, cancellationToken);

        JobDataMap? jobData = null;
        if (pageSize is > 0)
            jobData = SendNewInvoiceNotificationsJobData.CreateMapForManualTrigger(pageSize);

        if (jobData is null)
            await scheduler.TriggerJob(jobKey, cancellationToken);
        else
            await scheduler.TriggerJob(jobKey, jobData, cancellationToken);

        _logger.LogInformation(
            "Manually triggered job {JobName} (PageSize={PageSize}).",
            SendNewInvoiceNotificationsJobName,
            pageSize);
        return true;
    }

    public async Task<bool> TriggerSendInvoiceDueReminderAsync(
        int? pageSize = null,
        int? daysBefore = null,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(SendInvoiceDueReminderJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", SendInvoiceDueReminderJobName);
            return false;
        }

        var triggers = (await scheduler.GetTriggersOfJob(jobKey, cancellationToken)).ToList();
        foreach (var t in triggers)
            await scheduler.UnscheduleJob(t.Key, cancellationToken);

        JobDataMap? jobData = null;
        if (pageSize is > 0 || daysBefore is > 0)
            jobData = SendInvoiceDueReminderJobData.CreateMapForManualTrigger(pageSize, daysBefore);

        if (jobData is null)
            await scheduler.TriggerJob(jobKey, cancellationToken);
        else
            await scheduler.TriggerJob(jobKey, jobData, cancellationToken);

        _logger.LogInformation(
            "Manually triggered job {JobName} (PageSize={PageSize}, DaysBefore={DaysBefore}).",
            SendInvoiceDueReminderJobName,
            pageSize,
            daysBefore);
        return true;
    }

    public async Task<bool> TriggerBillerRolloverAsync(
        DateTime start,
        DateTime end,
        string regions,
        bool registerNotificationOnPull,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(BillerRolloverJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning(
                "Manual trigger requested for {JobName}, but job is not registered.",
                BillerRolloverJobName);
            return false;
        }

        var jobData = new JobDataMap
        {
            { "StartTs", start.ToString("O") },
            { "EndTs", end.ToString("O") },
            { "Regions", regions },
            { "RegisterNotificationOnPull", registerNotificationOnPull ? "true" : "false" }
        };
        var triggers = scheduler.GetTriggersOfJob(jobKey, cancellationToken).Result.ToList();
        triggers.ForEach(t => scheduler.UnscheduleJob(t.Key));
        await scheduler.TriggerJob(jobKey, jobData, cancellationToken);
        _logger.LogInformation(
            "Manually triggered job {JobName} with Start={Start:O}, End={End:O}, Regions={Regions}, RegisterNotificationOnPull={RegisterNotificationOnPull}.",
            BillerRolloverJobName,
            start,
            end,
            regions,
            registerNotificationOnPull);
        return true;
    }

    public async Task<bool> TriggerConnectAfricaEOS(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(ConnectAfricaEosJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", JobName);
            return false;
        }

        var jobData = new JobDataMap
        {
            { "StartTs", start.ToString("O") },
            { "EndTs", end.ToString("O") }
        };
        var triggers = scheduler.GetTriggersOfJob(jobKey, cancellationToken).Result.ToList();
        triggers.ForEach(t => scheduler.UnscheduleJob(t.Key));
        await scheduler.TriggerJob(jobKey, jobData, cancellationToken);
        _logger.LogInformation("Manually triggered job {JobName} with Start={Start:O}, End={End:O}.", JobName, start, end);
        return true;
    }

    public async Task<bool> TriggerSyncInPendingPaymentsStatusJobAsync(
        int? pageSize = null,
        int? minimumAgeMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(SyncInPendingPaymentsStatusJobName, JobGroup);
        if (!await scheduler.CheckExists(jobKey, cancellationToken))
        {
            _logger.LogWarning("Manual trigger requested for {JobName}, but job is not registered.", SyncInPendingPaymentsStatusJobName);
            return false;
        }

        var triggers = (await scheduler.GetTriggersOfJob(jobKey, cancellationToken)).ToList();
        foreach (var t in triggers)
            await scheduler.UnscheduleJob(t.Key, cancellationToken);

        JobDataMap? jobData = null;
        if (pageSize is > 0 || minimumAgeMinutes is > 0)
            jobData = SyncInPendingPaymentsStatusJobData.CreateMapForManualTrigger(pageSize, minimumAgeMinutes);

        if (jobData is null)
            await scheduler.TriggerJob(jobKey, cancellationToken);
        else
            await scheduler.TriggerJob(jobKey, jobData, cancellationToken);

        _logger.LogInformation(
            "Manually triggered job {JobName} (PageSize={PageSize}, MinimumAgeMinutes={MinimumAgeMinutes}).",
            SyncInPendingPaymentsStatusJobName,
            pageSize,
            minimumAgeMinutes);
        return true;
    }
}
