// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Longbow.Tasks.Test;

/// <summary>
/// 
/// </summary>
public class TestHost
{
    public TestHost()
    {
        var sc = new ServiceCollection();
        sc.AddLogging();

        sc.AddSingleton<IConfiguration>(sp =>
        {
            var configuration = new ConfigurationBuilder();
            configuration.AddInMemoryCollection();
            return configuration.Build();
        });
        sc.AddTaskServices(builder => builder.AddFileStorage<FileStorage>(op => op.DeleteFileByRemoveEvent = false));

        var provider = sc.BuildServiceProvider();
        var service = provider.GetRequiredService<IHostedService>();
        service.StartAsync(CancellationToken.None);
    }
}
