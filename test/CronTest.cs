// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Cronos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Longbow.Tasks
{
    public class CronTest
    {
        private readonly ITestOutputHelper _helper;

        public CronTest(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact]
        public void Cron_Ok()
        {
            var dtm = DateTimeOffset.Parse("2020-01-01 00:00:00");

            // 秒
            Assert.Equal(dtm.AddSeconds(1), Cron.Secondly().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddSeconds(5), Cron.Secondly(5).ParseCronExpression().GetNextExecution(dtm));

            // 分钟
            Assert.Equal(dtm.AddMinutes(1), Cron.Minutely().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddMinutes(5), Cron.Minutely(5).ParseCronExpression().GetNextExecution(dtm.AddMinutes(1)));
            Assert.Equal(dtm.AddMinutes(2).AddSeconds(5), Cron.Minutely(2, 5).ParseCronExpression().GetNextExecution(dtm.AddMinutes(1)));

            // 小时
            Assert.Equal(dtm.AddHours(1), Cron.Hourly().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddHours(5), Cron.Hourly(5).ParseCronExpression().GetNextExecution(dtm));

            // 天
            Assert.Equal(dtm.AddDays(1), Cron.Daily().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddDays(5), Cron.Daily(5).ParseCronExpression().GetNextExecution(dtm.AddDays(1)));
            Assert.Equal(dtm.AddDays(5).AddHours(5), Cron.Daily(5, 5).ParseCronExpression().GetNextExecution(dtm.AddDays(1)));
            Assert.Equal(dtm.AddDays(5).AddHours(2).AddMinutes(5), Cron.Daily(5, 2, 5).ParseCronExpression().GetNextExecution(dtm.AddDays(1)));

            // 星期
            // 计算星期几
            var interval = 7 - (dtm.DayOfWeek - DayOfWeek.Monday);
            Assert.Equal(dtm.AddDays(interval), Cron.Weekly().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddDays(interval + 1), Cron.Weekly(DayOfWeek.Tuesday).ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddDays(interval + 1).AddHours(2), Cron.Weekly(DayOfWeek.Tuesday, 2).ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddDays(interval + 1).AddHours(2).AddMinutes(5), Cron.Weekly(DayOfWeek.Tuesday, 2, 5).ParseCronExpression().GetNextExecution(dtm));

            // 月
            // 计算天数
            interval = 1 - dtm.Day;
            Assert.Equal(dtm.AddDays(interval).AddMonths(1), Cron.Monthly().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddDays(interval + 1).AddMonths(1), Cron.Monthly(2).ParseCronExpression().GetNextExecution(dtm.AddDays(interval + 2)));
            Assert.Equal(dtm.AddDays(interval + 1).AddMonths(1).AddHours(1), Cron.Monthly(2, 1).ParseCronExpression().GetNextExecution(dtm.AddDays(interval + 2)));
            Assert.Equal(dtm.AddDays(interval + 1).AddMonths(1).AddHours(1).AddMinutes(10), Cron.Monthly(2, 1, 10).ParseCronExpression().GetNextExecution(dtm.AddDays(interval + 1).AddHours(1).AddMinutes(15)));

            // 年
            // 计算天数
            Assert.Equal(dtm.AddYears(1), Cron.Yearly().ParseCronExpression().GetNextExecution(dtm));
            Assert.Equal(dtm.AddYears(1).AddMonths(1), Cron.Yearly(2).ParseCronExpression().GetNextExecution(dtm.AddYears(1).AddDays(1)));
            Assert.Equal(dtm.AddYears(1).AddDays(1).AddMonths(1), Cron.Yearly(2, 2).ParseCronExpression().GetNextExecution(dtm.AddYears(1).AddDays(1)));
            Assert.Equal(dtm.AddYears(1).AddDays(1).AddMonths(1).AddHours(1), Cron.Yearly(2, 2, 1).ParseCronExpression().GetNextExecution(dtm.AddYears(1).AddDays(1)));
            Assert.Equal(dtm.AddYears(1).AddDays(interval + 1).AddMonths(1).AddHours(1).AddMinutes(1), Cron.Yearly(2, 2, 1, 1).ParseCronExpression().GetNextExecution(dtm.AddYears(1).AddDays(1)));
        }

        [Fact]
        public void ParseCronExpression_Exception()
        {
            Assert.ThrowsAny<CronFormatException>(() => Cron.ParseCronExpression("* *"));
            Assert.ThrowsAny<NullReferenceException>(() => Cron.ParseCronExpression(null));
        }

        [Fact]
        public void ParseCron_Ok()
        {
            var crop = Cron.Secondly().ParseCronExpression();
            Assert.Equal(Cron.Secondly(), crop.ToString());
        }

        [Fact]
        public void GetNextExecution_Ok()
        {
            Assert.Null(Cron.Never().ParseCronExpression().GetNextExecution());
        }

        [Fact]
        public void Secondly_Ok()
        {
            var cron = Cron.Secondly().ParseCronExpression();
            var now = DateTimeOffset.Now;
            var nextRuntimes = cron.GetOccurrences(now, now.AddMinutes(1), TimeZoneInfo.Local).Take(3).ToList();
            nextRuntimes.ForEach(d =>
            {
                _helper.WriteLine($"{d.ToString()}");
            });
            Assert.Equal(nextRuntimes[0].AddSeconds(1), nextRuntimes[1]);
            Assert.Equal(nextRuntimes[1].AddSeconds(1), nextRuntimes[2]);

            // 每 2 秒
            cron = "*/2 * * * * *".ParseCronExpression();
            nextRuntimes = cron.GetOccurrences(now, now.AddMinutes(1), TimeZoneInfo.Local).Take(3).ToList();
            nextRuntimes.ForEach(d =>
            {
                _helper.WriteLine(d.ToString());
            });
            Assert.Equal(nextRuntimes[0].AddSeconds(2), nextRuntimes[1]);
            Assert.Equal(nextRuntimes[1].AddSeconds(2), nextRuntimes[2]);

            // at 10 秒
            cron = "10 * * * * *".ParseCronExpression();
            nextRuntimes = cron.GetOccurrences(now, now.AddMinutes(5), TimeZoneInfo.Local).Take(3).ToList();
            nextRuntimes.ForEach(d =>
            {
                _helper.WriteLine(d.ToString());
            });
            Assert.Equal(nextRuntimes[0].AddSeconds(60), nextRuntimes[1]);
            Assert.Equal(nextRuntimes[1].AddSeconds(60), nextRuntimes[2]);

            // range 10-15
            cron = "10-13 * * * * *".ParseCronExpression();
            nextRuntimes = cron.GetOccurrences(now, now.AddMinutes(1), TimeZoneInfo.Local).Take(3).ToList();
            nextRuntimes.ForEach(d =>
            {
                _helper.WriteLine(d.ToString());
            });
            Assert.Equal(nextRuntimes[0].AddSeconds(1), nextRuntimes[1]);
            Assert.Equal(nextRuntimes[1].AddSeconds(1), nextRuntimes[2]);
        }

        [Fact]
        public async void Second_Loop()
        {
            var cron = Cron.Secondly().ParseCronExpression();
            var count = 3;
            var now = DateTimeOffset.Now;
            while (count-- > 0)
            {
                var next = cron.GetNextExecution();
                _helper.WriteLine($"{now} - {next}");
                await Task.Delay(1000);
            }
        }

        [Fact]
        public void Exception_Ok()
        {
            Assert.ThrowsAny<CronFormatException>(() => "61 * * * * *".ParseCronExpression().GetNextExecution());
        }
    }
}
