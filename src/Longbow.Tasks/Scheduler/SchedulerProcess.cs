﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 调度执行实体类
    /// </summary>
    internal class SchedulerProcess
    {
        private readonly DefaultScheduler _sche;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="sche">IScheduler 调度实例</param>
        /// <param name="logger">日志回调方法</param>
        /// <param name="storage">IStorage 实例</param>
        public SchedulerProcess(DefaultScheduler sche, Action<string> logger, IStorage storage)
        {
            _sche = sche;
            LoggerAction = logger;
            Storage = storage;
        }

        /// <summary>
        /// 获得/设置 任务调度状态
        /// </summary>
        public SchedulerStatus Status { get; set; }

        /// <summary>
        /// 获得/设置 任务持久化实例
        /// </summary>
        public IStorage Storage { get; }

        /// <summary>
        /// 获得/设置 任务调度实例
        /// </summary>
        public IScheduler Scheduler { get => _sche; }

        /// <summary>
        /// 获得/设置 调度任务
        /// </summary>
        public DefaultTaskMetaData? TaskContext { get; private set; }

        /// <summary>
        /// 获得 所有触发器执行实例
        /// </summary>
        public List<TriggerProcess> Triggers { get; } = new List<TriggerProcess>();

        /// <summary>
        /// 日志委托
        /// </summary>
        public Action<string> LoggerAction { get; }

        /// <summary>
        /// 调度取消令牌
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// 任务构造函数初始化取消令牌
        /// </summary>
        private readonly CancellationTokenSource _initToken = new CancellationTokenSource();

        /// <summary>
        /// 调度开始 每次调用
        /// </summary>
        /// <param name="trigger">ITrigger 实例</param>
        public void Start<T>(ITrigger trigger) where T : ITask, new()
        {
            // 泛型 T 的构造函数可能耗时很长 关注相关单元测试 TaskManagerTest -> LongDelayTrigger_Ok
            var sw = Stopwatch.StartNew();
            Task.Run(() =>
            {
                TaskContext = new DefaultTaskMetaData(new T());
                _sche.Task = TaskContext.Task;
                _initToken.Cancel();

                // Stop 调用
                if (_cancellationTokenSource?.IsCancellationRequested ?? false) return;
                LoggerAction($"{nameof(SchedulerProcess)} Start<{typeof(T).Name}> new({typeof(T).Name}) ThreadId({Thread.CurrentThread.ManagedThreadId})");
            });
            InternalStart(trigger);
            sw.Stop();
            LoggerAction($"{nameof(SchedulerProcess)} Start<{typeof(T).Name}> success Called Elapsed: {sw.Elapsed}");
        }

        /// <summary>
        /// 调度开始
        /// </summary>
        /// <param name="task">ITask 实例</param>
        /// <param name="trigger">ITrigger 实例</param>
        public void Start(ITask task, ITrigger trigger)
        {
            _initToken.Cancel();
            var sw = Stopwatch.StartNew();
            TaskContext = new DefaultTaskMetaData(task);
            _sche.Task = TaskContext.Task;
            _sche.Task = task;
            InternalStart(trigger);
            sw.Stop();
            LoggerAction($"{nameof(SchedulerProcess)} Start(methodCall) success Called Elapsed: {sw.Elapsed}");
        }

        private void InternalStart(ITrigger trigger)
        {
            var dowork = new Func<CancellationToken, Task>(async token =>
            {
                _sche.Exception = null;
                // 设置任务超时取消令牌
                var taskCancelTokenSource = new CancellationTokenSource(trigger.Timeout);
                try
                {
                    // 保证 ITask 的 new() 方法被执行完毕
                    _initToken.Token.WaitHandle.WaitOne();

                    var taskToken = CancellationTokenSource.CreateLinkedTokenSource(token, taskCancelTokenSource.Token);
                    if (!taskToken.IsCancellationRequested && TaskContext != null)
                    {
                        await TaskContext.Execute(taskToken.Token).ConfigureAwait(false);
                        trigger.LastResult = TriggerResult.Success;
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _sche.Exception = ex;
                    ex.Log();
                }

                // 设置 Trigger 状态
                if (token.IsCancellationRequested) trigger.LastResult = TriggerResult.Cancelled;
                if (taskCancelTokenSource.IsCancellationRequested) trigger.LastResult = TriggerResult.Timeout;
                if (_sche.Exception != null) trigger.LastResult = TriggerResult.Error;
            });
            var triggerProcess = new TriggerProcess(Scheduler.Name, LoggerAction, trigger, Storage, dowork);
            Triggers.Add(triggerProcess);

            // 注册触发器状态改变回调方法
            trigger.EnabeldChanged = enabled =>
            {
                LoggerAction($"{nameof(TriggerProcess)} Trigger({trigger.GetType().Name}) Enabled({enabled})");
                if (!enabled)
                {
                    triggerProcess.Stop();
                    return;
                }
                if (Status == SchedulerStatus.Running) triggerProcess.Start(_cancellationTokenSource?.Token);
            };
            if (Status == SchedulerStatus.Ready)
            {
                Status = SchedulerStatus.Running;
                _cancellationTokenSource = new CancellationTokenSource();
                triggerProcess.Start(_cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// 所有 调度开始
        /// </summary>
        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Triggers.ForEach(t => t.Start(_cancellationTokenSource.Token));
        }

        /// <summary>
        /// 调度停止
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _initToken.Cancel();
            LoggerAction($"{nameof(TriggerProcess)} Stop() Called");
        }
    }
}
