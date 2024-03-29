﻿using DotNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace DotNetRabbitMQ.ModeDemo.Producer
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

            #region 事务
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using(var channel = conn.CreateModel())
            //    {
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        try
            //        {
            //            channel.TxSelect();//开启事务
            //            channel.BasicPublish(string.Empty, RabbitMQHelper.Queue_1, null, System.Text.Encoding.UTF8.GetBytes("事务消息测试"));
            //            int i = 1;
            //            int j = i / 0;//模拟异常
            //            channel.TxCommit();
            //            Console.WriteLine("Publish succeed.");
            //        }
            //        catch(Exception)
            //        {
            //            if(channel.IsOpen)
            //            {
            //                channel.TxRollback();
            //                Console.WriteLine("Publish failure and execute rollback");
            //            }
            //        }
            //    }
            //}
            #endregion

            #region 推送队列失败回调
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null);
            //        channel.BasicReturn += (sender, e) =>
            //        {
            //            var code = e.ReplyCode;//失败code
            //            var text = e.ReplyText;//失败原因
            //            var content = System.Text.Encoding.UTF8.GetString(e.Body.Span);//消息内容
            //            Console.WriteLine($"Publish failure,code:{code},text:{text},content:{content}");//对消息不可达做处理
            //        };
            //        var properties = channel.CreateBasicProperties();
            //        properties.MessageId = "MsgId";
            //        channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, true, properties, System.Text.Encoding.UTF8.GetBytes("消息推送失败测试"));//mandatory必须为true
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            #region 备份交换机
            //using (var conn = RabbitMQHelper.GetConnection())
            //{
            //    using (var channel = conn.CreateModel())
            //    {
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct, false, false, new Dictionary<string, object> { { "alternate-exchange", RabbitMQHelper.Exchange_Backup } });//指定备份交换机
            //        channel.ExchangeDeclare(RabbitMQHelper.Exchange_Backup, ExchangeType.Fanout, false, false, null);//声明备份交换机,模式Fanout(直接推到交换机而无需推到路由键)
            //        channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //        channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null);
            //        channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Backup, string.Empty, null);
            //        string[] arrMsg = new string[] { "test1", "test2", "test3", "test4", "test5" };
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1, null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        foreach (string msg in arrMsg)
            //            channel.BasicPublish(RabbitMQHelper.Exchange_1, "Push to error routing key", null, System.Text.Encoding.UTF8.GetBytes(msg));
            //        Console.WriteLine("Publish succeed.");
            //    }
            //}
            #endregion

            Console.ReadLine();
        }
    }
}
