// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 可执行命令行任务单元实体类
    /// </summary>
    public class ExecutableTask : ITask
    {
        /// <summary>
        /// 获得/设置 可执行命令脚本
        /// </summary>
        public string Command { get; set; } = "";

        /// <summary>
        /// 获得/设置 可执行命令脚本参数
        /// </summary>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// 获得/设置 是否等待可直接脚本运行完毕 默认为 true
        /// </summary>
        public bool WaitForExit { get; set; } = true;

        /// <summary>
        /// 任务执行方法
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        [Obsolete("已过期，请使用 IServiceProvider 重载方法")]
        public Task Execute(CancellationToken cancellationToken)
        {
            var valid = !string.IsNullOrEmpty(Command) && File.Exists(Command);
            if (valid)
            {
                var startInfo = new ProcessStartInfo(Command, Arguments);
                ConfigureStartInfo(startInfo);
                var process = Process.Start(startInfo);
                if (process != null && WaitForExit) process.WaitForExit();
            }
            return Task.FromResult(valid);
        }

        public Task Execute(IServiceProvider provider, CancellationToken cancellationToken)
        {
            var valid = !string.IsNullOrEmpty(Command) && File.Exists(Command);
            if (valid)
            {
                var startInfo = new ProcessStartInfo(Command, Arguments);
                ConfigureStartInfo(provider, startInfo);
                var process = Process.Start(startInfo);
                if (process != null && WaitForExit) process.WaitForExit();
            }
            return Task.FromResult(valid);
        }

        /// <summary>
        /// 配置 ProcessStartInfo 实例
        /// </summary>
        /// <param name="startInfo"></param>
        [Obsolete("已过期，请使用 IServiceProvider 重载方法")]
        protected virtual void ConfigureStartInfo(ProcessStartInfo startInfo)
        {

        }

        /// <summary>
        /// 配置 ProcessStartInfo 实例
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="startInfo"></param>
        protected virtual void ConfigureStartInfo(IServiceProvider provider, ProcessStartInfo startInfo)
        {

        }
    }
}
