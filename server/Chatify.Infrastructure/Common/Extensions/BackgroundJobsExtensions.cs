using Quartz;

namespace Chatify.Infrastructure.Common.Extensions;

public static class BackgroundJobsExtensions
{
    public static async Task<DateTimeOffset> ScheduleImmediateJob<TJob>(
        this IScheduler scheduler,
        Action<JobBuilder> configureJob,
        CancellationToken cancellationToken = default)
        where TJob : IJob
    {
        var jobBuilder = JobBuilder
            .Create<TJob>();
        configureJob(jobBuilder);
        
        var jobDetail = jobBuilder.Build();
        var trigger = TriggerBuilder
            .Create()
            .StartNow()
            .WithIdentity(typeof(TJob).Name)
            .WithSimpleSchedule(s => s.WithRepeatCount(0))
            .Build();

        return await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
    }
}