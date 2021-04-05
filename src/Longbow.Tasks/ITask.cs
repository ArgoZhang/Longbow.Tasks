using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 任务类接口
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 任务执行操作方法
        /// </summary>
        /// <param name="cancellationToken">CancellationToken 实例</param>
        Task Execute(CancellationToken cancellationToken);
    }
}
