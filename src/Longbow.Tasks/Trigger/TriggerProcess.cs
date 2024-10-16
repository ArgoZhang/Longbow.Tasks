// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks;

/// <summary>
/// Trigger 触发器进程类 负责维护触发器的运行
/// </summary>
/// <remarks>
/// 构造函数
/// </remarks>
internal class TriggerProcess(string name, Action<string> loggerAction, ITrigger trigger, IStorage storage, Func<CancellationToken, Task> doWork)
{
    /// <summary>
    /// 触发器取消令牌 此令牌单独设置触发器是否工作
    /// </summary>
    private CancellationTokenSource? _triggerCancelTokenSource;

    /// <summary>
    /// 调度取消令牌
    /// </summary>
    private CancellationToken _schedulerCancelToken;

    /// <summary>
    /// 触发器取消令牌与调度取消令牌合集
    /// </summary>
    private CancellationTokenSource? _cancelTokenSource;

    /// <summary>
    /// 获得 任务调度名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 获得/设置 触发器实例
    /// </summary>
    public ITrigger Trigger { get; } = trigger;

    /// <summary>
    /// 执行任务方法
    /// </summary>
    public Func<CancellationToken, Task> DoWork { get; } = doWork;

    /// <summary>
    /// 获得/设置 日志委托
    /// </summary>
    public Action<string> LoggerAction { get; } = loggerAction;

    /// <summary>
    /// 获得/设置 触发器持久化 IStorage 实例
    /// </summary>
    public IStorage Storage { get; } = storage;

    /// <summary>
    /// 触发器处理器开始工作
    /// </summary>
    /// <param name="cancellationToken">调度取消令牌</param>
    public void Start(CancellationToken? cancellationToken)
    {
        _schedulerCancelToken = cancellationToken ?? CancellationToken.None;
        _triggerCancelTokenSource = new CancellationTokenSource();
        _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_triggerCancelTokenSource.Token, _schedulerCancelToken);

        // 异步执行触发器开始心跳
        Task.Run(async () =>
        {
            // 等待开始时间
            if (Trigger.StartTime != null)
            {
                var interval = Trigger.StartTime.Value - DateTime.Now;
                if (interval > TimeSpan.Zero) _cancelTokenSource.Token.WaitHandle.WaitOne(interval);
            }
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                if (!Trigger.Pulse(_cancelTokenSource.Token)) break;

                LoggerAction($"{Trigger.GetType().Name} PulseAsync() Trigger.Enabled({Trigger.Enabled}) Cancelled({_cancelTokenSource.IsCancellationRequested})");

                // 立刻运行一次
                var sw = Stopwatch.StartNew();
                await DoWork(_cancelTokenSource.Token).ConfigureAwait(false);
                sw.Stop();

                Trigger.LastRunElapsedTime = sw.Elapsed;
                Trigger.PulseCallback?.Invoke(Trigger);
                LoggerAction($"{Trigger.GetType().Name} {nameof(DoWork)}({Trigger.LastResult}) Elapsed: {Trigger.LastRunElapsedTime} NextRuntime: {Trigger.NextRuntime}");
                if (Trigger.NextRuntime == null) break;

                // 持久化
                Trigger.Save(Name, Storage, LoggerAction);
            }
        });
    }

    /// <summary>
    /// 触发器停止操作
    /// </summary>
    public void Stop()
    {
        _triggerCancelTokenSource?.Cancel();
        LoggerAction($"{nameof(TriggerProcess)} Stop() called");
    }
}
