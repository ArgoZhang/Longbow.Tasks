using System;
using System.Collections.Generic;
using System.Threading;

namespace Longbow.Tasks
{
    /// <summary>
    /// 触发器接口
    /// </summary>
    public interface ITrigger
    {
        /// <summary>
        /// 获得/设置 触发器是否启用
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// 获得/设置 触发器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 触发器状态改变回调方法
        /// </summary>
        Action<bool>? EnabeldChanged { get; set; }

        /// <summary>
        /// 获得 任务开始时间
        /// </summary>
        DateTimeOffset? StartTime { get; }

        /// <summary>
        /// 获得 上次任务执行时间
        /// </summary>
        DateTimeOffset? LastRuntime { get; }

        /// <summary>
        /// 获得 下一次运行时间
        /// </summary>
        /// <returns></returns>
        DateTimeOffset? NextRuntime { get; }

        /// <summary>
        /// 获得/设置 上一次运行任务耗时
        /// </summary>
        TimeSpan LastRunElapsedTime { get; set; }

        /// <summary>
        /// 获得/设置 触发器上一次执行结果
        /// </summary>
        TriggerResult LastResult { get; set; }

        /// <summary>
        /// 获得 任务超时时间
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// 触发器执行情况回调方法
        /// </summary>
        Action<ITrigger>? PulseCallback { get; set; }

        /// <summary>
        /// 触发器 心跳 返回 true 时触发任务执行 同步阻塞线程方法 内部阻塞到 ITrigger 的下一次运行时间 内部调用外部代码不要调用
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>返回真时表示执行任务</returns>
        bool Pulse(CancellationToken cancellationToken = default);

        /// <summary>
        /// 设置序列化属性集合
        /// </summary>
        /// <returns></returns>
        Dictionary<string, object> SetData();

        /// <summary>
        /// 加载属性集合值
        /// </summary>
        /// <param name="datas"></param>
        void LoadData(Dictionary<string, object> datas);
    }
}
