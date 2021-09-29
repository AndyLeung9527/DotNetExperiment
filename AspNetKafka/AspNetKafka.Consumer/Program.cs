using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace AspNetKafka.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "172.16.6.47:9092",//集群用逗号分隔
                GroupId = "groupId",
                AutoOffsetReset = AutoOffsetReset.Earliest,//Latest=0:只消费消费者启动之后的数据;Earliest=1:从头开始消费
                EnableAutoCommit = false,//true:自动提交,若数据未处理完毕已提交,此时消费端宕机则数据丢失;false:手动提交,若数据处理完未提交,此时消费端宕机,下次则会重复消费.所以消息需符合幂等性
                EnablePartitionEof = true
            };

            using(var c = new ConsumerBuilder<Ignore,string>(config)
                .SetErrorHandler((_, e) => { Console.WriteLine($"Error{e.Reason}"); })
                .SetPartitionsAssignedHandler((_, partitions) => { Console.WriteLine($"Assigned partitions:{string.Join(',',partitions)}"); })//分配的时候调用
                .SetPartitionsRevokedHandler((_, partitions) => { Console.WriteLine($"Revoking assignment:{string.Join(',',partitions)}"); })//新加入消费者的时候调用
                .Build())
            {
                c.Subscribe("topicName");

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;//prevent the process from terminating
                    cts.Cancel();
                };

                try
                {
                    Console.WriteLine("Consume started");
                    while(true)
                    {
                        try
                        {
                            var consumeResult = c.Consume(cts.Token);
                            if (consumeResult.IsPartitionEOF)
                                continue;

                            Console.WriteLine($"Consumed message:'{consumeResult.Message.Value}', at:'{consumeResult.TopicPartitionOffset}'");
                            c.Commit(consumeResult);
                        }
                        catch(ConsumeException e)
                        {
                            Console.WriteLine($"Consumed error:{e.Error.Reason}");
                        }
                    }
                }
                catch(OperationCanceledException)
                {
                    c.Close();
                    Console.WriteLine("Consume stoped");
                }
            }

            Console.ReadKey();
        }
    }
}
