﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 任务类接口
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 任务执行操作方法
        /// </summary>
        /// <param name="cancellationToken">CancellationToken 实例</param>
        [Obsolete("已过期，请使用 IServiceProvider 重载方法")]
        Task Execute(CancellationToken cancellationToken);

        /// <summary>
        /// 任务执行操作方法
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cancellationToken">CancellationToken 实例</param>
        Task Execute(IServiceProvider provider, CancellationToken cancellationToken);
    }
}
