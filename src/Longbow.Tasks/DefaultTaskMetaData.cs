// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

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
    /// 
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Created;

    /// <summary>
    /// 任务执行操作方法
    /// </summary>
    /// <param name="cancellationToken">CancellationToken 实例</param>
    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            Status = TaskStatus.Running;
            await Task.Execute(TaskServicesFactory.Instance.ServiceProvider, cancellationToken);
            Status = TaskStatus.RanToCompletion;
        }
        catch (OperationCanceledException)
        {
            Status = TaskStatus.Canceled;
        }
        catch
        {
            Status = TaskStatus.Faulted;
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
