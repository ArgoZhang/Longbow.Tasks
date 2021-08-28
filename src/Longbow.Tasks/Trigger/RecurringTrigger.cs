// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Longbow.Tasks
{
    internal class RecurringTrigger : DefaultTrigger
    {
        /// <summary>
        /// 获得/设置 重复间隔
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// 获得/设置 重复次数
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>
        /// 获得/设置 当前次数
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// 触发器 心跳 返回 true 时触发任务执行 同步阻塞线程方法 内部阻塞到 ITrigger 的下一次运行时间
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>返回真时表示执行任务</returns>
        public override bool Pulse(CancellationToken cancellationToken = default)
        {
            if (CurrentCount >= RepeatCount && RepeatCount > 0) return false;

            bool ret = false;
            if (Interval > TimeSpan.Zero)
            {
                // 先计算下一次运行时间
                if (!cancellationToken.WaitHandle.WaitOne(Interval))
                {
                    LastRuntime = DateTimeOffset.Now;
                    if (RepeatCount > 0) CurrentCount++;
                    NextRuntime = RepeatCount == 0 || CurrentCount < RepeatCount ? DateTimeOffset.Now.Add(Interval) : (DateTimeOffset?)null;
                    ret = true;
                }
            }
            return ret;
        }

        /// <summary>
        /// 设置序列化属性集合方法
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, object> SetData()
        {
            var data = base.SetData();
            data.Add(nameof(CurrentCount), CurrentCount);
            data.Add(nameof(RepeatCount), RepeatCount);
            data.Add(nameof(Interval), Interval.TotalSeconds);
            return data;
        }

        /// <summary>
        /// 加载序列化属性集合值方法
        /// </summary>
        /// <param name="datas"></param>
        public override void LoadData(Dictionary<string, object> datas)
        {
            base.LoadData(datas);
            if (int.TryParse(datas[nameof(CurrentCount)].ToString(), out var count)) CurrentCount = count;
            if (int.TryParse(datas[nameof(RepeatCount)].ToString(), out var repeatCount)) RepeatCount = repeatCount;
            if (double.TryParse(datas[nameof(Interval)].ToString(), out var interval)) Interval = TimeSpan.FromSeconds(interval);
        }

        /// <summary>
        /// 重载 ToString 方法
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"RepeatCount({RepeatCount} Interval({Interval}) Trigger";
    }
}
