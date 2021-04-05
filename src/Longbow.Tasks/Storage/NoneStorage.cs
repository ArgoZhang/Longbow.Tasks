using System;
using System.Collections.Generic;

namespace Longbow.Tasks
{
    internal class NoneStorage : IStorage
    {
        public Exception? Exception { get; }

        public bool Load(string schedulerName, ITrigger trigger) => true;

        public bool Remove(IEnumerable<string> schedulerNames) => true;

        public bool Save(string schedulerName, ITrigger trigger) => true;
    }
}
