﻿// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using Longbow.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 后台服务注入方法扩展类
/// </summary>
public static class TaskServiceCollectionExtensions
{
    /// <summary>
    /// 增加后台任务服务到容器中 <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="configure"></param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddTaskServices(this IServiceCollection services, Action<ITaskStorageBuilder>? configure = null)
    {
        // 创建服务
        services.TryAddSingleton<IStorage, NoneStorage>();
        services.TryAddSingleton<IConfigureOptions<TaskServicesOptions>, TaskServicesConfigureOptions<TaskServicesOptions>>();
        services.TryAddSingleton<IOptionsChangeTokenSource<TaskServicesOptions>, ConfigurationChangeTokenSource<TaskServicesOptions>>();
        services.AddHostedService<TaskServicesFactory>();
        configure?.Invoke(new TaskStorageBuilder(services));
        return services;
    }
}
