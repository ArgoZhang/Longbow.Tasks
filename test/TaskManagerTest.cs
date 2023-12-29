// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

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

namespace Longbow.Tasks.Test;

[CollectionDefinition("TaskManagerContext")]
public class TaskManagerContext : ICollectionFixture<TestHost>
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

    public TaskManagerTest(ITestOutputHelper helper)
    {
        InstanceCount = 0;
        ExecuteCount = 0;
        _outputHelper = helper;
        ResetToken();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ScheduleName_NullException(string scheduleName)
    {
        Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd(scheduleName, (provider, token) => Task.CompletedTask));
        Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.Get(scheduleName));
        Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd(scheduleName, (ITask)null));
    }

    [Fact]
    public void MethodCall_NullException()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd("UnitTest_MethodCall_Exception", (ITask)null, null));
        Assert.ThrowsAny<ArgumentNullException>(() => TaskServicesManager.GetOrAdd("UnitTest_MethodCall_Exception", (Func<IServiceProvider, CancellationToken, Task>)null, null));
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

        protected override void ConfigureStartInfo(IServiceProvider provider, ProcessStartInfo startInfo)
        {
            base.ConfigureStartInfo(provider, startInfo);
            startInfo.UseShellExecute = true;
        }
    }

    [Fact]
    public async void ExecutableTask()
    {
        var schedule = TaskServicesManager.GetOrAdd<Foo2Task>();
        await Task.Delay(300);
        await schedule.Task.Execute(null, default);
        Assert.Null(schedule.Exception);
    }

    [Fact]
    public void MethodCall_Ok()
    {
        var scheduler = TaskServicesManager.GetOrAdd("MethodCall", async (provider, token) =>
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
        RegisterToken(scheduler);
        Assert.Equal(0, ExecuteCount);
        // 任务内部延时500毫秒，测试延时600毫秒 任务被执行 执行次数为1
        Wait();
        Assert.Equal(1, ExecuteCount);
    }

    [Fact]
    public void MethodCall_Exception()
    {
        var scheduler = TaskServicesManager.GetOrAdd("MethodCall_Exception", (provider, token) =>
        {
            throw new Exception(nameof(MethodCall_Exception));
        });
        RegisterToken(scheduler);
        Wait(1000);
        Assert.NotNull(scheduler.Exception);
        Assert.Equal(nameof(MethodCall_Exception), scheduler.Exception.Message);
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void RepeatTask_Ok()
    {
        var scheduler = CreateTask("Recurring", TriggerBuilder.Default.WithRepeatCount(2).WithInterval(TimeSpan.FromMilliseconds(500)).Build());

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
        Assert.True(scheduler.Triggers.First().Enabled);
    }

    [Fact]
    public void Schedule_Equals()
    {
        var scheduler1 = TaskServicesManager.GetOrAdd<FooTask>();
        var scheduler2 = TaskServicesManager.GetOrAdd<FooTask>(null);
        var scheduler3 = TaskServicesManager.GetOrAdd<FooTask>(string.Empty);
        var scheduler4 = TaskServicesManager.GetOrAdd<FooTask>(nameof(FooTask));
        Assert.Same(scheduler1, scheduler2);
        Assert.Same(scheduler2, scheduler3);
        Assert.Same(scheduler3, scheduler4);
        Assert.True(scheduler1.CreatedTime < DateTimeOffset.Now);
    }

    [Fact]
    public void Schedule_Remove()
    {
        var scheduler1 = TaskServicesManager.GetOrAdd<FooTask>();
        TaskServicesManager.Remove(nameof(FooTask));
        Assert.DoesNotContain(TaskServicesManager.ToList(), scheduler => scheduler.Name == nameof(FooTask));
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void Schedule_Stop()
    {
        var scheduler = CreateTask("Schedule", TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddSeconds(1)).WithRepeatCount(2).WithInterval(TimeSpan.FromMilliseconds(500)).Build());
        scheduler.Status = SchedulerStatus.Ready;
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
        var scheduler = TaskServicesManager.GetOrAdd<DelayTask>("Schedule_Disable");
        scheduler.Status = SchedulerStatus.Disabled;
    }

    [Fact]
    public void Schedule_NextRuntime()
    {
        var locker = new AutoResetEvent(false);
        var scheduler = TaskServicesManager.GetOrAdd("Schedule_NextTime", (provider, token) =>
        {
            locker.Set();
            return Task.CompletedTask;
        }, TriggerBuilder.Default.WithInterval(400).WithRepeatCount(10).Build());
        locker.WaitOne();

        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.NotNull(scheduler.NextRuntime);

        scheduler.Status = SchedulerStatus.Disabled;
        Assert.Null(scheduler.NextRuntime);
    }

    [Fact]
    public void Cancel_Ok()
    {
        var scheduler = CreateTask("Cancel", TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());

        Wait(300);
        // 任务已经开始执行内部延时500毫秒，此时调度停止执行次数为 0 任务被取消
        scheduler.Status = SchedulerStatus.Ready;
        Assert.Equal(0, ExecuteCount);
    }

    [Fact]
    public void Scheduler_Status()
    {
        var scheduler = CreateTask("Scheduler", TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());

        // 500 毫秒一个周期 任务内部延时 500 毫秒 所以 1 秒钟执行一次
        Wait();
        // 延时 1 秒后 任务执行一次
        Assert.Equal(1, ExecuteCount);

        scheduler.Status = SchedulerStatus.Disabled;
        ResetToken();
        Wait(300);
        // 延时 1 秒 由于 SchedulerStatus.Disabled 不执行任务
        Assert.Equal(1, ExecuteCount);
        Assert.Equal(SchedulerStatus.Disabled, scheduler.Status);

        scheduler.Status = SchedulerStatus.Ready;
        Assert.Equal(1, ExecuteCount);
        Assert.Equal(SchedulerStatus.Ready, scheduler.Status);

        scheduler.Status = SchedulerStatus.Running;
        ResetToken();
        Wait();
        Assert.Equal(2, ExecuteCount);
    }

    [Fact]
    public void Scheduler_Get()
    {
        var scheduler = CreateTask(nameof(Scheduler_Get));
        Assert.Same(scheduler, TaskServicesManager.Get(nameof(Scheduler_Get)));
        Assert.Null(TaskServicesManager.Get("UnitTest_None"));
    }

    [Fact]
    public void Scheduler_Task()
    {
        var scheduler = CreateTask(nameof(Scheduler_Task));
        Assert.NotNull(scheduler.Task);
    }

    [Fact]
    public void Schedule_Delay()
    {
        var startTime = DateTimeOffset.Now;
        var scheduler = CreateTask(nameof(Schedule_Delay), TriggerBuilder.Default.WithStartTime(startTime.AddMinutes(5)).WithInterval(TimeSpan.FromMinutes(5)).Build());
        Assert.Equal(startTime.AddMinutes(10), scheduler.NextRuntime);

        startTime = DateTimeOffset.Now;
        var scheduler1 = CreateTask("Schedule_Delay2", TriggerBuilder.Default.WithInterval(TimeSpan.FromMinutes(5)).Build());
        Assert.True(startTime.AddMinutes(5) <= scheduler1.NextRuntime);
    }

    [Fact]
    public void Trigger_Stop()
    {
        var scheduler = CreateTask("Trigger"); ;
        scheduler.Triggers.First().Enabled = false;

        // 任务内部延时500毫秒，任务被取消
        Assert.Equal(0, ExecuteCount);

        // 触发器被取消，调度仍然运行
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.Equal(TriggerResult.Cancelled, scheduler.Triggers.First().LastResult);
    }

    [Fact]
    public void DelayInit_Ok()
    {
        var sw = Stopwatch.StartNew();
        var scheduler = TaskServicesManager.GetOrAdd<DelayTask>(TriggerBuilder.Default.WithInterval(TimeSpan.FromMilliseconds(500)).Build());
        RegisterToken(scheduler);
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
        var scheduler = TaskServicesManager.GetOrAdd<DelayTask>("DelayTask_Cancel");
        scheduler.Status = SchedulerStatus.Ready;
        Assert.Equal(1, InitCount);
        Wait(2200);
        Assert.Equal(1, InstanceCount);
        Assert.Equal(0, ExecuteCount);
    }

    [Fact]
    public void Trigger_Enable()
    {
        var scheduler = CreateTask("Trigger_Enabled");
        scheduler.Triggers.First().Enabled = false;

        Assert.Equal(TriggerResult.Cancelled, scheduler.Triggers.First().LastResult);

        // 任务内部延时500毫秒，任务被取消
        Assert.Equal(0, ExecuteCount);

        // 触发器被禁止，调度仍然运行
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.False(scheduler.Triggers.First().Enabled);

        // 任务被执行
        Wait(200);

        // 设置 Enable = true 触发任务执行方法
        ResetToken();
        scheduler.Triggers.First().Enabled = true;
        Assert.True(scheduler.Triggers.First().Enabled);
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);

        // 触发器开始运行
        // 由于之前运行过被取消。此时不运行
        Assert.Equal(0, ExecuteCount);
    }

    [Fact]
    public void Cron_Second()
    {
        var trigger = TriggerBuilder.Build(Cron.Secondly());
        var scheduler = CreateTask("Cron_Second", trigger);
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
        var scheduler = TaskServicesManager.GetOrAdd("Recurring_Timeout", (provider, token) => Task.Delay(500), TriggerBuilder.Default.WithInterval(300).WithRepeatCount(2).WithTimeout(200).Build());

        await Task.Delay(2000);
        Assert.Null(scheduler.NextRuntime);
        Assert.Equal(TriggerResult.Timeout, scheduler.Triggers.First().LastResult);
    }

    [Fact]
    public void Task_Exception()
    {
        var scheduler = TaskServicesManager.GetOrAdd(nameof(Task_Exception), (provider, token) =>
        {
            throw new Exception("UnitTest");
        }, TriggerBuilder.Default.Build());

        // 设置 500 毫秒超时
        Wait(500);
        Assert.Equal(TriggerResult.Error, scheduler.Triggers.First().LastResult);
    }

    [Fact]
    public void WithStartTime_Ok()
    {
        var token1 = new CancellationTokenSource();
        var scheduler = TaskServicesManager.GetOrAdd("StartTime", async (provider, token) =>
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

        var scheduler2 = CreateTask("prevTime", TriggerBuilder.Default.WithStartTime(DateTimeOffset.Now.AddSeconds(-5)).Build());
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
        var scheduler = CreateTask(nameof(Schedule_NextRuntime), tri);

        // 任务开始执行 调度下次时间不为空 触发器下次时间不为空
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.NotNull(scheduler.NextRuntime);
        Assert.NotNull(tri.NextRuntime);
        Wait();

        // 第一次任务执行完毕 调度状态为Running 调度下次时间不为空 触发器下次时间不为空
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.NotNull(scheduler.NextRuntime);
        Assert.NotNull(tri.NextRuntime);

        // 触发器被禁用 调度状态为运行 触发器被禁用 调度下次时间为空 触发器下次时间为空
        tri.Enabled = false;
        ResetToken();
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.False(tri.Enabled);
        Assert.Null(scheduler.NextRuntime);
        Assert.Null(tri.NextRuntime);
        Assert.False(Wait(500)); // 超时退出

        ResetToken();
        tri.Enabled = true;
        Wait();
        Assert.Equal(SchedulerStatus.Running, scheduler.Status);
        Assert.NotNull(tri.NextRuntime);
        Assert.NotNull(scheduler.NextRuntime);
    }

    private IScheduler CreateTask(string schedulerName = null, ITrigger trigger = null)
    {
        var scheduler = TaskServicesManager.GetOrAdd<FooTask>(schedulerName ?? nameof(FooTask), trigger);
        RegisterToken(scheduler);
        return scheduler;
    }

    private void RegisterToken(IScheduler scheduler)
    {
        scheduler.Triggers.First().PulseCallback = t =>
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

        public virtual async Task Execute(IServiceProvider provider, CancellationToken cancellationToken)
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
