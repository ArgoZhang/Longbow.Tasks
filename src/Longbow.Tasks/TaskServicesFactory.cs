// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Longbow.Tasks
{
    /// <summary>
    /// 后台服务工厂操作类
    /// </summary>
    internal sealed class TaskServicesFactory : IDisposable, IHostedService
    {
        private readonly CancellationTokenSource _shutdownCts = new();

        private TaskServicesOptions _options;
        internal static TaskServicesFactory? Instance { get; set; }
        /// <summary>
        /// 默认构造函数
        /// </summary>
        /// <param name="logger">ILogger(TaskServicesFactory) 实例</param>
        /// <param name="options">后台服务配置类实例</param>
        /// <param name="storage">IStorage 任务持久化接口实例</param>
        public TaskServicesFactory(ILogger<TaskServicesFactory> logger, IOptionsMonitor<TaskServicesOptions> options, IStorage storage)
        {
            Logger = logger;
            _options = options.CurrentValue;
            options.OnChange(op => _options = op);
            Storage = storage;
            Instance = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        internal static void Create(TaskServicesOptions? options, IStorage storage)
        {
            var op = new OptionsMonitorTaskServicesOptions(options ?? new TaskServicesOptions());
            var logger = new TaskServicesFactoryLogger();
            var _ = new TaskServicesFactory(logger, op, storage);
        }

        /// <summary>
        /// 获得 日志实例
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// 获得 任务持久化配置项实例
        /// </summary>
        public IStorage Storage { get; }

        /// <summary>
        /// IHostedService 接口异步开始方法
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            Log($"{nameof(TaskServicesFactory)} StartAsync() Started");

            // load task from storage
            await Storage.LoadAsync();
        }

        /// <summary>
        /// IHostedService 接口异步结束方法
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            Log($"{nameof(TaskServicesFactory)} -> {nameof(StopAsync)}() Shutdown After({_options.ShutdownTimeout})");
            _shutdownCts.CancelAfter(_options.ShutdownTimeout);
            WaitForShutdown(CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, cancellationToken).Token);
            return Task.CompletedTask;
        }

        private void WaitForShutdown(CancellationToken token)
        {
            TaskServicesManager.Shutdown(token);
            Log($"{nameof(TaskServicesFactory)} WaitForShutdown()");
        }

        /// <summary>
        /// 日志记录方法
        /// </summary>
        /// <param name="message">日志内容</param>
        public void Log(string message)
        {
            Logger.Log(LogLevel.Information, $"{DateTimeOffset.Now}: {message}");
        }

        /// <summary>
        /// Dispose 方法
        /// </summary>
        public void Dispose()
        {
            Log($"{nameof(TaskServicesFactory)} Disposed");
            _shutdownCts?.Dispose();
        }
    }
}
