// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Longbow.Tasks;

/// <summary>
/// 内部默认 Trigger 仅执行一次任务
/// </summary>
internal class DefaultTrigger : ITrigger
{
    private bool _enabled = true;
    /// <summary>
    /// 获得/设置 触发器是否启用
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                if (!value)
                {
                    NextRuntime = null;
                    LastResult = TriggerResult.Cancelled;
                }
                EnabledChanged?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// 获得/设置 触发器名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 获得/设置 Enabled 改变回调方法
    /// </summary>
    public Action<bool>? EnabledChanged { get; set; }

    /// <summary>
    /// 获得 上次任务执行时间
    /// </summary>
    public DateTimeOffset? LastRuntime { get; set; }

    /// <summary>
    /// 获得/设置 任务开始时间
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// 获得上一次运行任务耗时
    /// </summary>
    public TimeSpan LastRunElapsedTime { get; set; }

    /// <summary>
    /// 获得 下一次任务运行时刻
    /// </summary>
    public DateTimeOffset? NextRuntime { get; set; }

    /// <summary>
    /// 获得 触发器上一次结果状态
    /// </summary>
    public TriggerResult LastResult { get; set; }

    /// <summary>
    /// 触发器执行情况回调方法
    /// </summary>
    public Action<ITrigger>? PulseCallback { get; set; }

    /// <summary>
    /// 获得/设置 任务超时时间
    /// </summary>
    public TimeSpan Timeout { get; set; }

    private bool _runState;

    /// <summary>
    /// 触发器 心跳 返回 true 时触发任务执行 同步阻塞线程方法 内部阻塞到 ITrigger 的下一次运行时间
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回真时表示执行任务</returns>
    public virtual bool Pulse(CancellationToken cancellationToken = default)
    {
        if (LastRuntime == null)
        {
            LastRuntime = DateTimeOffset.Now;
            _runState = true;
        }
        else
        {
            _runState = false;
        }
        return _runState;
    }

    public virtual void Run()
    {
        LastRuntime = null;
    }

    /// <summary>
    /// 重载 ToString 方法 返回 Run once trigger
    /// </summary>
    /// <returns></returns>
    public override string ToString() => "Run once trigger";

    /// <summary>
    /// 设置序列化属性集合方法
    /// </summary>
    /// <returns></returns>
    public virtual Dictionary<string, object> SetData() => new Dictionary<string, object>()
    {
        ["Name"] = Name,
        ["LastResult"] = LastResult,
        ["NextRuntime"] = NextRuntime?.ToString() ?? "",
        ["LastRunElapsedTime"] = LastRunElapsedTime.ToString(),
        ["StartTime"] = StartTime?.ToString() ?? "",
        ["LastRuntime"] = LastRuntime?.ToString() ?? ""
    };

    /// <summary>
    /// 加载序列化属性集合值方法
    /// </summary>
    /// <param name="datas"></param>
    public virtual void LoadData(Dictionary<string, object> datas)
    {
        if (datas["Name"] != null) Name = datas["Name"].ToString() ?? "";
        if (Enum.TryParse<TriggerResult>(datas["LastResult"].ToString(), out var result)) LastResult = result;
        if (TimeSpan.TryParse(datas["LastRunElapsedTime"].ToString(), out var elapsedTime)) LastRunElapsedTime = elapsedTime;
        if (DateTimeOffset.TryParse(datas["NextRuntime"]?.ToString(), out var nextRuntime)) NextRuntime = nextRuntime;
        if (NextRuntime != null && NextRuntime < DateTimeOffset.Now) NextRuntime = null;
        if (DateTimeOffset.TryParse(datas["StartTime"]?.ToString(), out var startTime)) StartTime = startTime;
        if (DateTimeOffset.TryParse(datas["LastRuntime"]?.ToString(), out var lastRuntime)) LastRuntime = lastRuntime;
    }
}
