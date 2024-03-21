// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Longbow.Tasks.Test;

public class ScheduleTaskTest
{
    [Fact]
    public void Wait_True()
    {
        var cts = new CancellationTokenSource();
        Assert.False(cts.Token.WaitHandle.WaitOne(2000));
        cts.CancelAfter(1000);
        Assert.True(cts.Token.WaitHandle.WaitOne(2000));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void WithCronExpression_Exception(string cronExpress)
    {
        Assert.ThrowsAny<ArgumentNullException>(() => TriggerBuilder.Build(cronExpress));
    }

    [Fact]
    public void WithCronExpression()
    {
        TriggerBuilder.Build(Cron.Secondly(), -1);
        TriggerBuilder.Build(Cron.Secondly(), -1, DateTimeOffset.Now);
    }

    [Fact]
    public void WithStartTime_Ok()
    {
        var trigger = TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddMilliseconds(500)).Build();
        var trigger1 = TriggerBuilder.Default.WithStartTime().Build();
        Assert.Equal(DateTimeOffset.MinValue, trigger1.StartTime);
    }

    [Fact]
    public void WithTimeout_Ok()
    {
        var trigger = TriggerBuilder.Build(Cron.Secondly());
        Assert.Equal(Timeout.InfiniteTimeSpan, trigger.Timeout);
        trigger = TriggerBuilder.Build(Cron.Secondly(), 1000);
        Assert.Equal(TimeSpan.FromSeconds(1), trigger.Timeout);

        trigger = TriggerBuilder.Default.WithTimeout().Build();
        Assert.Equal(Timeout.InfiniteTimeSpan, trigger.Timeout);
        trigger = TriggerBuilder.Default.WithTimeout(1000).Build();
        Assert.Equal(TimeSpan.FromSeconds(1), trigger.Timeout);
        trigger = TriggerBuilder.Default.WithTimeout(TimeSpan.FromSeconds(1)).Build();
        Assert.Equal(TimeSpan.FromSeconds(1), trigger.Timeout);
    }

    [Fact]
    public void WithInterval_Ok()
    {
        var trigger = TriggerBuilder.Default.WithInterval().Build();
        trigger = TriggerBuilder.Default.WithInterval(1000).Build();
    }

    [Fact]
    public void WithCustom_Ok()
    {
        var trigger = TriggerBuilder.Default.WithCustom(() => DateTimeOffset.Now.AddSeconds(10)).Build();
    }

    [Fact]
    public void Trigger_ToString()
    {
        Assert.Equal("Run once trigger", TriggerBuilder.Default.Build().ToString());
        var repeatCount = 2;
        var interval = TimeSpan.FromMinutes(1);
        Assert.Equal($"RepeatCount({repeatCount} Interval({interval}) Trigger", TriggerBuilder.Default.WithRepeatCount(repeatCount).WithInterval(interval).Build().ToString());
        var cron = "* * * * * *";
        Assert.Equal(cron, TriggerBuilder.Build(cron).ToString());
    }

    [Fact]
    public void TaskServicesOptions_Ok()
    {
        var op = new TaskServicesOptions()
        {
            ShutdownTimeout = TimeSpan.FromMinutes(1),
        };

        Assert.Equal(TimeSpan.FromMinutes(1), op.ShutdownTimeout);
    }

    [Fact]
    public void List_OK()
    {
        var source = Enumerable.Range(1, 2);
        var source1 = source.ToList();
        source1.Add(2);
        Assert.Equal(2, source.Count());
        Assert.Equal(3, source1.Count);
    }

    private class FooTrigger : ITrigger
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public Action<bool> EnabeldChanged { get; set; }
        public DateTimeOffset? NextRuntime { get; set; }
        public TimeSpan LastRunElapsedTime { get; set; }
        public TriggerResult LastResult { get; set; }
        public TimeSpan Timeout { get; set; }
        public Action<ITrigger> PulseCallback { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? LastRuntime { get; }

        public void LoadData(Dictionary<string, object> datas) { }

        public bool Pulse(CancellationToken cancellationToken = default) => true;

        public Dictionary<string, object> SetData() => new Dictionary<string, object>();
    }
}
