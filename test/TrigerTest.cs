// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Longbow.Tasks.Test;

public class TrigerTest
{
    [Fact]
    public void Run_Ok()
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

        scheduler1.Run();
        autoReset.WaitOne();
        Assert.Equal(2, count);
    }
}
