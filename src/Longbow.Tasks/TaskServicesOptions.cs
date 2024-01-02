// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Longbow.Tasks;

/// <summary>
/// 后台任务服务类配置类
/// </summary>
public class TaskServicesOptions
{
    /// <summary>
    /// 获得/设置 关闭服务超时时长
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// 缓存配置类
/// </summary>
/// <typeparam name="TOptions"></typeparam>
internal class TaskServicesConfigureOptions<TOptions> : ConfigureFromConfigurationOptions<TOptions> where TOptions : class
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="configuration"></param>
    public TaskServicesConfigureOptions(IConfiguration configuration)
        : base(configuration.GetSection("TaskServicesOptions"))
    {

    }
}
