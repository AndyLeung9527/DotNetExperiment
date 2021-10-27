using System;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;

namespace DotNetQuartz.BasicDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();

            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<DemoJob>()
                .WithIdentity("JobName", "GroupName")
                .UsingJobData("data1", "content")
                .UsingJobData("data2", 3.14f)
                .UsingJobData("data3", "jobContent")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("TriggerName", "GroupName")
                .UsingJobData("data3", "triggerContent")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(3).RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);

            await Task.Delay(TimeSpan.FromSeconds(10));

            await scheduler.Clear();
            await scheduler.Shutdown();

            Console.ReadLine();
        }
    }

    public class DemoJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                string data1 = dataMap.GetString("data1");
                float data2 = dataMap.GetFloat("data2");
                string data3 = dataMap.GetString("data3");

                JobDataMap mergeDataMap = context.MergedJobDataMap;//trigger的value会覆盖job的
                data1 = mergeDataMap.GetString("data1");
                data2 = mergeDataMap.GetFloat("data2");
                data3 = mergeDataMap.GetString("data3");

                await Console.Out.WriteLineAsync($"Hello job, {data1}, {data2}, {data3}");
            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}
