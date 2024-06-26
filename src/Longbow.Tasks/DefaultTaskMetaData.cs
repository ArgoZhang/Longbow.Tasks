// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks;

class DefaultTaskMetaData(ITask task)
{
    /// <summary>
    /// 
    /// </summary>
    public ITask Task { get; } = task;

    /// <summary>
    /// 任务执行操作方法
    /// </summary>
    /// <param name="cancellationToken">CancellationToken 实例</param>
    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Execute(TaskServicesFactory.Instance.ServiceProvider, cancellationToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            if (Task is IAsyncDisposable asyncDispose)
            {
                await asyncDispose.DisposeAsync();
            }
            if (Task is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
