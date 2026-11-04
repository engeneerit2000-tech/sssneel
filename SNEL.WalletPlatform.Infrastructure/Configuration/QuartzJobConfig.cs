using Quartz;

namespace SNEL.WalletPlatform.Infrastructure.Configuration;

/// <summary>
/// Configuration for a single Quartz job. The job class name must match <see cref="Name"/> (e.g. SyncGcoEnCoursFacturesJob).
/// </summary>
public class QuartzJobConfig
{
    public const string SectionName = "QuartzJobs";

    /// <summary>Job class name; must match the actual job type name (e.g. SyncGcoEnCoursFacturesJob).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether the job is enabled and should be scheduled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Cron expression for the schedule (e.g. "0 0 21 * * ?" for daily at 21:00 UTC).</summary>
    public string Schedule { get; set; } = string.Empty;

    /// <summary>MisfirePolicy from <see cref="MisfirePolicyEnum"/> </summary>
    public MisfirePolicyEnum MisfirePolicy { get; set; } = MisfirePolicyEnum.DoNothing;
    
    /// <summary>Optional job data passed to the job via MergedJobDataMap (e.g. BatchSize).</summary>
    public Dictionary<string, object>? Options { get; set; }

    public TriggerKey TriggerKey => new TriggerKey($"{Name}-trigger", "default");
    public JobKey JobKey => new JobKey(Name, "default");
}

public enum MisfirePolicyEnum
{
    DoNothing,
    FireAndProceed
}

