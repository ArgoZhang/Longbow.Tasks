// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.Options;
using System;

namespace Longbow.Tasks;

class OptionsMonitorTaskServicesOptions : IOptionsMonitor<TaskServicesOptions>
{
    public OptionsMonitorTaskServicesOptions(TaskServicesOptions op)
    {
        CurrentValue = op;
    }

    public TaskServicesOptions CurrentValue { get; }

#if NET6_0
    public TaskServicesOptions Get(string name) => CurrentValue;
#else
    public TaskServicesOptions Get(string? name) => CurrentValue;
#endif
    public IDisposable OnChange(Action<TaskServicesOptions, string> listener) => null!;
}
