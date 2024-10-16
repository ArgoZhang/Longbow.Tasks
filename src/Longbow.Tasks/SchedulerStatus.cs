// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

namespace Longbow.Tasks;

/// <summary>
/// 调度器枚举类型
/// </summary>
public enum SchedulerStatus
{
    /// <summary>
    /// 准备
    /// </summary>
    Ready = 0,

    /// <summary>
    /// 运行中
    /// </summary>
    Running = 1,

    /// <summary>
    /// 被禁用
    /// </summary>
    Disabled = 2
}
