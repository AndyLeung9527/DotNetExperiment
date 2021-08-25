using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace AspNetRabbitMQ.ModeDemo.Consumer
{
    /// <summary>
    /// 消费者
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            #region 简单模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            #endregion

            #region 工作模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //channel.BasicQos(0, 1, false);//prefetchSize:传输内容大小限制,0为不限制;prefetchCount:同时消费限制数量;global:true应用于信道,false应用于消费者
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            #endregion

            #region 发布订阅模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Fanout);
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty);
            //channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, string.Empty);
            //channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, string.Empty);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_2, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3, true, consumer);
            #endregion

            #region 路由模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}, routingKey:{ea.RoutingKey}");
            //    channel.BasicAck(ea.DeliveryTag, true);//手动签收,批量
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, false, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_2, false, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3, false, consumer);
            #endregion

            #region 主题模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Topic);
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3, false, false, false, null);
            //channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_1);
            //channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_2);
            //channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_Topic_Etc);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}, routingKey:{ea.RoutingKey}");
            //    channel.BasicAck(ea.DeliveryTag, true);//手动签收,批量
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_2, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3, true, consumer);
            #endregion

            Console.ReadLine();
        }
    }
}
