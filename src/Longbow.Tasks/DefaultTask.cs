using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// ITask 内部实现类
    /// </summary>
    internal class DefaultTask : ITask
    {
        private readonly Func<CancellationToken, Task> _methodCall;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="methodCall">匿名方法体</param>
        public DefaultTask(Func<CancellationToken, Task> methodCall)
        {
            _methodCall = methodCall;
        }

        /// <summary>
        /// 任务执行体
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        public Task Execute(CancellationToken cancellationToken) => _methodCall.Invoke(cancellationToken);
    }
}
