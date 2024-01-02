// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Cronos;
using System;

namespace Longbow.Tasks;

/// <summary>
/// Cron 表达式生成操作 (支持秒)
/// </summary>
public static class Cron
{
    /// <summary>
    /// 返回每秒 Cron 表达式
    /// </summary>
    /// <returns></returns>
    public static string Secondly()
    {
        return "* * * * * *";
    }

    /// <summary>
    /// 返回指定秒间隔的 Cron 表达式
    /// </summary>
    /// <param name="second">指定秒数</param>
    /// <returns></returns>
    public static string Secondly(int second)
    {
        return $"*/{second} * * * * *";
    }

    /// <summary>
    /// 返回每分钟 Cron 表达式
    /// </summary>
    public static string Minutely()
    {
        return "* * * * *";
    }

    /// <summary>
    /// 返回指定分钟间隔的 Cron 表达式
    /// </summary>
    /// <param name="minute">指定分钟数</param>
    public static string Minutely(int minute)
    {
        return $"*/{minute} * * * *";
    }

    /// <summary>
    /// 返回指定分钟与秒的 Cron 表达式
    /// </summary>
    /// <param name="minute">分钟数</param>
    /// <param name="second">秒数</param>
    public static string Minutely(int minute, int second)
    {
        return $"{second} */{minute} * * * *";
    }

    /// <summary>
    /// 返回每小时的 Cron 表达式
    /// </summary>
    public static string Hourly()
    {
        return "0 * * * *";
    }

    /// <summary>
    /// 返回指定小时的 Cron 表达式
    /// </summary>
    /// <param name="hour">小时数</param>
    public static string Hourly(int hour)
    {
        return $"0 */{hour} * * *";
    }

    /// <summary>
    /// 返回指定小时分钟的 Cron 表达式
    /// </summary>
    /// <param name="hour">小时数</param>
    /// <param name="minute">分钟数</param>
    /// <returns></returns>
    public static string Hourly(int hour, int minute)
    {
        return $"{minute} */{hour} * * *";
    }

    /// <summary>
    /// 返回指定小时分钟秒的 Cron 表达式
    /// </summary>
    /// <param name="hour">小时数</param>
    /// <param name="minute">分钟数</param>
    /// <param name="second">秒数</param>
    /// <returns></returns>
    public static string Hourly(int hour, int minute, int second)
    {
        return $"{second} {minute} */{hour} * * *";
    }

    /// <summary>
    /// 返回每天的 Cron 表达式
    /// </summary>
    public static string Daily()
    {
        return "0 0 * * *";
    }

    /// <summary>
    /// 返回指定天数的 Cron 表达式
    /// </summary>
    /// <param name="day">指定天数</param>
    public static string Daily(int day)
    {
        return $"0 0 */{day} * *";
    }

    /// <summary>
    /// 返回指定天数的 Cron 表达式
    /// </summary>
    /// <param name="day">指定天数</param>
    /// <param name="hour">指定小时数</param>
    public static string Daily(int day, int hour)
    {
        return $"0 {hour} */{day} * *";
    }

    /// <summary>
    /// 返回指定天数的 Cron 表达式
    /// </summary>
    /// <param name="day">指定天数</param>
    /// <param name="hour">指定小时数</param>
    /// <param name="minute">指定分钟数</param>
    public static string Daily(int day, int hour, int minute)
    {
        return $"{minute} {hour} */{day} * *";
    }

    /// <summary>
    /// 返回每周一的 Cron 表达式
    /// </summary>
    public static string Weekly()
    {
        return Weekly(DayOfWeek.Monday);
    }

    /// <summary>
    /// Returns cron expression that fires every week at 00:00 UTC of the specified
    /// day of the week.
    /// </summary>
    /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
    public static string Weekly(DayOfWeek dayOfWeek)
    {
        return Weekly(dayOfWeek, hour: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every week at the first minute
    /// of the specified day of week and hour in UTC.
    /// </summary>
    /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    public static string Weekly(DayOfWeek dayOfWeek, int hour)
    {
        return Weekly(dayOfWeek, hour, minute: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every week at the specified day
    /// of week, hour and minute in UTC.
    /// </summary>
    /// <param name="dayOfWeek">The day of week in which the schedule will be activated.</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
    public static string Weekly(DayOfWeek dayOfWeek, int hour, int minute)
    {
        return $"{minute} {hour} * * {(int)dayOfWeek}";
    }

    /// <summary>
    /// Returns cron expression that fires every month at 00:00 UTC of the first
    /// day of month.
    /// </summary>
    public static string Monthly()
    {
        return Monthly(day: 1);
    }

    /// <summary>
    /// Returns cron expression that fires every month at 00:00 UTC of the specified
    /// day of month.
    /// </summary>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    public static string Monthly(int day)
    {
        return Monthly(day, hour: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every month at the first minute of the
    /// specified day of month and hour in UTC.
    /// </summary>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    public static string Monthly(int day, int hour)
    {
        return Monthly(day, hour, minute: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every month at the specified day of month,
    /// hour and minute in UTC.
    /// </summary>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
    public static string Monthly(int day, int hour, int minute)
    {
        return $"{minute} {hour} {day} * *";
    }

    /// <summary>
    /// Returns cron expression that fires every year on Jan, 1st at 00:00 UTC.
    /// </summary>
    public static string Yearly()
    {
        return Yearly(month: 1);
    }

    /// <summary>
    /// Returns cron expression that fires every year in the first day at 00:00 UTC
    /// of the specified month.
    /// </summary>
    /// <param name="month">The month in which the schedule will be activated (1-12).</param>
    public static string Yearly(int month)
    {
        return Yearly(month, day: 1);
    }

    /// <summary>
    /// Returns cron expression that fires every year at 00:00 UTC of the specified
    /// month and day of month.
    /// </summary>
    /// <param name="month">The month in which the schedule will be activated (1-12).</param>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    public static string Yearly(int month, int day)
    {
        return Yearly(month, day, hour: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every year at the first minute of the
    /// specified month, day and hour in UTC.
    /// </summary>
    /// <param name="month">The month in which the schedule will be activated (1-12).</param>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    public static string Yearly(int month, int day, int hour)
    {
        return Yearly(month, day, hour, minute: 0);
    }

    /// <summary>
    /// Returns cron expression that fires every year at the specified month, day,
    /// hour and minute in UTC.
    /// </summary>
    /// <param name="month">The month in which the schedule will be activated (1-12).</param>
    /// <param name="day">The day of month in which the schedule will be activated (1-31).</param>
    /// <param name="hour">The hour in which the schedule will be activated (0-23).</param>
    /// <param name="minute">The minute in which the schedule will be activated (0-59).</param>
    public static string Yearly(int month, int day, int hour, int minute)
    {
        return $"{minute} {hour} {day} {month} *";
    }

    /// <summary>
    /// Returns cron expression that never fires. Specifically 31st of February
    /// </summary>
    /// <returns></returns>
    public static string Never()
    {
        return Yearly(2, 31);
    }

    /// <summary>
    /// 转换 Cron 表达式兼容秒
    /// </summary>
    /// <param name="cronExpression"></param>
    /// <returns></returns>
    public static CronExpression ParseCronExpression(this string cronExpression)
    {
        var parts = cronExpression.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var format = CronFormat.Standard;

        if (parts.Length == 6)
        {
            format |= CronFormat.IncludeSeconds;
        }
        else if (parts.Length != 5)
        {
            throw new CronFormatException($"Wrong number of parts in the `{cronExpression}` cron expression, you can only use 5 or 6 (with seconds) part-based expressions.");
        }

        return CronExpression.Parse(cronExpression, format);
    }

    /// <summary>
    /// 通过 Cron 表达式获取下一次执行时间 (本地时间)
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static DateTimeOffset? GetNextExecution(this CronExpression expression) => GetNextExecution(expression, DateTimeOffset.Now);

    /// <summary>
    /// 通过 Cron 表达式获取下一次执行时间 (本地时间)
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="from">开始时间 默认为当前时间</param>
    /// <returns></returns>
    public static DateTimeOffset? GetNextExecution(this CronExpression expression, DateTimeOffset from) => expression.GetNextOccurrence(from, TimeZoneInfo.Local);
}
