// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Longbow.Tasks.Test;

[CollectionDefinition("TaskStorageContext")]
public class TaskStorageContext : ICollectionFixture<TestHost>
{

}

[Collection("TaskStorageContext")]
public class TaskStorageTest
{
    public TaskStorageTest(TestHost factory)
    {
        InitStorage();
    }

    private static void InitStorage()
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

    private readonly AutoResetEvent locker = new(false);

    [Fact]
    public async Task RunOnce_Ok()
    {
        var count = 0;
        var scheduler = TaskServicesManager.GetOrAdd("StorageRunOnce", (provider, token) =>
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
        scheduler = TaskServicesManager.GetOrAdd("StorageRunOnce", (provider, token) =>
        {
            count++;
            locker.Set();
            return Task.CompletedTask;
        });
        var state = locker.WaitOne(500);
        var fileName = Path.Combine(AppContext.BaseDirectory, $"TaskStorage{Path.DirectorySeparatorChar}StorageRunOnce.bin");
        Assert.False(File.Exists(fileName));
        Assert.Equal(2, count);
        Assert.True(state);
    }

    [Fact]
    public async Task Recurring_Ok()
    {
        var scheduler = TaskServicesManager.GetOrAdd("StorageRecurring", (provider, token) =>
        {
            locker.Set();
            return Task.CompletedTask;
        }, TriggerBuilder.Default.WithInterval(500).WithStartTime(DateTimeOffset.Now).WithRepeatCount(10).Build());
        locker.WaitOne();
        await Task.Delay(300);

        TaskServicesManager.Clear();
        await Task.Delay(500);
        scheduler = TaskServicesManager.GetOrAdd("StorageRecurring", (provider, token) =>
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
        var scheduler = TaskServicesManager.GetOrAdd("DeleteStorageFile", (provider, token) =>
        {
            locker.Set();
            return Task.CompletedTask;
        });
        locker.WaitOne();
        await Task.Delay(300);

        // 利用发射获得 IStorage 实例
        var factory = typeof(TaskServicesManager).GetProperty("Factory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var instance = factory.GetValue(null, null);
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
}
