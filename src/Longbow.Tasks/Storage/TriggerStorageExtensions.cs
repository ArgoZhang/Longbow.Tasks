using System;
using System.Collections.Specialized;

namespace Longbow.Tasks
{
    internal static class TriggerStorageExtensions
    {
        /// <summary>
        /// 从持久化接口中加载任务触发器
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="scheduleName"></param>
        /// <param name="storage"></param>
        /// <param name="logger"></param>
        public static void Load(this ITrigger trigger, string scheduleName, IStorage storage, Action<string> logger)
        {
            if (storage != null)
            {
                if (!storage.Load(scheduleName, trigger) && storage.Exception != null) logger(storage.Exception.FormatException(new NameValueCollection()
                {
                    ["TaskName"] = scheduleName,
                    ["TriggerName"] = trigger.Name,
                    ["TriggerType"] = trigger.GetType().Name
                }));
            }
        }

        /// <summary>
        /// 保存任务触发器到持久化接口中
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="scheduleName"></param>
        /// <param name="storage"></param>
        /// <param name="logger"></param>
        public static void Save(this ITrigger trigger, string scheduleName, IStorage storage, Action<string> logger)
        {
            if (storage != null)
            {
                if (!storage.Save(scheduleName, trigger) && storage.Exception != null) logger(storage.Exception.FormatException(new NameValueCollection()
                {
                    ["TaskName"] = scheduleName,
                    ["TriggerName"] = trigger.Name,
                    ["TriggerType"] = trigger.GetType().Name
                }));
            }
        }
    }
}
