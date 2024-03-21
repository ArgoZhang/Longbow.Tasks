// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;

namespace Longbow.Tasks;

/// <summary>
/// 触发器操作类 规则是立即执行一次
/// </summary>
public class TriggerBuilder
{
    private TimeSpan _interval = TimeSpan.Zero;
    private int _repeatCount;
    private DateTimeOffset? _startTime;
    private TimeSpan _timeout = Timeout.InfiniteTimeSpan;
    private Func<DateTimeOffset>? _nextRunTime;
    private string _name = "";

    /// <summary>
    /// 获得 TriggerBuilder 新实例
    /// </summary>
    public static TriggerBuilder Default => new TriggerBuilder();

    /// <summary>
    /// 设置 任务开始时间
    /// </summary>
    /// <param name="startTime"></param>
    /// <returns></returns>
    public TriggerBuilder WithStartTime(DateTimeOffset startTime = default)
    {
        _startTime = startTime;
        return this;
    }

    /// <summary>
    /// 重复任务
    /// </summary>
    /// <param name="repeatCount">重复次数 默认值为 0 时表示一直重复</param>
    /// <returns></returns>
    public TriggerBuilder WithRepeatCount(int repeatCount = 0)
    {
        _repeatCount = repeatCount;
        return this;
    }

    /// <summary>
    /// 周期任务
    /// </summary>
    /// <param name="interval">周期间隔</param>
    /// <returns></returns>
    public TriggerBuilder WithInterval(TimeSpan interval)
    {
        _interval = interval;
        return this;
    }

    /// <summary>
    /// 周期任务
    /// </summary>
    /// <param name="milliseconds">周期间隔 默认值 1000 毫秒</param>
    /// <returns></returns>
    public TriggerBuilder WithInterval(int milliseconds = 1000)
    {
        _interval = TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    /// <summary>
    /// 设置任务超时时间
    /// </summary>
    /// <param name="timeout">任务超时时间</param>
    /// <returns></returns>
    public TriggerBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// 设置任务超时时间
    /// </summary>
    /// <param name="milliseconds">任务超时时间 默认值 -1 无超时设置</param>
    /// <returns></returns>
    public TriggerBuilder WithTimeout(int milliseconds = -1)
    {
        _timeout = milliseconds == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(milliseconds);
        return this;
    }

    public TriggerBuilder WithCustom(Func<DateTimeOffset> nextRunTime)
    {
        _nextRunTime = nextRunTime;
        return this;
    }

    /// <summary>
    /// 设置任务触发器名称
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public TriggerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// 生成 ITrigger 实例
    /// </summary>
    /// <returns></returns>
    public ITrigger Build()
    {
        // 根据不同的配置创建不同的 ITrigger
        ITrigger tri;
        if (_interval > TimeSpan.Zero)
        {
            tri = new RecurringTrigger()
            {
                Name = _name ?? Guid.NewGuid().ToString(),
                Interval = _interval,
                RepeatCount = _repeatCount,
                StartTime = _startTime,
                Timeout = _timeout,
                NextRuntime = _startTime.HasValue ? _startTime.Value.Add(_interval) : DateTimeOffset.Now.Add(_interval)
            };
        }
        else if (_nextRunTime != null)
        {
            tri = new CustomTrigger(_nextRunTime)
            {
                Name = _name ?? Guid.NewGuid().ToString(),
                NextRuntime = _nextRunTime()
            };
        }
        else
        {
            tri = new DefaultTrigger()
            {
                Name = _name ?? Guid.NewGuid().ToString(),
                StartTime = _startTime,
                Timeout = _timeout
            };
        }
        return tri;
    }

    /// <summary>
    /// 通过 Cron 表达式生成 ITrigger 实例
    /// </summary>
    /// <param name="cronExpression">Cron 表达式</param>
    /// <param name="timeout">超时时间 默认值 -1 无超时设置</param>
    /// <param name="startTime">任务开始时间 默认值 null 立即执行</param>
    /// <param name="name">触发器名称</param>
    /// <returns></returns>
    public static ITrigger Build(string cronExpression, int timeout = -1, DateTimeOffset? startTime = null, string name = "")
    {
        if (string.IsNullOrEmpty(cronExpression)) throw new ArgumentNullException(nameof(cronExpression));
        var time = timeout == -1 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout);
        return Build(cronExpression, time, startTime, name);
    }

    /// <summary>
    /// 通过 Cron 表达式生成 ITrigger 实例
    /// </summary>
    /// <param name="cronExpression">Cron 表达式</param>
    /// <param name="name">触发器名称</param>
    /// <param name="startTime">任务开始时间 默认值 null 立即执行</param>
    /// <param name="timeout">超时时间</param>
    /// <returns></returns>
    public static ITrigger Build(string cronExpression, TimeSpan timeout, DateTimeOffset? startTime = null, string name = "")
    {
        var trigger = new CronTrigger(cronExpression)
        {
            Name = name ?? Guid.NewGuid().ToString(),
            Timeout = timeout,
            StartTime = startTime
        };

        trigger.NextRuntime = trigger.CronExpression.GetNextExecution(startTime ?? DateTimeOffset.Now);
        return trigger;
    }
}
