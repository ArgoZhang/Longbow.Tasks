#if !NET45
using Microsoft.Extensions.Options;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Longbow.Tasks
{
    /// <summary>
    /// 持久化到物理文件操作类
    /// </summary>
#if !NET45
    internal class FileStorage : IStorage
#else
    public class FileStorage : IStorage
#endif
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
#if !NET45
        public FileStorage(IOptionsMonitor<FileStorageOptions> options)
        {
            options.OnChange(op => Options = op);
            Options = options.CurrentValue;
        }
#else
        public FileStorage(FileStorageOptions options)
        {
            Options = options;
        }
#endif

        /// <summary>
        /// 从物理文件加载 ITrigger 触发器
        /// </summary>
        /// <param name="schedulerName">任务调度器名称</param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool Load(string schedulerName, ITrigger trigger)
        {
            // 从文件加载
            Exception = null;
            var ret = true;
            if (Options.Enabled)
            {
                var fileName = RetrieveFileNameBySchedulerName(schedulerName);
                if (File.Exists(fileName))
                {
                    try
                    {
                        lock (locker) trigger.Deserialize(fileName, Options);
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;
                        ret = false;
                        // load 失败删除文件防止一直 load 出错
                        var target = $"{fileName}.err";
                        if (File.Exists(target)) File.Delete(target);
                        File.Move(fileName, $"{fileName}.err");
                    }
                }
            }
            return ret;
        }

        private static readonly object locker = new object();
        /// <summary>
        /// 持久化 ITrigger 实例到物理文件
        /// </summary>
        /// <param name="schedulerName">任务调度器名称</param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public bool Save(string schedulerName, ITrigger trigger)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

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
        public bool Remove(IEnumerable<string> schedulerNames)
        {
            var ret = true;
            if (Options.DeleteFileByRemoveEvent)
            {
                schedulerNames.AsParallel().ForAll(name =>
                {
                    var fileName = RetrieveFileNameBySchedulerName(name);
                    try
                    {
                        if (File.Exists(fileName))
                        {
                            lock (locker)
                            {
                                File.Delete(fileName);
                            }
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
        /// 通过指定调度器名称获得持久化文件名称
        /// </summary>
        /// <param name="schedulerName">调度器名称</param>
        /// <returns></returns>
        protected string RetrieveFileNameBySchedulerName(string schedulerName)
        {
#if !NET45
            var folder = Options.Folder.GetOSPlatformPath();
#else
            var folder = Options.Folder;
#endif
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? "", folder, $"{schedulerName}.bin");
        }
    }
}
