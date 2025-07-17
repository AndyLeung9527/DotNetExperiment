using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DotNetAI.SemanticKernelAgentOrchestration;

internal class Program
{
    const string API_KEY = "sk-0d10b1fef51846c6b1881ef0a53b4f0e";
    const string URL = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    const string MODEL = "qwen-turbo";

    async static Task Main(string[] args)
    {
        Kernel CreateKernelWithOpenAIChatCompletion()
        {
            return Kernel.CreateBuilder().AddOpenAIChatCompletion(
                modelId: MODEL,
                endpoint: new Uri(URL),
                apiKey: API_KEY
            ).Build();
        }

        var kernel = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: MODEL,
            endpoint: new Uri(URL),
            apiKey: API_KEY
        ).Build();

        // 需要使用运行时来管理代理的执行
        InProcessRuntime runtime = new();
        // 启动运行时
        await runtime.StartAsync();

#pragma warning disable SKEXP0110, SKEXP0001
        // 并发编排（Concurrent）
        /*
        {
            ChatCompletionAgent physicist = new ChatCompletionAgent
            {
                Name = "Physicist",
                Description = "An expert in physics",
                Instructions = "You are an expert in physics. You answer questions from a physics perspective.",
                Kernel = kernel
            };

            ChatCompletionAgent chemist = new ChatCompletionAgent
            {
                Name = "Chemist",
                Description = "An expert in chemistry",
                Instructions = "You are an expert in chemistry. You answer questions from a chemistry perspective.",
                Kernel = kernel
            };

            // 创建并发编排实例
            ConcurrentOrchestration orchestration = new(physicist, chemist);

            // 自定义输入、输出类型
            //ConcurrentOrchestration<object, object> ioOrchestration = new()
            //{
            //    // 自定义输入转换
            //    InputTransform = (input, CancellationToken) =>
            //    {
            //        var message = new ChatMessageContent(AuthorRole.User, input.ToString());
            //        return ValueTask.FromResult<IEnumerable<ChatMessageContent>>([message]);
            //    },
            //    ResultTransform = (result, CancellationToken) =>
            //    {
            //        return ValueTask.FromResult<object>(result);
            //    }
            //};

            // 调用编排
            // 编排将并发运行给定任务上的所有代理
            OrchestrationResult<string[]> result = await orchestration.InvokeAsync("什么是温度?", runtime);

            // 收集结果，输出多个结果，不保证结果顺序
            string[] output = await result.GetValueAsync(
                timeout: TimeSpan.FromSeconds(20)// 超时时间，未返回结果则抛异常
            );
            Console.WriteLine($"\n# RESULT:\n{string.Join("\n\n", output.Select(text => $"{text}"))}");

            // 停止后续代理流程
            //result.Cancel();
        }
        */

        // 顺序编排（Sequential）
        // 代理被组织为一个流程，按序地每个代理将其输出传递到下一个代理
        /*
        {
            ChatCompletionAgent analystAgent = new ChatCompletionAgent
            {
                Name = "Analyst",
                Description = "A agent that extracts key concepts from a product description.",
                Instructions = "You are a marketing analyst. Given a product description, identify:\n- Key features\n- Target audience\n- Unique selling points",
                Kernel = kernel
            };

            ChatCompletionAgent writerAgent = new ChatCompletionAgent
            {
                Name = "Copywriter",
                Description = "An agent that writes a marketing copy based on the extracted concepts.",
                Instructions = "You are a marketing copywriter. Given a block of text describing features, audience, and USPs, compose a compelling marketing copy (like a newsletter section) that highlights these points. Output should be short (around 150 words), output just the copy as a single text block.",
                Kernel = kernel
            };

            ChatCompletionAgent editorAgent = new ChatCompletionAgent
            {
                Name = "Editor",
                Description = "An agent that formats and proofreads the marketing copy.",
                Instructions = "You are an editor. Given the draft copy, correct grammar, improve clarity, ensure consistent tone, give format and make it polished. Output the final improved copy as a single text block.",
                Kernel = kernel
            };

            ChatHistory history = [];

            // 创建顺序编排实例
            SequentialOrchestration orchestration = new(analystAgent, writerAgent, editorAgent)
            {
                // 每次代理响应的回调函数
                ResponseCallback = (chatMessageContent) =>
                {
                    history.Add(chatMessageContent);
                    return ValueTask.CompletedTask;
                }
            };

            OrchestrationResult<string> result = await orchestration.InvokeAsync("环保不锈钢水瓶，可让饮料保持24小时低温", runtime);

            // 收集结果，输出最终结果
            string output = await result.GetValueAsync(TimeSpan.FromSeconds(20));
            Console.WriteLine($"\n# RESULT: {output}");

            // 按代理顺序输出每个响应结果
            Console.WriteLine("\n\nORCHESTRATION HISTORY");
            foreach (var (message, index) in history.Select((item, index) => (item, index)))
            {
                Console.WriteLine($"{index}. {message.Role} > {message.Content}");
            }
        }
        */

        // 群组编排（Group chat）
        // 通过管理器设定下一个代理或人工输入
        /*
        {
            ChatCompletionAgent writer = new ChatCompletionAgent
            {
                Name = "CopyWriter",
                Description = "A copy writer",
                Instructions = "You are a copywriter with ten years of experience and are known for brevity and a dry humor. The goal is to refine and decide on the single best copy as an expert in the field. Only provide a single proposal per response. You're laser focused on the goal at hand. Don't waste time with chit chat. Consider suggestions when refining an idea.",
                Kernel = kernel
            };

            ChatCompletionAgent editor = new ChatCompletionAgent
            {
                Name = "Reviewer",
                Description = "An editor.",
                Instructions = "You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine if the given copy is acceptable to print. If so, state that it is approved. If not, provide insight on how to refine suggested copy without example.",
                Kernel = kernel
            };

            ChatHistory history = [];

            // 创建群组编排实例
            GroupChatOrchestration orchestration = new(
                // 群组管理器
                // 可换用自定义的群组管理器CustomGroupChatManager
                manager: new RoundRobinGroupChatManager { MaximumInvocationCount = 5 },// 群组agent的最大调用次数
                writer,
                editor)
            {
                //每次代理响应的回调函数
                ResponseCallback = (chatMessageContent) =>
                {
                    history.Add(chatMessageContent);
                    return ValueTask.CompletedTask;
                }
            };

            OrchestrationResult<string> result = await orchestration.InvokeAsync("为一款价格实惠、驾驶有趣的新型电动SUV创造一个口号", runtime);

            string output = await result.GetValueAsync(TimeSpan.FromSeconds(60));
            Console.WriteLine($"\n# RESULT: {output}");

            // 按代理响应顺序输出每个响应结果
            Console.WriteLine("\n\nORCHESTRATION HISTORY");
            foreach (var (message, index) in history.Select((item, index) => (item, index)))
            {
                Console.WriteLine($"{index}. {message.Role} > {message.Content}");
            }
        }
        */

        // 交接编排（Handoff）
        // 每个代理根据可调用关系选择将对话和任务移交到另一个代理，确保用适当的代理去处理任务的每个部分
        // 每个代理有自己的函数调用，使用自己的kernel，不共享内核
        {
            ChatCompletionAgent triageAgent = new ChatCompletionAgent
            {
                Name = "TriageAgent",
                Description = "Handle customer requests.",
                Instructions = "A customer support agent that triages issues.",
                Kernel = CreateKernelWithOpenAIChatCompletion()
            };

            ChatCompletionAgent statusAgent = new ChatCompletionAgent
            {
                Name = "OrderStatusAgent",
                Description = "A customer support agent that checks order status.",
                Instructions = "Handle order status requests.",
                Kernel = CreateKernelWithOpenAIChatCompletion()
            };
            statusAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderStatusPlugin()));

            ChatCompletionAgent returnAgent = new ChatCompletionAgent
            {
                Name = "OrderReturnAgent",
                Description = "A customer support agent that handles order returns.",
                Instructions = "Handle order return requests.",
                Kernel = CreateKernelWithOpenAIChatCompletion()
            };
            returnAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderReturnPlugin()));

            ChatCompletionAgent refundAgent = new ChatCompletionAgent
            {
                Name = "OrderRefundAgent",
                Description = "A customer support agent that handles order refund.",
                Instructions = "Handle order refund requests.",
                Kernel = CreateKernelWithOpenAIChatCompletion()
            };
            refundAgent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromObject(new OrderRefundPlugin()));

            // 设置交接关系
            // 指定哪个代理可向哪个代理进行移交
            var handofffs = OrchestrationHandoffs
                .StartWith(triageAgent)
                .Add(triageAgent, statusAgent, returnAgent, refundAgent)
                .Add(statusAgent, triageAgent, "Transfer to this agent if the issue is not status related")
                .Add(returnAgent, triageAgent, "Transfer to this agent if the issue is not return related")
                .Add(refundAgent, triageAgent, "Transfer to this agent if the issue is not refund related");

            ChatHistory history = [];
            Queue<string> responses = new();
            responses.Enqueue("我想查看我的订单状态");
            responses.Enqueue("我的订单ID是123");
            responses.Enqueue("我想退回一个订单");
            responses.Enqueue("订单ID是321");
            responses.Enqueue("损坏的物品");
            responses.Enqueue("没有了，再见");

            // 创建交接编排实例
            HandoffOrchestration orchestration = new(
                handoffs: handofffs,
                triageAgent, statusAgent, returnAgent, refundAgent
            )
            {
                // 当代理需要用户输入时的回调函数
                InteractiveCallback = () =>
                {
                    string input = responses.Dequeue();
                    Console.WriteLine($"\n# INPUT: {input}\n");
                    return ValueTask.FromResult(new ChatMessageContent(AuthorRole.User, input));
                },
                // 每次代理响应的回调函数
                ResponseCallback = (chatMessageContent) =>
                {
                    history.Add(chatMessageContent);
                    return ValueTask.CompletedTask;
                }
            };

            var result = await orchestration.InvokeAsync("我是一名需要帮助处理订单的客户", runtime);

            string output = await result.GetValueAsync(TimeSpan.FromSeconds(300));
            Console.WriteLine($"\n# RESULT: {output}");

            // 按代理响应顺序输出每个响应结果
            Console.WriteLine("\n\nORCHESTRATION HISTORY");
            foreach (ChatMessageContent message in history)
            {
                Console.WriteLine($"# {message.Role} - {message.AuthorName}: {message.Content}");
            }
        }

        // 磁性编排（Magentic）
        // Magentic管理器协调代理，根据上下文、任务进度、代理功能等选择下一个代理
        /*
        {
            ChatCompletionAgent researchAgent = new ChatCompletionAgent
            {
                Name = "ResearchAgent",
                Description = "A helpful assistant with access to web search. Ask it to perform web searches.",
                Instructions = "You are a Researcher. You find information without additional computation or quantitative analysis.",
                Kernel = CreateKernelWithOpenAIChatCompletion()
            };

            ChatCompletionAgent coderAgent = new ChatCompletionAgent
            {
                Name = "CoderAgent",
                Description = "Write and executes code to process and analyze data.",
                Instructions = "You solve questions using code. Please provide detailed analysis and computation process.",
                Kernel = kernel
            };

            // Magentic管理器
            StandardMagenticManager manager = new StandardMagenticManager(
                service: kernel.GetRequiredService<IChatCompletionService>(),
                executionSettings: new OpenAIPromptExecutionSettings()
            )
            {
                MaximumInvocationCount = 5// agent的最大调用次数
            };

            ChatHistory history = [];

            // 创建磁性编排实例
            MagenticOrchestration orchestration = new MagenticOrchestration(
                manager: manager,
                researchAgent,
                coderAgent)
            {
                //每次代理响应的回调函数
                ResponseCallback = (chatMessageContent) =>
                {
                    history.Add(chatMessageContent);
                    return ValueTask.CompletedTask;
                }
            };

            OrchestrationResult<string> result = await orchestration.InvokeAsync("""
                I am preparing a report on the energy efficiency of different machine learning model architectures.
                Compare the estimated training and inference energy consumption of ResNet-50, BERT-base, and GPT-2 on standard datasets
                (e.g., ImageNet for ResNet, GLUE for BERT, WebText for GPT-2).
                Then, estimate the CO2 emissions associated with each, assuming training on an Azure Standard_NC6s_v3 VM for 24 hours.
                Provide tables for clarity, and recommend the most energy-efficient model per task type
                (image classification, text classification, and text generation).
                """, runtime);

            string output = await result.GetValueAsync(TimeSpan.FromSeconds(300));
            Console.WriteLine($"\n# RESULT: {output}");

            // 按代理响应顺序输出每个响应结果
            Console.WriteLine("\n\nORCHESTRATION HISTORY");
            foreach (ChatMessageContent message in history)
            {
                Console.WriteLine($"# {message.Role} - {message.AuthorName}: {message.Content}");
            }
        }
        */
#pragma warning restore SKEXP0110, SKEXP0001

        // 停止运行时（可选），清理资源
        await runtime.RunUntilIdleAsync();

        Console.ReadLine();
    }
}

#pragma warning disable SKEXP0110
/// <summary>
/// 自定义群组聊天管理器
/// 可用控制结果的筛选方式、下一个代理的选择方式、何时请求用户输入或终止聊天
/// 调用顺序：SelectNextAgent()->ShouldRequestUserInput()->ShouldTerminate()->|true|->FilterResults()
///                                                                        ->|false|->SelectNextAgent()
/// </summary>
public class CustomGroupChatManager : GroupChatManager
{
    public override ValueTask<GroupChatManagerResult<string>> FilterResults(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // 自定义逻辑以用于过滤或汇总聊天结果
        var summay = history.LastOrDefault(o => !string.IsNullOrEmpty(o.Content));// 获取最新的包含内容的对话
        return ValueTask.FromResult(new GroupChatManagerResult<string>(summay?.Content ?? string.Empty) { Reason = "Custom summary logic." });
    }

    public override ValueTask<GroupChatManagerResult<string>> SelectNextAgent(ChatHistory history, GroupChatTeam team, CancellationToken cancellationToken = default)
    {
        // 随机在群组上选择一个agent
        var nextAgent = Random.Shared.GetItems<KeyValuePair<string, (string, string)>>(team.Select(o => new KeyValuePair<string, (string, string)>(o.Key, o.Value)).ToArray().AsSpan(), 1).First();

        return ValueTask.FromResult(new GroupChatManagerResult<string>(nextAgent.Key) { Reason = "Custom selection logic." });
    }

    public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // 自定义逻辑以决定是否需要用户输入
        return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "No user input required." });
    }

    public override async ValueTask<GroupChatManagerResult<bool>> ShouldTerminate(ChatHistory history, CancellationToken cancellationToken = default)
    {
        // 调用基类的默认终止逻辑，若已终止则返回
        var baseResult = await base.ShouldTerminate(history, cancellationToken);
        if (baseResult.Value)
        {
            return baseResult;
        }

        // 否则使用自定义的终止逻辑
        bool shouldEnd = history.Count > 10; // 当超过10个消息时终止
        return new GroupChatManagerResult<bool>(shouldEnd) { Reason = "Custom termination logic." };
    }
}
#pragma warning restore SKEXP0110

public sealed class OrderStatusPlugin
{
    [KernelFunction]
    public string CheckOrderStatus(string orderId) => $"Order {orderId} is shipped and will arrive in 2-3 days.";
}
public sealed class OrderReturnPlugin
{
    [KernelFunction]
    public string ProcessReturn(string orderId, string reason) => $"Return for order {orderId} has been processed successfully.";
}
public sealed class OrderRefundPlugin
{
    [KernelFunction]
    public string ProcessReturn(string orderId, string reason) => $"Refund for order {orderId} has been processed successfully.";
}