// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks;

/// <summary>
/// 持久化到物理文件操作类
/// </summary>
public class FileStorage : IStorage
{
    /// <summary>
    /// 获得/设置 物理文件持久化文件目录名称 默认 TaskStorage
    /// </summary>
    public FileStorageOptions Options { get; private set; }

    /// <summary>
    /// 获得 上一次操作异常信息实例
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public FileStorage(IOptionsMonitor<FileStorageOptions> options)
    {
        options.OnChange(op => Options = op);
        Options = options.CurrentValue;
    }

    /// <summary>
    /// 从物理文件加载 ITrigger 触发器
    /// </summary>
    /// <returns></returns>
    public virtual Task LoadAsync()
    {
        // 从文件加载
        Exception = null;
        if (Options.Enabled)
        {
            RetrieveSchedulers().AsParallel().ForAll(fileName =>
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        lock (locker)
                        {
                            var scheduleName = Path.GetFileNameWithoutExtension(fileName);
                            var trigger = JsonSerializeExtensions.Deserialize(fileName, Options);
                            var task = CreateTaskByScheduleName(scheduleName);
                            if (task != null)
                            {
                                TaskServicesManager.GetOrAdd(scheduleName, task, trigger);
                            }
                            else
                            {
                                var callback = CreateCallbackByScheduleName(scheduleName);
                                if (callback != null)
                                {
                                    TaskServicesManager.GetOrAdd(scheduleName, callback, trigger);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;

                        // load 失败删除文件防止一直 load 出错
                        var target = $"{fileName}.err";
                        if (File.Exists(target)) File.Delete(target);
                        File.Move(fileName, $"{fileName}.err");
                    }
                }
            });
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scheduleName"></param>
    /// <returns></returns>
    protected virtual ITask? CreateTaskByScheduleName(string scheduleName) => null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scheduleName"></param>
    /// <returns></returns>
    protected virtual Func<IServiceProvider, CancellationToken, Task>? CreateCallbackByScheduleName(string scheduleName) => null;

    private static readonly object locker = new();
    /// <summary>
    /// 持久化 ITrigger 实例到物理文件
    /// </summary>
    /// <param name="schedulerName">任务调度器名称</param>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public virtual bool Save(string schedulerName, ITrigger trigger)
    {
        Exception = null;
        var ret = true;
        if (Options.Enabled)
        {
            try
            {
                lock (locker)
                {
                    var fileName = RetrieveFileNameBySchedulerName(schedulerName);
                    trigger.Serialize(fileName, Options);
                }
            }
            catch (Exception ex) { Exception = ex; ret = false; }
        }
        return ret;
    }

    /// <summary>
    /// 移除指定任务调度
    /// </summary>
    /// <param name="schedulerNames">要移除调度名称集合</param>
    public virtual bool Remove(IEnumerable<string> schedulerNames)
    {
        var ret = true;
        if (Options.DeleteFileByRemoveEvent)
        {
            schedulerNames.AsParallel().ForAll(name =>
            {
                var files = RetrieveSchedulers();
                try
                {
                    var file = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    ret = false;
                }
            });
        }
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="schedulerName"></param>
    /// <returns></returns>
    protected virtual string RetrieveFileNameBySchedulerName(string schedulerName)
    {
        var folder = Options.Folder.GetOSPlatformPath();
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", folder, $"{schedulerName}.bin");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<string> RetrieveSchedulers()
    {
        var folder = Options.Folder.GetOSPlatformPath();
        var workFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", folder);
        if (!Directory.Exists(workFolder))
        {
            Directory.CreateDirectory(workFolder);
        }
        return Directory.EnumerateFiles(workFolder, "*.bin", SearchOption.TopDirectoryOnly);
    }
}
