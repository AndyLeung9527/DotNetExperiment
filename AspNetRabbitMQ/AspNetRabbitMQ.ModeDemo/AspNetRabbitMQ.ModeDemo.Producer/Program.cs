using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using System;

namespace AspNetRabbitMQ.ModeDemo.Producer
{
    /// <summary>
    /// 生产者
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            #region 简单模式 & 工作模式
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(string.Empty, RabbitMQHelper.Queue_1, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 发布订阅模式
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Fanout);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty);
            //        channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, string.Empty);
            //        channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, string.Empty);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, string.Empty, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 路由模式
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //        channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //        channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 主题模式
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Topic);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_1);
            //        channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_2);
            //        channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Etc);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //        {
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Publish, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        }
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            Console.ReadLine();
        }
    }
}
