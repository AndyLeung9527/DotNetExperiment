using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace AspNetRabbitMQ.ModeDemo.Producer
{
    /// <summary>
    /// 生产者
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            #region 简单模式 & 工作模式 & Async
            using (var conn = RabbitMQHelper.GetConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
                    string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
                    foreach (string msg in arrMsg)
                        channel.BasicPublish(string.Empty, RabbitMQHelper.Queue_1, null, System.Text.Encoding.UTF8.GetBytes(msg));
                    Console.WriteLine("Publish succeed.");
                }
            }
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
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Publish, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 重试 & 死信(延迟)
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using(var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_Retry, ExchangeType.Direct);
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchane_Dead, ExchangeType.Direct);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMQHelper.Exchane_Dead } });//指定死信交换机,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMQHelper.Exchange_1 }, { "x-message-ttl", 6000 } });//指定死信交换机,用于将Queue_2队列中超时的消息投递到Exchange_1交换机;x-message-ttl定义消息的最大停留时间,超时后投递到死信队列
            //        channel.QueueDeclare(RabbitMQHelper.Queue_3, true, false, false);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //        channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1);
            //        channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchane_Dead, RabbitMQHelper.RoutingKey_1);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 消息持久化 & 集群
            //using (var conn = RabbitMQHelper.GetClusterConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Fanout, true, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, true, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty, null);
            //        var properties = channel.CreateBasicProperties();
            //        properties.Persistent = true;
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, string.Empty, properties, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            Console.ReadLine();
        }
    }
}
