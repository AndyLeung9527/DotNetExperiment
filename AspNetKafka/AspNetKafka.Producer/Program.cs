using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace AspNetKafka.Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "172.16.6.47:9092",//集群用逗号分隔
                Acks = Acks.All,//0(性能最高):写入leader还未落盘则返回ack,若未落盘leader发生宕机则数据丢失;
                                //1(性能中等,鸡肋):写入leader并落盘则返回ack,若未同步备份leader发生宕机则新leader数据丢失;
                                //-1(性能最差):写入leader和所有副本备份完成后才返回,若数据都写入后leader发生宕机则无法返回ack,客户端会补偿重试,推送重复消息.所以消息需符合幂等性
                MessageSendMaxRetries = 3,//补偿重试次数
                Partitioner = Partitioner.Random,//分区,集群用到
                EnableIdempotence = true//幂等性
            };

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                try
                {
                    var deliveryResult = await producer.ProduceAsync("topicName", new Message<string, string> { Key = new Random().Next(1, 10).ToString(), Value = "kafka queue message" });
                    Console.WriteLine($"Delivered to '{deliveryResult.TopicPartitionOffset}'");
                }
                catch (ProduceException<string, string> e)
                {
                    Console.WriteLine($"Delivery failed:{e.Error.Code},{e.Error.Reason},{e.Message}");
                }
            }

            Console.ReadKey();
        }

        ///// <summary>
        ///// 事务
        ///// </summary>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //static async Task Main(string[] args)
        //{
        //    string transactionalId = "transactionalId";
        //    var config = new ProducerConfig
        //    {
        //        BootstrapServers = "172.16.6.47:9092",
        //        Acks = Acks.All,
        //        Partitioner = Partitioner.Random,
        //        EnableIdempotence = true,
        //        TransactionalId = transactionalId
        //    };

        //    using (var producer = new ProducerBuilder<string, string>(config).Build())
        //    {
        //        try
        //        {
        //            producer.InitTransactions(TimeSpan.FromSeconds(3));
        //            producer.BeginTransaction();
        //            var deliveryResult = await producer.ProduceAsync("topicName", new Message<string, string> { Key = new Random().Next(1, 10).ToString(), Value = "kafka queue message(transactional)" });
        //            int i = 1;
        //            int j = i / 0;//模拟异常
        //            producer.CommitTransaction(TimeSpan.FromSeconds(3));
        //            Console.WriteLine($"Delivered to '{deliveryResult.TopicPartitionOffset}'(transactional)");
        //        }
        //        catch(Exception e)
        //        {
        //            producer.AbortTransaction(TimeSpan.FromSeconds(3));
        //            Console.WriteLine($"Delivery failed:{e.Message}(transactional)");
        //        }
        //    }
        //}
    }
}
