using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace AspNetRabbitMQ.ModeDemo.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 默认消费
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

            #region 工作队列模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.QueueDeclare(RabbitMQHelper.Queue_2, false, false, false, null);
            //channel.BasicQos(0, 1, false);//prefetchSize:传输内容大小限制,0为不限制;prefetchCount:同时消费限制数量;global:true应用于信道,false应用于消费者
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_2, true, consumer);
            #endregion

            #region 扇形队列模式(发布订阅)
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_3, ExchangeType.Fanout);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3_1, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3_2, false, false, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_3_3, false, false, false, null);
            //channel.QueueBind(RabbitMQHelper.Queue_3_1, RabbitMQHelper.Exchange_3, string.Empty);
            //channel.QueueBind(RabbitMQHelper.Queue_3_2, RabbitMQHelper.Exchange_3, string.Empty);
            //channel.QueueBind(RabbitMQHelper.Queue_3_3, RabbitMQHelper.Exchange_3, string.Empty);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (model, ea) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(ea.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_3_1, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3_2, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3_3, true, consumer);
            #endregion

            Console.ReadLine();
        }
    }
}
