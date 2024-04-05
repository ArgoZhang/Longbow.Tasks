// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Longbow.Tasks.Test;

public class TrigerTest
{
    [Fact]
    public async Task DefaultTrigger_Run()
    {
        TaskServicesManager.Init();

        var autoReset = new AutoResetEvent(false);
        var count = 0;
        var scheduler1 = TaskServicesManager.GetOrAdd("test-run", (provider, token) =>
        {
            count++;
            autoReset.Set();
            return Task.CompletedTask;
        });
        autoReset.WaitOne();

        await scheduler1.Run();
        autoReset.WaitOne();
        Assert.Equal(2, count);
    }

    [Fact]
    public void RecurringTrigger_Run()
    {
        TaskServicesManager.Init();

        var autoReset = new AutoResetEvent(false);
        var count = 0;
        var scheduler1 = TaskServicesManager.GetOrAdd("test-run", (provider, token) =>
        {
            count++;
            autoReset.Set();
            return Task.CompletedTask;
        }, TriggerBuilder.Default.WithInterval(5000).Build());

        scheduler1.Run();
        autoReset.WaitOne();
        Assert.Equal(1, count);
    }

    [Fact]
    public void CronTrigger_Run()
    {
        TaskServicesManager.Init();

        var autoReset = new AutoResetEvent(false);
        var count = 0;
        var scheduler1 = TaskServicesManager.GetOrAdd("test-run", (provider, token) =>
        {
            count++;
            autoReset.Set();
            return Task.CompletedTask;
        }, TriggerBuilder.Build("10 * * * * *"));

        scheduler1.Run();
        autoReset.WaitOne();
        Assert.Equal(1, count);
    }
}
