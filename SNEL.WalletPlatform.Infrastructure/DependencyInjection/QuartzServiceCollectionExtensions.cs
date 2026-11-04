using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Quartz;
using Quartz.Impl.Matchers;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Infrastructure.Configuration;

namespace SNEL.WalletPlatform.Infrastructure.DependencyInjection;

public static class QuartzServiceCollectionExtensions
{
    /// <summary>
    /// Base namespace for job types (Jobs live in Infrastructure.Jobs).
    /// </summary>
    private const string JobNamespace = "SNEL.WalletPlatform.Infrastructure.Jobs";
    private static readonly JobKey SnelBootstrapJobKey = new("SnelBootstrapJob", "startup");
    private static readonly TriggerKey SnelBootstrapTriggerKey = new("SnelBootstrapJob-trigger", "startup");

    private static readonly Assembly JobAssembly = typeof(QuartzServiceCollectionExtensions).Assembly;

    /// <summary>
    /// Registers Quartz and configures jobs from the "QuartzJobs" config section (RERO-style pattern).
    /// </summary>
    public static IServiceCollection RegisterQuartzServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<QuartzOptions>(opt =>
        {
            opt.Scheduling.OverWriteExistingData = true;
            opt.Scheduling.IgnoreDuplicates = true;
        });
        var jobConfigs = configuration
            .GetSection(QuartzJobConfig.SectionName)
            .Get<List<QuartzJobConfig>>() ?? new List<QuartzJobConfig>();
        var quartzConnectionString = configuration.GetConnectionString("DefaultConnection");
        // var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
        // var triggers = scheduler.GetTriggersOfJob(jobKey, cancellationToken).Result.ToList();
        // triggers.ForEach(t => scheduler.UnscheduleJob(t.Key));

        var desiredTriggerKeys = jobConfigs
            .Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Enabled)
            .Select(x => x.TriggerKey)
            .ToHashSet();
        desiredTriggerKeys.Add(SnelBootstrapTriggerKey);
        services.AddQuartz(q =>
        {
            q.SchedulerName = "Wallet-API-Scheduler";
            q.SchedulerId = "AUTO";

            q.UsePersistentStore(store =>
            {
                store.UseProperties = true;

                store.UsePostgres(postgres => { postgres.ConnectionString = quartzConnectionString; });

                store.UseClustering();

                // pick one serializer package you actually use
                store.UseSystemTextJsonSerializer();
            });

            services.AddTransient<Jobs.SnelBootstrapJob>();
            q.AddJob<Jobs.SnelBootstrapJob>(SnelBootstrapJobKey, j =>
            {
                j.StoreDurably(false);
                j.WithDescription("One-time startup bootstrap for SNEL runtime config.");
            });
            q.AddTrigger(t => t
                .WithIdentity(SnelBootstrapTriggerKey)
                .ForJob(SnelBootstrapJobKey)
                .StartNow()
                .WithSimpleSchedule(x => x.WithRepeatCount(0)));
            
            
            services.AddTransient<Jobs.PollPaymentStatusJob>();
            q.AddJob<Jobs.PollPaymentStatusJob>(PaymentStatusPollingJobConstants.JobKey, j =>
            {
                j.StoreDurably();
                j.WithDescription("One-off polling job for payment status updates.");
            });

            services.AddTransient<Jobs.ApplyPOIsAfterPayJob>();
            q.AddJob<Jobs.ApplyPOIsAfterPayJob>(ApplyPOIsAfterPayJobConstants.JobKey, j =>
            {
                j.StoreDurably();
                j.WithDescription("One-off job for applying POIs after payment.");
            });

            services.AddTransient<Jobs.SendCampaignNotificationsJob>();
            q.AddJob<Jobs.SendCampaignNotificationsJob>(SendCampaignNotificationsJobConstants.JobKey, j =>
            {
                j.StoreDurably();
                j.WithDescription("One-off batch job for campaign push notifications.");
            });

            foreach (var jobConfig in jobConfigs.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                var jobType = ResolveJobType(jobConfig.Name);
                if (jobType == null)
                    continue;

                services.AddTransient(jobType); // so job can be resolved for direct run (e.g. manual trigger)

                q.AddJob(jobType, jobConfig.JobKey, j =>
                {
                    j.StoreDurably(); // Required when job has no trigger (e.g. Enabled=false) so it can be triggered manually
                    j.WithDescription($"Scheduled: {jobConfig.Schedule}");
                    if (jobConfig.Options != null)
                    {
                        foreach (var kv in jobConfig.Options)
                        {
                            var v = kv.Value;
                            if (v == null) continue;
                            if (v is string s) j.UsingJobData(kv.Key, s);
                            else if (v is int i) j.UsingJobData(kv.Key, i);
                            else if (v is long l) j.UsingJobData(kv.Key, l);
                            else if (v is bool bVal) j.UsingJobData(kv.Key, bVal);
                            else j.UsingJobData(kv.Key, JsonSerializer.Serialize(v));
                        }
                    }
                });

                if (!jobConfig.Enabled)
                {
                    continue; // Job is registered so it can be triggered manually; no cron trigger.
                }

                q.AddTrigger(t =>
                {
                    var trigger = t
                        .WithIdentity(jobConfig.TriggerKey)
                        .ForJob(jobConfig.JobKey)
                        .WithCronSchedule(jobConfig.Schedule, x =>
                        {
                            x.InTimeZone(TimeZoneInfo.Utc);
                            switch (jobConfig.MisfirePolicy)
                            {
                                case MisfirePolicyEnum.DoNothing:
                                    x.WithMisfireHandlingInstructionDoNothing();
                                    break;
                                case MisfirePolicyEnum.FireAndProceed:
                                    x.WithMisfireHandlingInstructionFireAndProceed();
                                    break;
                                default:
                                    x.WithMisfireHandlingInstructionDoNothing();
                                    break;
                            }
                        });

                    if (jobConfig.Options != null)
                    {
                        foreach (var kv in jobConfig.Options)
                        {
                            var v = kv.Value;
                            if (v == null) continue;
                            if (v is string s)
                                trigger = trigger.UsingJobData(kv.Key, s);
                            else if (v is int i)
                                trigger = trigger.UsingJobData(kv.Key, i);
                            else if (v is long l)
                                trigger = trigger.UsingJobData(kv.Key, l);
                            else if (v is bool b)
                                trigger = trigger.UsingJobData(kv.Key, b);
                            else
                                trigger = trigger.UsingJobData(kv.Key, JsonSerializer.Serialize(v));
                        }
                    }
                });
            }
        });

        services.AddSingleton<IHostedService>(sp =>
            new QuartzCleanupHostedService(
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<ISchedulerFactory>(),
                desiredTriggerKeys,
                sp.GetRequiredService<ILogger<QuartzCleanupHostedService>>()));
        
        services.AddQuartzHostedService(opt => 
            opt.WaitForJobsToComplete = true);
        
        return services;
    }

    private static Type? ResolveJobType(string jobName)
    {
        var fullName = $"{JobNamespace}.{jobName}";
        var type = JobAssembly.GetType(fullName);
        if (type != null)
            return type;
        var assemblyName = JobAssembly.GetName().Name;
        return Type.GetType($"{fullName}, {assemblyName}");
    }
}

sealed class QuartzCleanupHostedService : IHostedService
{
    private const long CleanupLockKey = 8612345678901234;
    private readonly string _connectionString;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzCleanupHostedService> _logger;
    private readonly HashSet<TriggerKey> _desiredTriggerKeys;

    private List<string> exludedGroups = new List<string>() {
        ApplyPOIsAfterPayJobConstants.JobGroup,
        PaymentStatusPollingJobConstants.JobGroup,
        SendCampaignNotificationsJobConstants.JobGroup
        };
    public QuartzCleanupHostedService(
        IConfiguration configuration,
        ISchedulerFactory schedulerFactory,
        HashSet<TriggerKey> desiredTriggerKeys,
        ILogger<QuartzCleanupHostedService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Missing DefaultConnection");
        _schedulerFactory = schedulerFactory;
        _logger = logger;
        _desiredTriggerKeys = desiredTriggerKeys.ToHashSet();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        var lockTaken = await TryAcquireLockAsync(conn, cancellationToken);
        if (!lockTaken)
        {
            _logger.LogInformation("Quartz cleanup skipped on this node; another node owns startup cleanup lock.");
            return;
        }

        try
        {
            _logger.LogInformation("Quartz cleanup lock acquired; cleaning stale triggers.");

            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            var groups = await scheduler.GetTriggerGroupNames(cancellationToken);

            foreach (var group in groups)
            {
                if (exludedGroups.Contains(group))
                {
                    _logger.LogInformation("Skipping Quartz trigger group {TriggerGroup} from cleanup because it's in the excluded list.", group);
                    continue;
                }
                var existingKeys = await scheduler.GetTriggerKeys(
                    GroupMatcher<TriggerKey>.GroupEquals(group),
                    cancellationToken);

                foreach (var triggerKey in existingKeys)
                {
                    if (!_desiredTriggerKeys.Contains(triggerKey))
                    {
                        _logger.LogInformation("Unscheduling stale Quartz trigger {TriggerGroup}.{TriggerName}",
                            triggerKey.Group, triggerKey.Name);

                        await scheduler.UnscheduleJob(triggerKey, cancellationToken);
                    }
                }
            }
        }
        finally
        {
            await ReleaseLockAsync(conn, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    private static async Task<bool> TryAcquireLockAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand("select pg_try_advisory_lock(@k)", conn);
        cmd.Parameters.AddWithValue("k", CleanupLockKey);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is bool b && b;
    }

    private static async Task ReleaseLockAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand("select pg_advisory_unlock(@k)", conn);
        cmd.Parameters.AddWithValue("k", CleanupLockKey);
        await cmd.ExecuteScalarAsync(ct);
    }
}
