// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Longbow.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace Longbow.Tasks
{
    class TaskServicesFactoryLogger : ILogger<TaskServicesFactory>
    {
        private readonly ILogger _logger;

        public TaskServicesFactoryLogger()
        {
            _logger = new FileLoggerProvider(new FileLoggerOptions()).CreateLogger(nameof(TaskServicesFactory));
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope<TState>(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) => _logger.Log<TState>(logLevel, eventId, state, exception, formatter);
    }

}
