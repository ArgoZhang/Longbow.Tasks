// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

#if NETCOREAPP3_1
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
#endif
using Longbow.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Longbow.Tasks.TaskManagerTest;

namespace Longbow.Tasks
{
    [CollectionDefinition("TaskManagerContext")]
#if NETCOREAPP3_1
    public class TaskManagerContext : ICollectionFixture<TestWebHost<Startup>>
#else
    public class TaskManagerContext : ICollectionFixture<Startup>
#endif
    {

    }

    [Collection("TaskManagerContext")]
    public class TaskManagerTest : IDisposable
    {
        private static ITestOutputHelper _outputHelper;
        private static int InitCount;
        private static int InstanceCount;
        private static int ExecuteCount;
        private CancellationTokenSource _executeToken;

#if NETCOREAPP3_1
        public TaskManagerTest(TestWebHost<Startup> factory, ITestOutputHelper helper)
#else
        public TaskManagerTest(ITestOutputHelper helper)
#endif
        {
            InstanceCount = 0;
            ExecuteCount = 0;
            _outputHelper = helper;
#if NETCOREAPP3_1
            var client = factory.CreateDefaultClient();
#endif
            ResetToken();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ScheName_NullException(string scheName)
        {
            Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd(scheName, token => Task.CompletedTask));
            Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.Get(scheName));
            Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd(scheName, (ITask)null));
        }

        [Fact]
        public void MethodCall_NullException()
        {
            Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd("UnitTest_MethodCall_Exception", (ITask)null, null));
            Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd("UnitTest_MethodCall_Exception", (Func<CancellationToken, Task>)null, null));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void Log(string message) => _outputHelper?.WriteLine(message);

        [Fact]
        public void MultipleThreadTask_Ok()
        {
            // 多线程并发只初始化一个实例
            var task1 = Task.Run(() => CreateTask());
            var task2 = Task.Run(() => CreateTask());
            var task3 = Task.Run(() => CreateTask());
            Task.WaitAll(task1, task2, task3);
            Assert.Equal(1, InitCount);
            Assert.Equal(1, InstanceCount);
            Wait();
            Assert.Equal(1, ExecuteCount);
        }

        private class Foo2Task : ExecutableTask
        {
            public Foo2Task()
            {
                Command = @"C:\Windows\System32\NETSTAT.EXE";
                Arguments = "-an";
            }

            protected override void ConfigureStartInfo(ProcessStartInfo startInfo)
            {
                base.ConfigureStartInfo(startInfo);
                startInfo.UseShellExecute = true;
            }
        }

        [Fact]
        public async void ExecutableTask()
        {
            var sche = TaskServicesManager.GetOrAdd<Foo2Task>();
            await Task.Delay(300);
            await sche.Task.Execute(default);
            Assert.Null(sche.Exception);
        }

        [Fact]
        public void MethodCall_Ok()
        {
            var sche = TaskServicesManager.GetOrAdd("MethodCall", async token =>
            {
                try
                {
                    await Task.Delay(500, token);
                }
                catch (TaskCanceledException) { }
                if (!token.IsCancellationRequested)
                {
                    Interlocked.Increment(ref ExecuteCount);
                }
            });
            RegisterToken(sche);
            Assert.Equal(0, ExecuteCount);
            // 任务内部延时500毫秒，测试延时600毫秒 任务被执行 执行次数为1
            Wait();
            Assert.Equal(1, ExecuteCount);
        }

        [Fact]
        public void MethodCall_Exception()
        {
            var sche = TaskServicesManager.GetOrAdd("MethodCall_Exception", token =>
            {
                throw new Exception(nameof(MethodCall_Exception));
            });
            RegisterToken(sche);
            Wait(1000);
            Assert.NotNull(sche.Exception);
            Assert.Equal(nameof(MethodCall_Exception), sche.Exception.Message);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void RepeatTask_Ok()
        {
            var sche = CreateTask("Recurring", TriggerBuilder.Default.WithRepeatCount(2).WithInterval(TimeSpan.FromMilliseconds(500)).Build());

            Wait();
            Assert.Equal(1, InitCount);
            Assert.Equal(1, ExecuteCount);

            ResetToken();
            Wait();
            Assert.Equal(2, ExecuteCount);

            // 继续等待执行次数不会增加
            ResetToken();
            Wait(600);
            Assert.Equal(2, ExecuteCount);
            Assert.True(sche.Triggers.First().Enabled);
        }

        [Fact]
        public void Schedule_Equals()
        {
            var sche1 = TaskServicesManager.GetOrAdd<FooTask>();
            var sche2 = TaskServicesManager.GetOrAdd<FooTask>(null);
            var sche3 = TaskServicesManager.GetOrAdd<FooTask>(string.Empty);
            var sche4 = TaskServicesManager.GetOrAdd<FooTask>(nameof(FooTask));
            Assert.Same(sche1, sche2);
            Assert.Same(sche2, sche3);
            Assert.Same(sche3, sche4);
            Assert.True(sche1.CreatedTime < DateTimeOffset.Now);
        }

        [Fact]
        public void Schedule_Remove()
        {
            var sche1 = TaskServicesManager.GetOrAdd<FooTask>();
            TaskServicesManager.Remove(nameof(FooTask));
            Assert.DoesNotContain(TaskServicesManager.ToList(), sche => sche.Name == nameof(FooTask));
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Schedule_Stop()
        {
            var sche = CreateTask("Schedule", TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddSeconds(1)).WithRepeatCount(2).WithInterval(TimeSpan.FromMilliseconds(500)).Build());
            sche.Status = SchedulerStatus.Ready;
            Assert.Equal(1, InitCount);

            // 任务还未开始就停止调度了
            Assert.Equal(0, ExecuteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void Schedule_Disable()
        {
            var sche = TaskServicesManager.GetOrAdd<DelayTask>("Schedule_Disable");
            sche.Status = SchedulerStatus.Disabled;
        }

        [Fact]
        public void Schedule_NextRuntime()
        {
            var locker = new AutoResetEvent(false);
            var sche = TaskServicesManager.GetOrAdd("Schedule_Nexttime", token =>
            {
                locker.Set();
                return Task.CompletedTask;
            }, TriggerBuilder.Default.WithInterval(400).WithRepeatCount(10).Build());
            locker.WaitOne();

            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.NotNull(sche.NextRuntime);

            sche.Status = SchedulerStatus.Disabled;
            Assert.Null(sche.NextRuntime);
        }

        [Fact]
        public void Cancel_Ok()
        {
            var sche = CreateTask("Cancel", TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());

            Wait(300);
            // 任务已经开始执行内部延时500毫秒，此时调度停止执行次数为 0 任务被取消
            sche.Status = SchedulerStatus.Ready;
            Assert.Equal(0, ExecuteCount);
        }

        [Fact]
        public void Scheduler_Status()
        {
            var sche = CreateTask("Scheduler", TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());

            // 500 毫秒一个周期 任务内部延时 500 毫秒 所以 1 秒钟执行一次
            Wait();
            // 延时 1 秒后 任务执行一次
            Assert.Equal(1, ExecuteCount);

            sche.Status = SchedulerStatus.Disabled;
            ResetToken();
            Wait(300);
            // 延时 1 秒 由于 SchedulerStatus.Disabled 不执行任务
            Assert.Equal(1, ExecuteCount);
            Assert.Equal(SchedulerStatus.Disabled, sche.Status);

            sche.Status = SchedulerStatus.Ready;
            Assert.Equal(1, ExecuteCount);
            Assert.Equal(SchedulerStatus.Ready, sche.Status);

            sche.Status = SchedulerStatus.Running;
            ResetToken();
            Wait();
            Assert.Equal(2, ExecuteCount);
        }

        [Fact]
        public void Scheduler_Get()
        {
            var sche = CreateTask(nameof(Scheduler_Get));
            Assert.Same(sche, TaskServicesManager.Get(nameof(Scheduler_Get)));
            Assert.Null(TaskServicesManager.Get("UnitTest_None"));
        }

        [Fact]
        public void Scheduler_Task()
        {
            var sche = CreateTask(nameof(Scheduler_Task));
            Assert.NotNull(sche.Task);
        }

        [Fact]
        public void Schedule_Delay()
        {
            var startTime = DateTimeOffset.Now;
            var sche = CreateTask(nameof(Schedule_Delay), TriggerBuilder.Default.WithStartTime(startTime.AddMinutes(5)).WithInterval(TimeSpan.FromMinutes(5)).Build());
            Assert.Equal(startTime.AddMinutes(10), sche.NextRuntime);

            startTime = DateTimeOffset.Now;
            var sche1 = CreateTask("Schedule_Delay2", TriggerBuilder.Default.WithInterval(TimeSpan.FromMinutes(5)).Build());
            Assert.True(startTime.AddMinutes(5) <= sche1.NextRuntime);
        }

        [Fact]
        public void Trigger_Stop()
        {
            var sche = CreateTask("Trigger"); ;
            sche.Triggers.First().Enabled = false;

            // 任务内部延时500毫秒，任务被取消
            Assert.Equal(0, ExecuteCount);

            // 触发器被取消，调度仍然运行
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.Equal(TriggerResult.Cancelled, sche.Triggers.First().LastResult);
        }

        [Fact]
        public void DelayInit_Ok()
        {
            var sw = Stopwatch.StartNew();
            var sche = TaskServicesManager.GetOrAdd<DelayTask>(TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());
            RegisterToken(sche);
            sw.Stop();

            // 由于 DelayTask 内部构造函数线程休眠2秒 这里应该小于2秒才是合理的
            Assert.True(sw.Elapsed < TimeSpan.FromMilliseconds(2000));

            // 等待构造函数初始化完成
            Wait(2000);
            Assert.Equal(1, InitCount);
            Assert.Equal(1, InstanceCount);
            Assert.Equal(0, ExecuteCount);

            ResetToken();
            Wait();
            Assert.Equal(1, InitCount);
            Assert.Equal(1, InstanceCount);
            Assert.Equal(1, ExecuteCount);
        }

        [Fact]
        public void DelayInit_Cancel()
        {
            // 初始化时被Stop
            var sche = TaskServicesManager.GetOrAdd<DelayTask>("DelayTask_Cancel");
            sche.Status = SchedulerStatus.Ready;
            Assert.Equal(1, InitCount);
            Wait(2200);
            Assert.Equal(1, InstanceCount);
            Assert.Equal(0, ExecuteCount);
        }

        [Fact]
        public void Trigger_Enable()
        {
            var sche = CreateTask("Trigger_Enabled");
            sche.Triggers.First().Enabled = false;

            Assert.Equal(TriggerResult.Cancelled, sche.Triggers.First().LastResult);

            // 任务内部延时500毫秒，任务被取消
            Assert.Equal(0, ExecuteCount);

            // 触发器被禁止，调度仍然运行
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.False(sche.Triggers.First().Enabled);

            // 任务被执行
            Wait(200);

            // 设置 Enable = true 触发任务执行方法
            ResetToken();
            sche.Triggers.First().Enabled = true;
            Assert.True(sche.Triggers.First().Enabled);
            Assert.Equal(SchedulerStatus.Running, sche.Status);

            // 触发器开始运行
            // 由于之前运行过被取消。此时不运行
            Assert.Equal(0, ExecuteCount);
        }

        [Fact]
        public void Cron_Second()
        {
            var trigger = TriggerBuilder.Build(Cron.Secondly());
            var sche = CreateTask("Cron_Second", trigger);
            trigger.PulseCallback = t =>
            {
                Log($"{DateTimeOffset.Now}: Trigger PulseCallback({t.LastResult})");
                if (t.LastResult == TriggerResult.Success) _executeToken.Cancel();
            };
            Wait();
            Assert.Equal(1, InitCount);
            Assert.Equal(1, InstanceCount);
            Assert.Equal(1, ExecuteCount);

            // 复位
            ResetToken();
            Wait();
            Assert.Equal(1, InitCount);
            Assert.Equal(1, InstanceCount);
            Assert.Equal(2, ExecuteCount);
        }

        [Fact]
        public async void Task_Timeout()
        {
            var sche = TaskServicesManager.GetOrAdd("Recurring_Timeout", token => Task.Delay(500), TriggerBuilder.Default.WithInterval(300).WithRepeatCount(2).WithTimeout(200).Build());

            await Task.Delay(2000);
            Assert.Null(sche.NextRuntime);
            Assert.Equal(TriggerResult.Timeout, sche.Triggers.First().LastResult);
        }

        [Fact]
        public void Task_Exception()
        {
            var sche = TaskServicesManager.GetOrAdd(nameof(Task_Exception), token =>
            {
                throw new Exception("UnitTest");
            }, TriggerBuilder.Default.Build());

            // 设置 500 毫秒超时
            Wait(500);
            Assert.Equal(TriggerResult.Error, sche.Triggers.First().LastResult);
        }

        [Fact]
        public void WithStartTime_Ok()
        {
            var token1 = new CancellationTokenSource();
            var sche = TaskServicesManager.GetOrAdd("StartTime", async token =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                Interlocked.Increment(ref ExecuteCount);
                token1.Cancel();
            }, TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddMilliseconds(500)).Build());

            Assert.Equal(1, InitCount);
            Assert.Equal(0, ExecuteCount);

            token1.Token.WaitHandle.WaitOne();
            Assert.Equal(1, InitCount);
            Assert.Equal(1, ExecuteCount);

            var sche2 = CreateTask("prevTime", TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddSeconds(-5)).Build());
            Wait();
            Assert.Equal(1, InitCount);
            Assert.Equal(2, ExecuteCount);
        }

        [Fact]
        public void Runner_Performance()
        {
            var trigger = TriggerBuilder.Default.Build();
            var sw = Stopwatch.StartNew();
            var run = new Func<bool>(() =>
            {
                sw.Restart();
                var ret = trigger.Pulse();
                sw.Stop();
                Log($"Elapsed: {sw.Elapsed} NextRuntime: {trigger.NextRuntime}");
                return ret;
            });
            Assert.True(run());

            trigger = TriggerBuilder.Default.WithInterval(500).WithRepeatCount(2).Build();
            Assert.True(run());
            Assert.NotNull(trigger.NextRuntime);
            Assert.True(run());
            Assert.Null(trigger.NextRuntime);
            Assert.False(run());
            Assert.Null(trigger.NextRuntime);
        }

        [Fact]
        public void Trigger_NextRuntime()
        {
            // 间隔两秒执行2次任务
            var tri = TriggerBuilder.Default.WithInterval(500).WithRepeatCount(20).Build();
            var sche = CreateTask(nameof(Schedule_NextRuntime), tri);

            // 任务开始执行 调度下次时间不为空 触发器下次时间不为空
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.NotNull(sche.NextRuntime);
            Assert.NotNull(tri.NextRuntime);
            Wait();

            // 第一次任务执行完毕 调度状态为Running 调度下次时间不为空 触发器下次时间不为空
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.NotNull(sche.NextRuntime);
            Assert.NotNull(tri.NextRuntime);

            // 触发器被禁用 调度状态为运行 触发器被禁用 调度下次时间为空 触发器下次时间为空
            tri.Enabled = false;
            ResetToken();
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.False(tri.Enabled);
            Assert.Null(sche.NextRuntime);
            Assert.Null(tri.NextRuntime);
            Assert.False(Wait(500)); // 超时退出

            ResetToken();
            tri.Enabled = true;
            Wait();
            Assert.Equal(SchedulerStatus.Running, sche.Status);
            Assert.NotNull(tri.NextRuntime);
            Assert.NotNull(sche.NextRuntime);
        }

        private IScheduler CreateTask(string schedulerName = null, ITrigger trigger = null)
        {
            var sche = TaskServicesManager.GetOrAdd<FooTask>(schedulerName ?? nameof(FooTask), trigger);
            RegisterToken(sche);
            return sche;
        }

        private void RegisterToken(IScheduler sche)
        {
            sche.Triggers.First().PulseCallback = t =>
            {
                if (t.LastResult == TriggerResult.Success) _executeToken.Cancel();
            };
        }

        /// <summary>
        /// 执行取消 Token 复位
        /// </summary>
        private void ResetToken() => _executeToken = new CancellationTokenSource();

        /// <summary>
        /// 任务等待 任务执行完毕后未取消时调用 Cancel
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        private bool Wait(int millisecondsTimeout = -1) => _executeToken.Token.WaitHandle.WaitOne(millisecondsTimeout);

        public void Dispose()
        {
            _outputHelper = null;
            TaskServicesManager.Clear();
        }

        private class LongTask : FooTask
        {
            public override async Task Execute(CancellationToken cancellationToken)
            {
                await Task.Delay(2000, cancellationToken);
                await base.Execute(cancellationToken);
            }
        }

        private class FooTask : ITask
        {
            public FooTask()
            {
                Interlocked.Increment(ref InstanceCount);
            }

            public virtual async Task Execute(CancellationToken cancellationToken)
            {
                // 模拟任务执行耗时500毫秒
                try
                {
                    await Task.Delay(500, cancellationToken);
                }
                catch (TaskCanceledException) { }
                if (cancellationToken.IsCancellationRequested)
                {
                    Log($"{DateTimeOffset.Now}: FooTask Execute(Cancelled)");
                    return;
                }
                Interlocked.Increment(ref ExecuteCount);
                Log($"{DateTimeOffset.Now}: FooTask Execute(Success)");
            }
        }

        private class DelayTask : FooTask
        {
            public DelayTask() : base()
            {
                Thread.Sleep(2000);
            }
        }

        public class Startup
        {
            public Startup(IConfiguration configuration)
            {
                Interlocked.Increment(ref InitCount);
                Configuration = configuration;
            }

            public IConfiguration Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
                services.AddLogging(builder => builder.AddProvider(new LoggerProvider()).AddFileLogger());
                services.AddMvcCore();
                services.AddTaskServices();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseRouting();
                app.Run(context =>
                {
                    context.Response.Body.Write(Encoding.UTF8.GetBytes("UnitTest"));
                    return Task.CompletedTask;
                });
                app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
            }
        }
    }
}
