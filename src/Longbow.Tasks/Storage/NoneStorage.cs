// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Longbow.Tasks;

internal class NoneStorage : IStorage
{
    public Exception? Exception { get; }

    public Task LoadAsync() => Task.CompletedTask;

    public bool Remove(IEnumerable<string> schedulerNames) => true;

    public bool Save(string schedulerName, ITrigger trigger) => true;
}
