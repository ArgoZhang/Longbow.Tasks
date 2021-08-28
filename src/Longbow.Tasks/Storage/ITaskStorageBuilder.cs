// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Longbow.Tasks
{
    /// <summary>
    /// TaskSericesBuilder 扩展任务持久化接口
    /// </summary>
    public interface ITaskStorageBuilder
    {
        /// <summary>
        /// 获取 容器服务集合
        /// </summary>
        IServiceCollection Services { get; }
    }
}
