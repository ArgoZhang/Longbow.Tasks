#if NETCOREAPP3_1
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif
using static Longbow.Tasks.TaskStorageTest;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Longbow.Tasks
{
    [CollectionDefinition("TaskStorageContext")]
#if NETCOREAPP3_1
    public class TaskStorageContext : ICollectionFixture<TestWebHost<Startup>>
#else
    public class TaskStorageContext : ICollectionFixture<Startup>
#endif
    {

    }

    [Collection("TaskStorageContext")]
    public class TaskStorageTest : IDisposable
    {
#if NETCOREAPP3_1
        public TaskStorageTest(TestWebHost<Startup> factory)
        {
            var _ = factory.CreateDefaultClient();
            InitStorage();
        }
#else
        public TaskStorageTest(Startup factory)
        {
            InitStorage();
        }
#endif

        private void InitStorage()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TaskStorage");
            if (Directory.Exists(path))
            {
                Directory.EnumerateFiles(path, "*.bin", SearchOption.TopDirectoryOnly).ToList().ForEach(file =>
                {
                    File.Delete(file);
                });
            }
        }

        private AutoResetEvent locker = new AutoResetEvent(false);

        [Fact]
        public async void RunOnce_Ok()
        {
            var count = 0;
            var sche = TaskServicesManager.GetOrAdd("StorageRunOnce", token =>
            {
                count++;
                locker.Set();
                return Task.CompletedTask;
            });
            locker.WaitOne(500);

            // 第二次执行 由于持久化 任务体不被执行
            // 等待序列化完成
            await Task.Delay(500);
            TaskServicesManager.Clear();
            sche = TaskServicesManager.GetOrAdd("StorageRunOnce", token =>
            {
                count++;
                locker.Set();
                return Task.CompletedTask;
            });
            var state = locker.WaitOne(500);
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TaskStorage{Path.DirectorySeparatorChar}StorageRunOnce.bin");
            Assert.True(File.Exists(fileName));
            File.Delete(fileName);
            Assert.Equal(1, count);
            Assert.False(state);
        }

        [Fact]
        public async void Recurring_Ok()
        {
            var sche = TaskServicesManager.GetOrAdd("StorageRecurring", token =>
            {
                locker.Set();
                return Task.CompletedTask;
            }, TriggerBuilder.Default.WithInterval(500).WithStartTime(DateTimeOffset.Now).WithRepeatCount(10).Build());
            locker.WaitOne();
            await Task.Delay(300);

            TaskServicesManager.Clear();
            await Task.Delay(500);
            sche = TaskServicesManager.GetOrAdd("StorageRecurring", token =>
            {
                locker.Set();
                return Task.CompletedTask;
            }, TriggerBuilder.Default.WithInterval(500).WithStartTime(DateTimeOffset.Now).WithRepeatCount(10).Build());
            await Task.Delay(800);
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TaskStorage{Path.DirectorySeparatorChar}StorageRecurring.bin");

            // 循环任务
            Assert.True(File.Exists(fileName));
        }

        [Fact]
        public async void DeleteStorageFile_Ok()
        {
            var sche = TaskServicesManager.GetOrAdd("DeleteStorageFile", token =>
            {
                locker.Set();
                return Task.CompletedTask;
            });
            locker.WaitOne();
            await Task.Delay(300);

            // 利用发射获得 IStorage 实例
#if NETCOREAPP3_1
            var factory = typeof(TaskServicesManager).GetProperty("Factory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var instance = factory.GetValue(null, null);
#else
            var factory = typeof(TaskServicesManager).GetField("Factory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var instance = factory.GetValue(null);
#endif
            var storageInstance = instance.GetType().GetProperty("Storage").GetValue(instance);
            var option = storageInstance.GetType().GetProperty("Options").GetValue(storageInstance) as FileStorageOptions;
            option.DeleteFileByRemoveEvent = true;
            TaskServicesManager.Clear();
            option.DeleteFileByRemoveEvent = false;
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, option.Folder, "DeleteStorageFile.bin");
            Assert.False(File.Exists(fileName));
        }

        [Fact]
        public void TripleDES_Des()
        {
            var des = TripleDES.Create();
            des.GenerateIV();
            des.GenerateKey();

            //var key = des.Key;
            //var iv = des.IV;

            //var keyString = Convert.ToBase64String(key);
            //var ivString = Convert.ToBase64String(iv);

            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            des.IV = Convert.FromBase64String("rNWuCRQAWjI=");
            des.Key = Convert.FromBase64String("LIBSFjql+0qPjAjBaQYQ9Ka2oWkzR1j6");

            var encryptor = des.CreateEncryptor();
            var content = "606WGAeU+ohIIpze8mkGg/7X1FPQ0Ae+8DTSxopRB023YX8XpC9kRkT1u0PY7krb7iNNVXiwUxlSR7pDnLo+3nkgbjLYk7/f+4msdzxCL11HKIvHvwl2C2R3xb8c4vVT1fXafAY3UqlsJT7mmWIylG8Ed8oUkWHQd6qyewgAHi8q9Wacv+z0QBdVta5UAZmerDDgqQhTeSuRLL9Bb1z4qd08MB82tVWUDXsZ+Hs11fPON+aQxh8aHz3yM9JpGtRIz/SQunBoQUROoOH/ElYXYwct7hX9V1bGQ3RzRiuUfYzYywFcZ6vBBOPCFrxqcevDQmJMh4GdRIqyWWe5IBaRNnAubZwd6eBiSao6uupNvkUqooM1wgx+N+rhNYmHTC99yI1ksf9kX0mkrQCOINOSuAmTEKfvQNeEUerPd1u3d3qi0OaVIPP7Qqd/9SU7jIMIBy3uFf1XVsb32lLwG49MuDWNZDrUAbUO/88AJxuQrXvAh/6CmDpD3HPQoXlNVR1VbXs9odVgs6h3ob+qyyN5UYA2fUriJudONlL5Stfbl76VMTVm/Tc4QAyGKoq4UgZxVs6d4k+WeX0M67Goen33OUZif2qkSx7w";
            var data = Encoding.UTF8.GetBytes(content);
            var buffer = encryptor.TransformFinalBlock(data, 0, data.Length);

            des = TripleDES.Create();
            des.IV = Convert.FromBase64String("rNWuCRQAWjI=");
            des.Key = Convert.FromBase64String("LIBSFjql+0qPjAjBaQYQ9Ka2oWkzR1j6");
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            var decryptor = des.CreateDecryptor();
            var buffer2 = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer2);
            Assert.Equal(content, result);
        }

        public void Dispose()
        {

        }

        public class Startup
        {
#if NETCOREAPP3_1
            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
                Configuration["TaskServicesOptions:FileStorageOptions:Enabled"] = "true";
            }

            public IConfiguration Configuration { get; }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddLogging(builder => builder.AddFileLogger());
                services.AddTaskServices(builder => builder.AddFileStorage(op => op.DeleteFileByRemoveEvent = false));
                services.AddControllers();
                services.AddRouting();
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app)
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
            }
#else
            public Startup()
            {
                TaskServicesManager.Init(options: new TaskServicesOptions(), storage: new FileStorage(new FileStorageOptions() { DeleteFileByRemoveEvent = false }));
            }
#endif
        }
    }
}
