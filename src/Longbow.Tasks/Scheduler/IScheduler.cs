// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Longbow.Tasks;

/// <summary>
/// 任务调度类接口
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// 获得 任务调度名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获得/设置 调度器状态
    /// </summary>
    SchedulerStatus Status { get; set; }

    /// <summary>
    /// 获得 下一次运行时间 为空时表示不再运行
    /// </summary>
    DateTimeOffset? NextRuntime { get; }

    /// <summary>
    /// 获得 上一次运行时间 为空时表示未运行
    /// </summary>
    DateTimeOffset? LastRuntime { get; }

    /// <summary>
    /// 获得 上一次任务运行结果
    /// </summary>
    TriggerResult LastRunResult { get; }

    /// <summary>
    /// 获得 上一次运行异常
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// 获得 调度器创建时间
    /// </summary>
    DateTimeOffset CreatedTime { get; }

    /// <summary>
    /// 获得 调度器相关触发器
    /// </summary>
    IEnumerable<ITrigger> Triggers { get; }

    /// <summary>
    /// 获得 调度器相关联任务
    /// </summary>
    ITask? Task { get; }

    /// <summary>
    /// 立即执行任务方法
    /// </summary>
    Task Run();
}
