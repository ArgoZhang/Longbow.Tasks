// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Longbow.Tasks;

/// <summary>
/// 任务持久化操作类
/// </summary>
internal class TaskStorageBuilder : ITaskStorageBuilder
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="services"></param>
    public TaskStorageBuilder(IServiceCollection services) => Services = services;

    /// <summary>
    /// 获得 容器服务集合 IServiceCollection 实例
    /// </summary>
    public IServiceCollection Services { get; }
}
