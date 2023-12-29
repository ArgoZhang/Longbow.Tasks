// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Longbow.Tasks.Test;

/// <summary>
/// 
/// </summary>
public class TestHost
{
    public TestHost()
    {
        TaskServicesManager.Init();
    }
}
