using Chatify.Infrastructure.Common.Settings;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Chatify.Infrastructure.Common;

internal sealed class SelectiveJobFactory(QuartzSettings settings)
    : IJobFactory
{
    private readonly SimpleJobFactory _simpleJobFactory = new();

    public IJob NewJob(
        TriggerFiredBundle bundle,
        IScheduler scheduler)
        => !settings.Enabled
            ? _simpleJobFactory.NewJob(bundle, scheduler)
            : default!;

    public void ReturnJob(IJob job) => _simpleJobFactory.ReturnJob(job);
}