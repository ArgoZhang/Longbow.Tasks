// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

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

    public TaskServicesOptions Get(string? name) => CurrentValue;

    public IDisposable OnChange(Action<TaskServicesOptions, string> listener) => null!;
}
