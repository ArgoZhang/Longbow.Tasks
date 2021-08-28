// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    internal class DefaultTaskMetaData
    {
        /// <summary>
        /// 
        /// </summary>
        public DefaultTaskMetaData(ITask task)
        {
            Task = task;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITask Task { get; }

        /// <summary>
        /// 任务执行操作方法
        /// </summary>
        /// <param name="cancellationToken">CancellationToken 实例</param>
        public Task Execute(CancellationToken cancellationToken) => Task.Execute(cancellationToken);
    }
}
