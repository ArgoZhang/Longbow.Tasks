// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks;

/// <summary>
/// ITask 内部实现类
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="methodCall">匿名方法体</param>
class DefaultTask(Func<IServiceProvider, CancellationToken, Task> methodCall) : ITask
{
    private readonly Func<IServiceProvider, CancellationToken, Task> _methodCall = methodCall;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task Execute(IServiceProvider provider, CancellationToken cancellationToken) => _methodCall.Invoke(provider, cancellationToken);
}
