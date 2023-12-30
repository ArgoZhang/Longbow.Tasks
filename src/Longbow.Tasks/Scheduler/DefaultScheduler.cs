// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Longbow.Tasks
{
    /// <summary>
    /// 内部默认任务调度类
    /// </summary>
    internal class DefaultScheduler : IScheduler
    {
        /// <summary>
        /// 获得/设置 下一次运行时间 为空时表示不再运行
        /// </summary>
        public DateTimeOffset? NextRuntime { get => Status == SchedulerStatus.Running ? Triggers.Where(t => t.Enabled && t.NextRuntime != null).Min(t => t.NextRuntime) : null; }

        /// <summary>
        /// 获得/设置 上一次运行时间 为空时表示未运行
        /// </summary>
        public DateTimeOffset? LastRuntime { get { return SchedulerProcess?.Triggers.Select(t => t.Trigger.LastRuntime).Max(); } }

        /// <summary>
        /// 获得/设置 上一次任务运行结果
        /// </summary>
        public TriggerResult LastRunResult { get { return SchedulerProcess?.Triggers.FirstOrDefault(t => t.Trigger.LastRuntime == LastRuntime)?.Trigger.LastResult ?? TriggerResult.Error; } }

        /// <summary>
        /// 获得/设置 调度器创建时间
        /// </summary>
        public DateTimeOffset CreatedTime { get; } = DateTimeOffset.Now;

        /// <summary>
        /// 获得/设置 任务调度名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获得/设置 调度器相关触发器
        /// </summary>
        public IEnumerable<ITrigger> Triggers => SchedulerProcess?.Triggers.Select(t => t.Trigger) ?? Array.Empty<DefaultTrigger>();

        /// <summary>
        /// 获得/设置 调度处理器实例
        /// </summary>
        public SchedulerProcess? SchedulerProcess { get; set; }

        /// <summary>
        /// 获得/设置 调度器相关联任务实例
        /// </summary>
        public ITask? Task { get; set; }

        /// <summary>
        /// 获得/设置 任务调度状态
        /// </summary>
        public SchedulerStatus Status
        {
            get => SchedulerProcess?.Status ?? SchedulerStatus.Disabled;
            set
            {
                if (SchedulerProcess != null && SchedulerProcess.Status != value)
                {
                    SchedulerProcess.Status = value;
                    SchedulerProcess.LoggerAction($"{nameof(Tasks.SchedulerProcess)} SchedulerStatus({value})");
                    if (value == SchedulerStatus.Running)
                    {
                        // 运行调度
                        SchedulerProcess.Start();
                    }
                    else
                    {
                        // 停止调度
                        SchedulerProcess.Stop();
                    }
                }
            }
        }

        /// <summary>
        /// 获得/设置 上一次错误信息
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public DefaultScheduler(string name) => Name = name;

        /// <summary>
        /// 额外的数据
        /// </summary>
        private object? Data { get; set; }

        /// <summary>
        /// 设置 额外的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void SetData<T>(T? data) where T : class => Data = data;

        /// <summary>
        /// 获得 额外的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetData<T>() where T : class => Data as T;
    }
}
