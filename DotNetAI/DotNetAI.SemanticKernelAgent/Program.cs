using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Functions;
using Microsoft.SemanticKernel.Memory;
using System.Net.Http.Headers;

namespace DotNetAI.SemanticKernelAgent;

internal class Program
{
    const string API_KEY = "sk-0d10b1fef51846c6b1881ef0a53b4f0e";
    const string URL = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    const string CHAT_MODEL = "qwen-turbo";
    const string VECTOR_MODEL = "text-embedding-v4";

    async static Task Main(string[] args)
    {
        var kernel = Kernel.CreateBuilder().AddOpenAIChatClient(
            modelId: CHAT_MODEL,
            endpoint: new Uri(URL),
            apiKey: API_KEY
        ).AddOpenAITextEmbeddingGeneration(
            modelId: VECTOR_MODEL,
            apiKey: API_KEY,
            httpClient: new HttpClient { BaseAddress = new Uri(URL) }
        ).Build();

        var chatClient = kernel.GetRequiredService<IChatClient>();

        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // 创建聊天完成agent
        ChatCompletionAgent agent = new()
        {
            Name = "ReviewGuru",
            Instructions = "You are a friendly assistant that summarizes key points and sentiments from customer reviews. For each response, list available functions.",
            Kernel = kernel,
#pragma warning disable SKEXP0001, SKEXP0130
            Arguments = new(new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions
                {
                    RetainArgumentTypes = true// 是否保留函数的参数类型，当true时反序列化为JsonElement以保留类型信息
                })
            }),
            UseImmutableKernel = true// 允许动态更改kernel
#pragma warning restore SKEXP0001, SKEXP0130
        };

        // 创建agent线程，有状态的代理服务
        // agent线程可以添加上下文函数选择provider（函数调用）、内存provider（会话状态存储）等
        ChatHistoryAgentThread agentThread = new();
#pragma warning disable SKEXP0110, SKEXP0130
        // 添加上下文函数选择provider，根据当前对话上下文，选择最相关的可调用函数
        {
            // 定义上下文函数选择provider
            // 把上下文（用户的对话内容）和所有可用函数进行向量化处理，通过计算相似度，筛选出最相关的几个函数，供agent调用
            var contextualFunctionProvider = new ContextualFunctionProvider(
                vectorStore: new InMemoryVectorStore(new() { EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator() }),
                vectorDimensions: 1536,
                functions: GetAvailableFunctions(),
                maxNumberOfFunctions: 3,// 仅传入前3个函数
                options: new ContextualFunctionProviderOptions
                {
                    NumberOfRecentMessagesInContext = 1,// 上下文中只包含最后一条消息
                    // 在上下文向量化前预处理
                    ContextEmbeddingValueProvider = (recentMessages, newMessages, cancellationToken) =>
                    {
                        // 只获取user的消息进行嵌入向量化
                        var allUserMessages = recentMessages.Concat(newMessages)
                            .Where(m => m.Role == ChatRole.User)
                            .Select(m => m.Text)
                            .Where(content => !string.IsNullOrWhiteSpace(content));

                        return Task.FromResult(string.Join("\n", allUserMessages));
                    },
                    // 在对函数向量化前预处理
                    EmbeddingValueProvider = (function, cancellationToken) =>
                    {
                        // 只对函数名进行嵌入向量化
                        return Task.FromResult(function.Name);
                    }
                }
            );

            agentThread.AIContextProviders.Add(contextualFunctionProvider);
        }

        // 添加内存provider（使用Mem0），为agent线程提供长期记忆能力
        HttpClient? httpClient = null;
        {
            // Mem0使用的httpClient
            httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.mem0.ai")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "m0-UcVKDqqe2KI9Cu1NxoY5pBf73z7iTanDlNms9693");

            // 定义内存provider
            // 存储上下文，把历史对话、关键信息等存储到Mem0云服务中，方便后续对话的检索和利用
            // 多代理线程共享记忆，多个agent线程共享一个用户的上下文（也可隔离）
            // 使用额外的上下文提示词（ContextPrompt），为记忆检索聚焦于特定类型的信息
            var mem0Provider = new Mem0Provider(httpClient, options: new()
            {
                UserId = "U1",// 用户ID
                ScopeToPerOperationThreadId = false,// 内存是否只作用于当前线程ID，默认false，即所有线程共享上下文
                ContextPrompt = "请只关注与客户评价相关的信息"// 额外补充的提示词信息
            });
            // 清空历史的内存记录（可选）
            await mem0Provider.ClearStoredMemoriesAsync();

            agentThread.AIContextProviders.Add(mem0Provider);
        }

        // 添加内容provider（使用白板内存），为agent线程提供临时记忆能力
        {
            // 定义内存provider
            // 存储上下文中临时、重要的信息，方便后续对话的快速访问
            // 在多轮对话中聚合和提取关键信息，而不必每次重新分析全部历史消息
            // 减少传递给大模型的上下文长度，只保留关键信息，提高效率和相关性
            var whiteboardProvider = new WhiteboardProvider(chatClient, options: new()
            {
                MaxWhiteboardMessages = 6,// 白板保留的最大消息数，最大值后将删除最不相关的消息
                ContextPrompt = "请只关注与客户评价相关的信息",// 额外补充的提示词信息
                WhiteboardEmptyPrompt = "当前没有与客户评价相关的关键信息，请根据最新对话内容进行分析",// 当白板为空时的提示词模板
                // 当白板维护时的提示词模板，维护操作包括清理无关或过期信息、总结信息、触发上下文更新等
                // 支持参数：
                // {{$maxWhiteboardMessages}}：白板上允许的最大消息数
                // {{$inputMessages}}：要添加到白板的输入消息
                // {{$currentWhiteboard}}：白板的当前状态
                MaintenancePromptTemplate = "请只保留与客户评价相关的关键信息"
            });

            agentThread.AIContextProviders.Add(whiteboardProvider);
        }

        // 添加文本搜索provider
        TextSearchStore<string>? textSearchStore = null;
        {
            // TextSearchStore实例，对VectorStore的封装（包括集合操作），用于存储文本和向量搜索，泛型为集合的键值类型
            textSearchStore = new TextSearchStore<string>(
                 vectorStore: new InMemoryVectorStore(new() { EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator() }),
                 collectionName: "ReviewData",
                 vectorDimensions: 1536,
                 options: new()
                 {
                     SearchNamespace = "group/g2",// 只筛选在此命名空间下进行搜索
                 }
            );

            // 添加数据
            await textSearchStore.UpsertTextAsync([
                "The effect exceeded expectations and the cost-effectiveness is very high!",
                "Comfortable and stable to wear, the best partner for commuting."
            ]);

            // 添加引文
            await textSearchStore.UpsertDocumentsAsync([
                new TextSearchDocument
                {
                    Text = "Stunning effect, long-lasting battery life",
                    SourceName = "客户评价报告",
                    SourceLink = "https://www.dxomark.cn/",
                    Namespaces = ["group/g2"]// 为引文添加命名空间
                }
            ]);

            // 定义文本搜索provider
            // 使用向量化对本文进行语义搜索
            var textSearchProvider = new TextSearchProvider(textSearchStore, options: new()
            {
                // 控制执行文本搜索的时机
                // BeforeAIInvoke（默认值）：agent调用前则执行文本搜索
                // OnDemandFunctionCalling：通过函数调用按需执行文本搜索，以提高性能
                SearchTime = TextSearchProviderOptions.RagBehavior.OnDemandFunctionCalling,
                // 搜索返回的最大结果数
                Top = 3,
                // 插件函数名称，默认值：Search，当SearchTime设为OnDemandFunctionCalling，agent只有在需要时才会通过这个函数调用文本搜索功能
                PluginFunctionName = "GetReviewData",
                // 插件函数描述，同上
                PluginFunctionDescription = "Obtain customer evaluation data",
                ContextPrompt = "",// 额外补充的提示词信息，向AI指示用途和使用方法
                IncludeCitationsPrompt = "",// 向AI指示如何进行引文
                // 生成文本搜索的回调，当提供此委托，则不会使用ContextPrompt和IncludeCitationsPrompt设置
                ContextFormatter = (result) =>
                {
                    return "";
                }
            });

            agentThread.AIContextProviders.Add(textSearchProvider);
        }
#pragma warning restore SKEXP0110, SKEXP0130

        // 调用agent
        // 若参数中传递了agentThread，则会返回原始agentThread，否则返回一个新的agentThread
        var result = await agent.InvokeAsync("获取并总结客户评价", agentThread
        /*
        , new AgentInvokeOptions
        {
            Kernel = kernel,// 用当前内核覆盖代理的内核
            KernelArguments = null,// 用当前内核参数覆盖代理的内核参数
            AdditionalInstructions = "",// 代理附加的Instructions，只对此次调用生效
            // 生成聊天完成消息时的回调函数
            OnIntermediateMessage = (chatMessageContent) =>
            {
                return Task.CompletedTask;
            }
        }
        */
        ).FirstAsync();

        // 删除代理线程
        await agentThread.DeleteAsync();
        httpClient?.Dispose();
        textSearchStore?.Dispose();

        Console.WriteLine(result.Message.Content);

        Console.ReadLine();

        IReadOnlyList<AIFunction> GetAvailableFunctions()
        {
            // 只有少数函数与提示词相关，大部分是无关函数
            return new List<AIFunction>
            {
                // 有相关的函数
                AIFunctionFactory.Create(() => "[ { 'reviewer': 'John D.', 'date': '2023-10-01', 'rating': 5, 'comment': 'Great product and fast shipping!' } ]", "GetCustomerReviews"),
                AIFunctionFactory.Create((string text) => "Summary generated based on input data: key points include customer satisfaction.", "Summarize"),
                AIFunctionFactory.Create((string text) => "The collected sentiment is mostly positive.", "CollectSentiments"),

                // 无相关的函数
                AIFunctionFactory.Create(() => "Current weather is sunny.", "GetWeather"),
                AIFunctionFactory.Create(() => "Email sent.", "SendEmail"),
                AIFunctionFactory.Create(() => "The current stock price is $123.45.", "GetStockPrice"),
                AIFunctionFactory.Create(() => "The time is 12:00 PM.", "GetCurrentTime")
            };
        }
    }
}