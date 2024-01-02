// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Cronos;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Longbow.Tasks;

/// <summary>
/// Cron 表达式触发器
/// </summary>
internal class CronTrigger : DefaultTrigger
{
    /// <summary>
    /// 获得 Cron 字符串表达式
    /// </summary>
    public string Cron { get; protected set; }

    /// <summary>
    /// 获得 Cron 表达式
    /// </summary>
    public CronExpression CronExpression { get; protected set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cronExpress"></param>
    public CronTrigger(string cronExpress)
    {
        Cron = cronExpress;
        CronExpression = cronExpress.ParseCronExpression();
    }

    /// <summary>
    /// 触发器 心跳 返回 true 时触发任务执行 同步阻塞线程方法 内部阻塞到 ITrigger 的下一次运行时间
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>返回真时表示执行任务</returns>
    public override bool Pulse(CancellationToken cancellationToken = default)
    {
        bool ret = false;
        var nextTime = CronExpression.GetNextExecution(DateTimeOffset.Now);
        if (nextTime != null)
        {
            // 等待时间间隔周期
            var interval = nextTime.Value - DateTimeOffset.Now;
            ret = !cancellationToken.WaitHandle.WaitOne(interval);
            NextRuntime = CronExpression.GetNextExecution(nextTime.Value);
            LastRuntime = DateTimeOffset.Now;
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
        data.Add("Cron", Cron);
        return data;
    }

    /// <summary>
    /// 加载序列化属性集合值方法
    /// </summary>
    /// <param name="datas"></param>
    public override void LoadData(Dictionary<string, object> datas)
    {
        base.LoadData(datas);
        if (datas.TryGetValue("Cron", out var cron))
        {
            var express = cron.ToString();
            if (!string.IsNullOrEmpty(express)) CronExpression = express.ParseCronExpression();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Cron;
}
