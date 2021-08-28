// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 持久化接口
    /// </summary>
    public interface IStorage
    {
        /// <summary>
        /// 从持久化载体加载 ITrigger 触发器
        /// </summary>
        /// <returns></returns>
        Task LoadAsync();

        /// <summary>
        /// 将 ITrigger 触发器保存到序列化载体中
        /// </summary>
        /// <param name="schedulerName">任务调度器名称</param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        bool Save(string schedulerName, ITrigger trigger);

        /// <summary>
        /// 获得 上一次操作异常信息实例
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// 移除指定任务调度
        /// </summary>
        /// <param name="schedulerNames">要移除调度名称集合</param>
        bool Remove(IEnumerable<string> schedulerNames);
    }
}
