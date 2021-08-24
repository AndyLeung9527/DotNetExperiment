using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using System;

namespace AspNetRabbitMQ.ModeDemo.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 默认推送到队列
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.BasicPublish(string.Empty, RabbitMQHelper.Queue_1, null, System.Text.Encoding.UTF8.GetBytes("test message"));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 工作队列模式
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(string.Empty, RabbitMQHelper.Queue_2, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 扇形队列模式(发布订阅)
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_3, ExchangeType.Fanout);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3_1, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3_2, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3_3, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_3_1, RabbitMQHelper.Exchange_3, string.Empty);
            //        channel.QueueBind(RabbitMQHelper.Queue_3_2, RabbitMQHelper.Exchange_3, string.Empty);
            //        channel.QueueBind(RabbitMQHelper.Queue_3_3, RabbitMQHelper.Exchange_3, string.Empty);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_3, string.Empty, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            Console.ReadLine();
        }
    }
}
