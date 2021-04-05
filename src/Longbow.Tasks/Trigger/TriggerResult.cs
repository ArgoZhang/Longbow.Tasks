namespace Longbow.Tasks
{
    /// <summary>
    /// 触发器执行结果
    /// </summary>
    public enum TriggerResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// 超时
        /// </summary>
        Timeout = 2,

        /// <summary>
        /// 故障
        /// </summary>
        Error = 3
    }
}
