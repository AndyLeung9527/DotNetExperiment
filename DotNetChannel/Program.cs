using System.Threading.Channels;

namespace DotNetChannel
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Channel是线程安全类型，支持多线程使用
            //无上限通道
            {
                var channel = Channel.CreateUnbounded<string>();
            }

            //有上限通道
            {
                var channel = Channel.CreateBounded<string>(10);
            }

            //到达上限后如何处理
            {
                var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10)
                {
                    FullMode = BoundedChannelFullMode.Wait//当队列已满，写入数据时返回false，直到队列内有空间时可以继续写入
                    //FullMode = BoundedChannelFullMode.DropNewest//移除最新的数据，即从队列尾部开始移除元素
                    //FullMode = BoundedChannelFullMode.DropOldest//移除最旧的数据，即从队列头部开始移除元素
                    //FullMode = BoundedChannelFullMode.DropWrite//可以写入数据，但是数据会被立即丢弃
                }, item => //被删除的项处理回调
                {
                    Console.WriteLine(item);
                });
            }

            //指定是否单一消费者或生产者，默认都是false
            {
                //创建一个单生产者，多消费者的Channel
                var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
                {
                    SingleWriter = true,
                    SingleReader = false
                });
            }

            //写入\读取
            {
                var channel = Channel.CreateUnbounded<string>();
                await channel.Writer.WriteAsync("demo");
                //var isSucceed = channel.Writer.TryWrite("demo");//写入失败时返回false
                //while(await channel.Writer.WaitToWriteAsync())//channel关闭后会返回false，否则非阻塞等待写入
                //{
                //    var isSucceed = channel.Writer.TryWrite("demo");
                //}
                channel.Writer.Complete();//Complete表示不再向通道写入数据

                //一次读取一个
                while (await channel.Reader.WaitToReadAsync())
                {
                    if (channel.Reader.TryRead(out var item))
                    {
                        Console.WriteLine(item);
                    }
                }
                //一次全部读出
                while (await channel.Reader.WaitToReadAsync())
                {
                    await foreach (var item in channel.Reader.ReadAllAsync())
                    {
                        Console.WriteLine(item);
                    }
                }
            }

            //流式应用(分治)
            {
                //获取文件
                Task<Channel<string>> GetFilesAsync()
                {
                    var filePathChannel = Channel.CreateUnbounded<string>();
                    filePathChannel.Writer.TryWrite("file1.txt");
                    filePathChannel.Writer.TryWrite("file2.txt");
                    filePathChannel.Writer.Complete();
                    return Task.FromResult(filePathChannel);
                }

                //处理文件输出结果
                async Task<Channel<string>[]> Analyse(Channel<string> filePathChannel)
                {
                    var counterChannel = Channel.CreateUnbounded<string>();
                    var errorsChannel = Channel.CreateUnbounded<string>();
                    while (await filePathChannel.Reader.WaitToReadAsync())
                    {
                        await foreach (var filePath in filePathChannel.Reader.ReadAllAsync())
                        {
                            counterChannel.Writer.TryWrite($"File [{filePath}] Analysis begins.");
                            errorsChannel.Writer.TryWrite($"File [{filePath}] Analysis errors");
                        }
                    }
                    counterChannel.Writer.Complete();
                    errorsChannel.Writer.Complete();
                    return new Channel<string>[] { counterChannel, errorsChannel };
                }

                //汇总信息
                async Task<Channel<string>> Merge(params Channel<string>[] channels)
                {
                    var mergeTasks = new List<Task>();
                    var outputChannel = Channel.CreateUnbounded<string>();
                    foreach (var channel in channels)
                    {
                        var thisChannel = channel;
                        var mergeTask = Task.Run(async () =>
                        {
                            while (await thisChannel.Reader.WaitToReadAsync())
                            {
                                await foreach (var item in thisChannel.Reader.ReadAllAsync())
                                {
                                    outputChannel.Writer.TryWrite(item);
                                }
                            }
                        });
                        mergeTasks.Add(mergeTask);
                    }
                    await Task.WhenAll(mergeTasks);
                    outputChannel.Writer.Complete();
                    return outputChannel;
                }

                //执行
                //       ------(channel)-------> Step2-1 ------(channel)------->
                //      /                                                        \
                //Step1                                                            Step3
                //      \                                                        /
                //       ------(channel)-------> Step2-2 ------(channel)------->
                var fileChannelPath = await GetFilesAsync();
                var analyseChannels = await Analyse(fileChannelPath);
                var mergedChannel = await Merge(analyseChannels);
                while (await mergedChannel.Reader.WaitToReadAsync())
                {
                    await foreach (var item in mergedChannel.Reader.ReadAllAsync())
                    {
                        Console.WriteLine(item);
                    }
                }
            }
        }
    }
}