// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Specialized;

namespace Longbow.Tasks;

internal static class TriggerStorageExtensions
{
    /// <summary>
    /// 保存任务触发器到持久化接口中
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="scheduleName"></param>
    /// <param name="storage"></param>
    /// <param name="logger"></param>
    public static void Save(this ITrigger trigger, string scheduleName, IStorage storage, Action<string> logger)
    {
        if (trigger.NextRuntime != null && !storage.Save(scheduleName, trigger) && storage.Exception != null)
        {
            logger(storage.Exception.FormatException(new NameValueCollection()
            {
                ["TaskName"] = scheduleName,
                ["TriggerName"] = trigger.Name,
                ["TriggerType"] = trigger.GetType().Name
            }));
        }
    }
}
