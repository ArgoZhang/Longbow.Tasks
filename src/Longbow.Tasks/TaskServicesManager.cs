// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks;

/// <summary>
/// 后台服务管理操作类
/// </summary>
public static class TaskServicesManager
{
    private static TaskServicesFactory? Factory { get { return TaskServicesFactory.Instance; } }

    /// <summary>
    /// 后台服务初始化 提供控制台或者 WinForm 使用
    /// </summary>
    /// <param name="options">后台任务服务配置实例</param>
    /// <param name="storage">IStorage 任务持久化接口实例</param>
    public static void Init(TaskServicesOptions? options = null, IStorage? storage = null)
    {
        TaskServicesFactory.Create(options, storage ?? new NoneStorage());
        _ = Factory?.StartAsync();
    }

    internal static readonly ConcurrentDictionary<string, Lazy<SchedulerProcess>> _schedulerPool = new();

    /// <summary>
    /// 将任务与触发器添加到调度中 多线程安全
    /// </summary>
    /// <typeparam name="T">任务</typeparam>
    /// <param name="trigger">ITrigger 实例 为空时内部使用 TriggerBuilder.Default</param>
    /// <returns>返回 IScheduler 实例</returns>
    public static IScheduler GetOrAdd<T>(ITrigger? trigger = null) where T : ITask, new() => GetOrAdd<T>(typeof(T).Name, trigger);

    /// <summary>
    /// 将任务与触发器添加到调度中 多线程安全
    /// </summary>
    /// <typeparam name="T">任务</typeparam>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <param name="trigger">ITrigger 实例 为空时内部使用 TriggerBuilder.Default</param>
    /// <returns>返回 IScheduler 实例</returns>
    public static IScheduler GetOrAdd<T>(string schedulerName, ITrigger? trigger = null) where T : ITask, new()
    {
        if (string.IsNullOrEmpty(schedulerName))
        {
            schedulerName = typeof(T).Name;
        }

        return _schedulerPool.GetOrAdd(schedulerName, key => new Lazy<SchedulerProcess>(() =>
        {
            var process = GetSchedulerProcess(key);

            // 绑定任务与触发器
            process.Start<T>(trigger ?? TriggerBuilder.Default.Build());
            return process;
        })).Value.Scheduler;
    }

    /// <summary>
    /// 将任务与触发器添加到调度中 多线程安全
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <returns>返回 IScheduler 实例</returns>
    public static IScheduler? Get(string schedulerName)
    {
        _schedulerPool.TryGetValue(schedulerName, out var process);
        return process?.Value.Scheduler;
    }

    /// <summary>
    /// 将任务与触发器添加到调度中 多线程安全
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <param name="methodCall">创建任务委托 string 为 Scheduler 名称</param>
    /// <param name="trigger">ITrigger 实例 为空时内部使用 TriggerBuilder.Default</param>
    /// <returns>返回 IScheduler 实例</returns>
    public static IScheduler GetOrAdd(string schedulerName, Func<IServiceProvider, CancellationToken, Task> methodCall, ITrigger? trigger = null) => GetOrAdd(schedulerName, new DefaultTask(methodCall), trigger);

    /// <summary>
    /// 将任务与触发器添加到调度中 多线程安全
    /// </summary>
    /// <param name="schedulerName">Scheduler 名称</param>
    /// <param name="task">创建任务委托 string 为 Scheduler 名称</param>
    /// <param name="trigger">ITrigger 实例 为空时内部使用 TriggerBuilder.Default</param>
    /// <returns>返回 IScheduler 实例</returns>
    public static IScheduler GetOrAdd(string schedulerName, ITask task, ITrigger? trigger = null) => _schedulerPool.GetOrAdd(schedulerName, key => new Lazy<SchedulerProcess>(() =>
    {
        var process = GetSchedulerProcess(key);

        // 绑定任务与触发器
        process.Start(task, trigger ?? TriggerBuilder.Default.Build());
        return process;
    })).Value.Scheduler;

    private static SchedulerProcess GetSchedulerProcess(string key)
    {
        // 创建调度
        if (Factory == null)
        {
            throw new InvalidOperationException("Please Call the AddTaskServices method in the Startup ConfigureServices first.");
        }
        var scheduler = new DefaultScheduler(key);
        var process = new SchedulerProcess(scheduler, Factory.Log, Factory.Storage);
        process.LoggerAction($"{nameof(TaskServicesManager)} {nameof(DefaultScheduler)}({key}) Created");

        // 关联调度与调度执行器
        scheduler.SchedulerProcess = process;
        return process;
    }

    /// <summary>
    /// 移除指定名称的任务
    /// </summary>
    /// <param name="schedulerName">任务名称</param>
    public static bool Remove(string schedulerName)
    {
        Factory?.Storage.Remove([schedulerName]);
        var ret = _schedulerPool.TryRemove(schedulerName, out var scheduler);
        if (ret && scheduler != null)
        {
            scheduler.Value.LoggerAction("Remove()");
            scheduler.Value.Stop();
        }
        return ret;
    }

    /// <summary>
    /// 清除所有调度
    /// </summary>
    public static void Clear()
    {
        Shutdown();
        var scheduler = _schedulerPool.Keys;
        Factory?.Storage.Remove(scheduler);
        _schedulerPool.Clear();
    }

    /// <summary>
    /// 将内部所有调度转化为集合
    /// </summary>
    /// <returns>IScheduler 集合实例</returns>
    public static IEnumerable<IScheduler> ToList() => _schedulerPool.Values.Select(v => v.Value.Scheduler);

    /// <summary>
    /// 停止所有后台调度
    /// </summary>
    internal static void Shutdown(CancellationToken token = default)
    {
        foreach (var scheduler in _schedulerPool.Values)
        {
            scheduler.Value.Stop();
            if (token.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
