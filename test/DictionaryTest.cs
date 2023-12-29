// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Longbow.Tasks.Test;

public class DictionaryTest
{
    private readonly ITestOutputHelper _helper;

    public DictionaryTest(ITestOutputHelper helper)
    {
        _helper = helper;
    }

    [Fact]
    public void AddOrUpdate_Bad()
    {
        var pool = new ConcurrentDictionary<string, string>();
        var tasks = Enumerable.Range(1, 5).Select(i => Task.Run(() =>
        {
            pool.AddOrUpdate("Test", key =>
            {
                var t = GenerateValue(i);
                t.Wait();
                return t.Result;
            }, (key, value) =>
            {
                var t = UpdateValue(i);
                t.Wait();
                return t.Result;
            });
        }));

        _helper.WriteLine($"Total Tasks: {tasks.Count()}");
        Task.WaitAll(tasks.ToArray());
    }

    private async Task<string> GenerateValue(int i)
    {
        _helper.WriteLine($"Loop {i}: {nameof(GenerateValue)}");
        await Task.Delay(2000);
        _helper.WriteLine($"Loop {i}: {nameof(GenerateValue)} {DateTime.Now}");
        return DateTime.Now.ToString();
    }

    private async Task<string> UpdateValue(int i)
    {
        _helper.WriteLine($"Loop {i}: {nameof(UpdateValue)}");
        await Task.Delay(2000);
        _helper.WriteLine($"Loop {i}: {nameof(UpdateValue)} {DateTime.Now}");
        return DateTime.Now.ToString();
    }

    [Fact]
    public void AddOrUpdate_Good()
    {
        var pool = new ConcurrentDictionary<string, Lazy<string>>();
        var tasks = Enumerable.Range(1, 5).Select(i => Task.Run(() =>
        {
            var temp = pool.AddOrUpdate("Test", key => new Lazy<string>(() =>
            {
                var t = GenerateValue(i);
                t.Wait();
                return t.Result;
            }), (key, value) => new Lazy<string>(() =>
            {
                var t = UpdateValue(i);
                t.Wait();
                return t.Result;
            }));
        }));

        _helper.WriteLine($"Total Tasks: {tasks.Count()}");
        Task.WaitAll(tasks.ToArray());
        _helper.WriteLine(pool["Test"].Value);
    }
}
