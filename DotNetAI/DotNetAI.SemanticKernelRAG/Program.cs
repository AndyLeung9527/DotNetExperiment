using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace DotNetAI.SemanticKernelRAG;

// RAG（Retrieval-Augmented Generation），检索增强生成
// 从外部知识库中检索相关信息，发送到LLM的prompt，LLM根据上下文提供更准确的响应
internal class Program
{
    const string API_KEY = "sk-0d10b1fef51846c6b1881ef0a53b4f0e";
    const string URL = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    const string CHAT_MODEL = "qwen-turbo";
    const string VECTOR_MODEL = "text-embedding-v4";

    async static Task Main(string[] args)
    {

        var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: CHAT_MODEL,
            endpoint: new Uri(URL),
            apiKey: API_KEY
        ).AddOpenAITextEmbeddingGeneration(
            modelId: VECTOR_MODEL,
            apiKey: API_KEY,
            httpClient: new HttpClient { BaseAddress = new Uri(URL) }
        );
        var kernel = builder.Build();

        // 提示词模板使用Handlebars语法，SearchPlugin-DemoSearch为搜索插件和函数名称，query为输入变量
        var template = """
            {{#with (SearchPlugin-DemoSearch query)}}  
                {{#each this}}  
                Name: {{Name}}
                Value: {{Value}}
                Link: {{Link}}
                -----------------
                {{/each}}  
            {{/with}}  

            {{query}}

            Include citations to the relevant information where it is referenced in the response.
            """;

        var query = "什么是语义内核？";
        var arguments = new KernelArguments
        {
            ["query"] = query
        };

        // 使用联网的搜索引擎进行RAG
        /*
        {
#pragma warning disable SKEXP0050, SKEXP0001
            // 创建Google文本搜索实例（Bing搜索：new BingTextSearch()）
            var textSearch = new GoogleTextSearch(searchEngineId: "942abbb725a8f439f", apiKey: "AIzaSyBkrnm06kJEG-guiPkJUa9Yj0Izdnx418A");

            // 执行搜索，三种方式
            {
                // 方式一（Search）：执行搜索，只获取结果的字符串值（内容）
                //KernelSearchResults<string> searchResults = await textSearch.SearchAsync(query, new() { Top = 2 });
                //await foreach (string result in searchResults.Results)
                //{
                //    Console.WriteLine(result);
                //}

                // 方式二（GetSearchResults）：执行搜索，获取结果为自定义的格式（例子是Bing搜索结果的Web页面格式）
                //KernelSearchResults<object> webPages = await textSearch.GetSearchResultsAsync(query, new() { Top = 2 });
                //await foreach (BingWebPage webPage in webPages.Results)
                //{
                //    Console.WriteLine($"Name:            {webPage.Name}");
                //    Console.WriteLine($"Snippet:         {webPage.Snippet}");
                //    Console.WriteLine($"Url:             {webPage.Url}");
                //    Console.WriteLine($"DisplayUrl:      {webPage.DisplayUrl}");
                //    Console.WriteLine($"DateLastCrawled: {webPage.DateLastCrawled}");
                //}

                // 方式三（GetTextSearch）：执行搜索，获取结果的规范化数据模型（TextSearchResult，包括名称（标题）、字符串值（内容）、链接（可选））
                KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2 });
                await foreach (TextSearchResult result in textResults.Results)
                {
                    Console.WriteLine($"Name:  {result.Name}");
                    Console.WriteLine($"Value: {result.Value}");
                    Console.WriteLine($"Link:  {result.Link}");
                }
            }

            // 将搜索转换为插件并加入到SK，供LLM调用
            var searchPlugin = kernel.CreatePluginFromFunctions(
                pluginName: "SearchPlugin",
                description: "Search Microsoft Developer Blogs site only",
                functions: [
                    // 同样有对应上述的三种执行搜索方式
                    //textSearch.CreateSearch(),// 方式一（Search，FunctionName默认是"Search"）
                    //textSearch.CreateGetSearchResults(),// 方式二（GetSearchResults，FunctionName默认是"GetSearchResults"）
                    //textSearch.CreateGetTextSearchResults(),// 方式三（GetTextSearch，FunctionName默认是"GetTextSearchResults"）
                    textSearch.CreateGetTextSearchResults(
                        // 自定义插件选项
                        options: new KernelFunctionFromMethodOptions
                        {
                            // 插件函数名称
                            FunctionName = "DemoSearch",
                            // 插件函数描述
                            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
                            // 插件函数参数描述
                            Parameters =
                            [
                                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                                new KernelParameterMetadata("top") { Description = "Number of results", IsRequired = false, DefaultValue = 2 },
                                new KernelParameterMetadata("skip") { Description = "Number of results to skip", IsRequired = false, DefaultValue = 0 },
                                new KernelParameterMetadata("site") { Description = "Only return results from this domain", IsRequired = false },
                            ],
                            // 插件函数返回值类型
                            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<TextSearchResult>) },
                        },
                        // 自定义搜索选项
                        searchOptions: new TextSearchOptions
                        {
                            Skip = 0, //跳过的结果数
                            Top = 2, //返回的结果数
                            IncludeTotalCount = false, //是否包含总结果数
                            //搜索筛选器，指定只搜索来自devblogs.microsoft.com的站点（'siteSearch'特定于Google，Bing使用'site'）
                            Filter = new TextSearchFilter().Equality("siteSearch", "devblogs.microsoft.com")
                        }
                    )
                ]
            );
            kernel.Plugins.Add(searchPlugin);
#pragma warning restore SKEXP0050, SKEXP0001
        }
        */

        // 使用向量数据库搜索进行RAG
        /*
        {
            var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            var vectorStore = new InMemoryVectorStore();

            var collection = vectorStore.GetCollection<Guid, DataModel>("datamodels");
            await collection.EnsureCollectionExistsAsync();
            string text = "语义内核是一种轻型开源开发工具包，可用于轻松生成AI 代理并将最新的AI 模型集成到C#、Python 或Java 代码库中。";
            await collection.UpsertAsync(new DataModel
            {
                Key = Guid.NewGuid(),
                Text = text,
                Link = "https://learn.microsoft.com/zh-cn/semantic-kernel/overview/",
                Embedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text)
            });

#pragma warning disable SKEXP0001
            // 自定义映射器，将DataModel的结果映射为字符串值（内容），对应上述搜索方式一（Search）
            var stringMapper = new DataModelTextSearchStringMapper();
            // 自定义映射器，将DataModel的结果映射为TextSearchResult对象，对应上述搜索方式三（GetTextSearch）
            var resultMapper = new DataModelTextSearchResultMapper();

            // 创建根据向量搜索的搜索实例，特性映射和自定义映射二选一
            var textSearch = new VectorStoreTextSearch<DataModel>(
                vectorSearchable: collection,
                embeddingGenerator: textEmbeddingGenerationService.AsEmbeddingGenerator(),
                stringMapper: stringMapper, // 自定义映射器，可选
                resultMapper: resultMapper // 自定义映射器，可选
            );
#pragma warning restore SKEXP0001

            // 执行搜索，并映射为规范化的结果返回
            {
                //KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });
                //await foreach (TextSearchResult result in textResults.Results)
                //{
                //    Console.WriteLine($"Name:  {result.Name}");
                //    Console.WriteLine($"Value: {result.Value}");
                //    Console.WriteLine($"Link:  {result.Link}");
                //}
            }

            // 将搜索转换为插件并加入到SK，供LLM调用
            var searchPlugin = kernel.CreatePluginFromFunctions(
                pluginName: "SearchPlugin",
                description: "Search a record collection",
                functions: [
                    // 同样有对应上述的三种执行搜索方式
                    textSearch.CreateGetTextSearchResults(options: new(){
                        FunctionName = "DemoSearch",
                        Description = "Perform a search for content related to the specified query from a record collection.",
                        Parameters =
                        [
                            new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                            new KernelParameterMetadata("top") { Description = "Number of results", IsRequired = false, DefaultValue = 2 },
                            new KernelParameterMetadata("skip") { Description = "Number of results to skip", IsRequired = false, DefaultValue = 0 },
                        ],
                        ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<TextSearchResult>) }
                    })
                ]
            );
            kernel.Plugins.Add(searchPlugin);
        }
        */

        var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate: template,
            arguments: arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory));

        Console.ReadLine();
    }
}

// 向量数据模型
// 可使用特性将其映射为TextSearchResult，用于返回规范化的搜索结果
public sealed class DataModel
{
    // 映射到TextSearchResult的名称
    [TextSearchResultName]
    [VectorStoreKey]
    public Guid Key { get; init; }

    // 映射到TextSearchResult的值（内容）
    [TextSearchResultValue]
    [VectorStoreData]
    public string Text { get; init; } = string.Empty;

    // 映射到TextSearchResult的链接
    [TextSearchResultLink]
    [VectorStoreData]
    public string Link { get; init; } = string.Empty;

    [VectorStoreData]
    public string Tag { get; init; } = string.Empty;

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; init; } = ReadOnlyMemory<float>.Empty;
}

public sealed class DataModelTextSearchStringMapper : ITextSearchStringMapper
{
    public string MapFromResultToString(object result)
    {
        if (result is DataModel dataModel)
        {
            return dataModel.Text;
        }

        throw new ArgumentException("Invalid result type.", nameof(result));
    }
}

public sealed class DataModelTextSearchResultMapper : ITextSearchResultMapper
{
    public TextSearchResult MapFromResultToTextSearchResult(object result)
    {
        if (result is DataModel dataModel)
        {
            return new TextSearchResult(dataModel.Text)
            {
                Name = dataModel.Key.ToString(),
                Link = dataModel.Link
            };
        }

        throw new ArgumentException("Invalid result type.", nameof(result));
    }
}