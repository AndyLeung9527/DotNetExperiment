using AspNetRabbitMQ.ModeDemo.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var conn = RabbitMQHelper.GetConnection();
            var channel = conn.CreateModel();
            channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
            };
            channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            #endregion

            #region 工作模式
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //channel.BasicQos(0, 1, false);//prefetchSize:传输内容大小限制,0为不限制;prefetchCount:同时消费限制数量;global:true应用于信道,false应用于消费者
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            #endregion

            #region Async
            //var conn = RabbitMQHelper.GetConnection(true);
            //var channel = conn.CreateModel();
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, false, false, false, null);
            //var consumer = new AsyncEventingBasicConsumer(channel);
            //consumer.Received += async (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed(async):{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
            //    channel.BasicAck(e.DeliveryTag, false);
            //    await Task.CompletedTask;
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, false, consumer);
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
            //consumer.Received += (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
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
            //consumer.Received += (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, routingKey:{e.RoutingKey}");
            //    channel.BasicAck(e.DeliveryTag, true);//手动签收,批量
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
            //consumer.Received += (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, routingKey:{e.RoutingKey}");
            //    channel.BasicAck(e.DeliveryTag, true);//手动签收,批量
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_2, true, consumer);
            //channel.BasicConsume(RabbitMQHelper.Queue_3, true, consumer);
            #endregion

            #region 重试 & 死信
            //var conn = RabbitMQHelper.GetConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Direct);
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_Retry, ExchangeType.Direct);
            //channel.ExchangeDeclare(RabbitMQHelper.Exchane_Dead, ExchangeType.Direct);
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMQHelper.Exchane_Dead } });//指定死信交换机,用于将Queue_1队列中失败的消息投递到Exchane_Dead交换机
            //channel.QueueDeclare(RabbitMQHelper.Queue_2, true, false, false, new Dictionary<string, object> { { "x-dead-letter-exchange", RabbitMQHelper.Exchange_1 }, { "x-message-ttl", 6000 } });//指定死信交换机,用于将Queue_2队列中超时的消息投递到Exchange_1交换机;x-message-ttl定义消息的最大停留时间,超时后投递到死信队列
            //channel.QueueDeclare(RabbitMQHelper.Queue_3, true, false, false);
            //channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, RabbitMQHelper.RoutingKey_1);
            //channel.QueueBind(RabbitMQHelper.Queue_2, RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1);
            //channel.QueueBind(RabbitMQHelper.Queue_3, RabbitMQHelper.Exchane_Dead, RabbitMQHelper.RoutingKey_1);

            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (sender, e) =>
            //{
            //    var _sender = (EventingBasicConsumer)sender;                                //消息传送者
            //    var _channel = ((EventingBasicConsumer)sender).Model;                       //消息传送信道
            //    var _message = (BasicDeliverEventArgs)e;                                    //消息传送参数
            //    var _header = e.BasicProperties.Headers;                                    //消息头
            //    var _content = System.Text.Encoding.UTF8.GetString(e.Body.ToArray());       //消息内容

            //    var dicDeath = default(IDictionary<string, object>);                        //死信参数(null)
            //    if (e.BasicProperties.Headers != null && e.BasicProperties.Headers.ContainsKey("x-death"))
            //    {
            //        var temp = e.BasicProperties.Headers["x-death"] as List<object>;
            //        dicDeath = (IDictionary<string, object>)(e.BasicProperties.Headers["x-death"] as List<object>)[0];
            //    }

            //    //模拟成功
            //    try
            //    {
            //        //throw new Exception("模拟消息处理失败效果");
            //        var retryCount = (long)(dicDeath?["count"] ?? default(long));
            //        Console.WriteLine($"{DateTime.Now} Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, retryCount:{retryCount}");
            //        channel.BasicAck(e.DeliveryTag, false);
            //    }
            //    //模拟失败
            //    catch
            //    {
            //        var retryCount = (long)(dicDeath?["count"] ?? default(long));
            //        Console.WriteLine($"{DateTime.Now} Consume failure:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}, retryCount:{retryCount}");
            //        if (retryCount > 1)//重试第3次(首次为0)还没成功,投递到死信交换机--也就是上边定义的{ "x-dead-letter-exchange", RabbitMQHelper.Exchane_Dead }
            //            channel.BasicNack(e.DeliveryTag, false, false);
            //        else//否则投递到Exchange_Retry
            //        {
            //            e.BasicProperties.Expiration = ((retryCount + 1) * 1000 * 10).ToString();//定义下一次投递的间隔时间(毫秒)--如:首次重试间隔10秒,第二次重试间隔20秒,第三次间隔30秒
            //            channel.BasicPublish(RabbitMQHelper.Exchange_Retry, RabbitMQHelper.RoutingKey_1, e.BasicProperties, e.Body);
            //            channel.BasicAck(e.DeliveryTag, false);
            //        }
            //    }
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, false, consumer);
            #endregion

            #region 消息持久化 & 集群
            //var conn = RabbitMQHelper.GetClusterConnection();
            //var channel = conn.CreateModel();
            //channel.ExchangeDeclare(RabbitMQHelper.Exchange_1, ExchangeType.Fanout, true, false, null);
            //channel.QueueDeclare(RabbitMQHelper.Queue_1, true, false, false, null);
            //channel.QueueBind(RabbitMQHelper.Queue_1, RabbitMQHelper.Exchange_1, string.Empty, null);
            //var consumer = new EventingBasicConsumer(channel);
            //consumer.Received += (sender, e) =>
            //{
            //    Console.WriteLine($"Consume succeed:{System.Text.Encoding.UTF8.GetString(e.Body.ToArray())}");
            //    channel.BasicAck(e.DeliveryTag, false);
            //};
            //channel.BasicConsume(RabbitMQHelper.Queue_1, false, consumer);
            #endregion

            Console.ReadLine();
        }
    }
}
