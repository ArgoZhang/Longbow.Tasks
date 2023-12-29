// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// ITask 内部实现类
    /// </summary>
    class DefaultTask : ITask
    {
        private readonly Func<CancellationToken, Task> _methodCall = default!;

        private readonly Func<IServiceProvider, CancellationToken, Task> _providerMethodCall = default!;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodCall">匿名方法体</param>
        [Obsolete("已过期，请使用 IServiceProvider 重载方法")]
        public DefaultTask(Func<CancellationToken, Task> methodCall)
        {
            _methodCall = methodCall;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodCall">匿名方法体</param>
        public DefaultTask(Func<IServiceProvider, CancellationToken, Task> methodCall)
        {
            _providerMethodCall = methodCall;
        }

        /// <summary>
        /// 任务执行体
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        [Obsolete("已过期，请使用 IServiceProvider 重载方法")]
        public Task Execute(CancellationToken cancellationToken) => _methodCall.Invoke(cancellationToken);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task Execute(IServiceProvider provider, CancellationToken cancellationToken) => _providerMethodCall.Invoke(provider, cancellationToken);
    }
}
