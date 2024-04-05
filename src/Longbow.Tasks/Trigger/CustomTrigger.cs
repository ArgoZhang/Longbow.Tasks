// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;

namespace Longbow.Tasks;

internal class CustomTrigger : DefaultTrigger
{
    private readonly Func<DateTimeOffset> _nextRunTime;

    public CustomTrigger(Func<DateTimeOffset> nextRunTime)
    {
        _nextRunTime = nextRunTime;
    }

    public override bool Pulse(CancellationToken cancellationToken = default)
    {
        var nextTime = NextRuntime;
        if (nextTime != null)
        {
            // 等待时间间隔周期
            var interval = nextTime.Value - DateTimeOffset.Now;
            var ret = !cancellationToken.WaitHandle.WaitOne(interval);
            NextRuntime = _nextRunTime();
            LastRuntime = DateTimeOffset.Now;

            return ret;
        }

        return false;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override void Run()
    {
        base.Run();
    }

    public override string ToString()
    {
        return "Custom Trigger";
    }
}
