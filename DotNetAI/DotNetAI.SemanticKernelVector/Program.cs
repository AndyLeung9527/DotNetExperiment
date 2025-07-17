using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;

namespace DotNetAI.SemanticKernelVector;

internal class Program
{
    const string API_KEY = "sk-0d10b1fef51846c6b1881ef0a53b4f0e";
    const string URL = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    const string MODEL = "text-embedding-v4";

    async static Task Main(string[] args)
    {
        // 定义方法：使用大模型生成文本嵌入向量（需要大模型和服务支持）
        var kernel = Kernel.CreateBuilder().AddOpenAITextEmbeddingGeneration(
            modelId: MODEL,
            apiKey: API_KEY,
            httpClient: new HttpClient { BaseAddress = new Uri(URL) }
        ).Build();

        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // 创建向量存储实例，使用内存向量数据库（实际应用中，需要使用持久化的向量数据库，如Redis、MongoDB、Qdrant等）
        // Using Kernel Builder：kernelBuilder.Services.AddInMemoryVectorStore();
        // Using IServiceCollection with ASP.NET Core：builder.Services.AddInMemoryVectorStore();
        var vectorStore = new InMemoryVectorStore();

        // 自动生成向量（模型VectorStoreVector特性需保留）方式1：在向量存储上注册自动生成的嵌入向量生成器
        //var vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions
        //{
        //    EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator()
        //});

        // 根据静态类型进行向量搜索

        {
            // 从数据库中选择一个集合，指定集合的键值类型
            var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels");

            // 自动生成向量方式2：在集合定义时注册自动生成的嵌入向量生成器
            //var collection = new InMemoryCollection<ulong, Hotel>("skhotels", new InMemoryCollectionOptions
            //{
            //    EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator()
            //});

            // 使用自定义存储架构，数据模型可以与特性解耦
            //{
            //    var hotelDefintion = new VectorStoreCollectionDefinition
            //    {
            //        Properties = new List<VectorStoreProperty>
            //        {
            //            new VectorStoreKeyProperty("HotelId",typeof(ulong)),
            //            new VectorStoreDataProperty("HotelName", typeof(string)) { IsIndexed = true },
            //            new VectorStoreDataProperty("Description", typeof(string)) { IsFullTextIndexed = true },
            //            new VectorStoreVectorProperty("DescriptionEmbedding", typeof(float), dimensions: 1536) { DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw }

            //            // 自动生成向量方式4：在向量属性定义中注册的嵌入向量生成器
            //            //new VectorStoreVectorProperty("DescriptionEmbedding", typeof(float), dimensions: 1536)
            //            //{ 
            //            //    EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator(),
            //            //}
            //        },

            //        // 自动生成向量方式3：在存储架构定义时注册自动生成的嵌入向量生成器
            //        //EmbeddingGenerator = textEmbeddingGenerationService.AsEmbeddingGenerator()
            //    };
            //    collection = vectorStore.GetCollection<ulong, Hotel>("skhotels", hotelDefintion);
            //}

            // 如果集合不存在，则创建集合
            await collection.EnsureCollectionExistsAsync();

            // 向集合中添加数据，并手动生成描述的向量（无需自动生成向量时）
            string descText = "这是一家位于佛山的豪华酒店，提供一流的服务和设施。酒店拥有宽敞的客房、精致的餐厅和健身中心。适合商务旅行和休闲度假。";
            ulong hotelId = 1;
            await collection.UpsertAsync(new Hotel
            {
                HotelId = hotelId,
                HotelName = "2213夜总会",
                Description = descText,
                DescriptionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(descText),// 手动生成向量
                Tags = ["豪华", "佛山", "酒店", "商务"]
            });

            // 查询集合中的数据
            Hotel? retrievedHotel = await collection.GetAsync(hotelId);

            ReadOnlyMemory<float> searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync("我需要找一家佛山的豪华商务酒店");

            // 执行向量搜索，查找与给定描述相似的酒店
            var searchResult = collection.SearchAsync(searchVector, top: 1, new VectorSearchOptions<Hotel>
            {
                // 当数据模型指定多个向量属性（VectorStoreVector）时，搜索时需要指定使用哪个向量属性
                VectorProperty = r => r.DescriptionEmbedding,
                IncludeVectors = false,// 指定结果是否包含向量数据，如果false减少回传数据量可以更高效，默认为false
                Filter = r => r.Tags.Contains("佛山") // 筛选器，更高效地过滤结果，减少向量匹配的计算量
            });
            await foreach (var record in searchResult)
            {
                Console.WriteLine($"找到酒店：{record.Record.HotelName}，描述：{record.Record.Description}，匹配度得分：{record.Score}");
            }

            // 执行混合搜索，即向量搜索和关键词搜索的结合，并行执行，之后将返回两个结果集的并集
            // 在需要进行关键字搜索的字段上加上IsFullTextIndexed
            // 支持混合搜索的连接器（VectorStore->VectorStoreCollection）才实现了IKeywordHybridSearchable接口
            searchResult = (collection as IKeywordHybridSearchable<Hotel>)?.HybridSearchAsync(searchVector, ["商务", "度假"], top: 1, new HybridSearchOptions<Hotel>
            {
                // 当数据模型指定多个全文索引属性（[VectorStoreData(IsFullTextIndexed = true)]）时，搜索时需要指定使用哪个全文索引属性
                AdditionalProperty = r => r.Description
                // 其他选项同上
            });
            await foreach (var record in searchResult ?? AsyncEnumerable.Empty<VectorSearchResult<Hotel>>())
            {
                Console.WriteLine($"找到酒店：{record.Record.HotelName}，描述：{record.Record.Description}，匹配度得分：{record.Score}");
            }
        }


        // 根据动态类型进行向量搜索
        /*
        {
            // 自定义存储架构
            VectorStoreCollectionDefinition definition = new()
            {
                Properties = new List<VectorStoreProperty>
                {
                    new VectorStoreKeyProperty("Key", typeof(string)),
                    new VectorStoreDataProperty("Term", typeof(string)),
                    new VectorStoreDataProperty("Definition", typeof(string)),
                    new VectorStoreVectorProperty("DefinitionEmbedding", typeof(ReadOnlyMemory<float>), dimensions: 1536)
                }
            };

            // 从数据库中选择一个集合，使用动态类型
            var dynamicDataModelCollection = vectorStore.GetDynamicCollection("glossary", definition);

            // 如果集合不存在，则创建集合
            await dynamicDataModelCollection.EnsureCollectionExistsAsync();

            // 向集合中添加数据，并生成描述的向量
            await dynamicDataModelCollection.UpsertAsync(new Dictionary<string, object?>
            {
                { "Key", "1" },
                { "Term", "First" },
                { "Definition", "第一" },
                { "DefinitionEmbedding", await textEmbeddingGenerationService.GenerateEmbeddingAsync("第一") }
            });

            // 返回字典类
            var record = await dynamicDataModelCollection.GetAsync("1");

            Console.WriteLine(record?.GetValueOrDefault("Definition"));
        }
        */

        Console.ReadLine();
    }
}

// 向量数据库的数据模型
public class Hotel
{
    // 向量存储的主键，必须唯一
    [VectorStoreKey]
    public ulong HotelId { get; set; }

    // 属性编制索引
    [VectorStoreData(IsIndexed = true)]
    public string HotelName { get; set; } = string.Empty;

    // 属性编制全文索引
    [VectorStoreData(IsFullTextIndexed = true)]
    public string Description { get; set; } = string.Empty;

    // 向量数据
    // Dimensions: 向量的维度，需要符合向量模型的输出范围
    // DistanceFunction：CosineSimilarity（余弦相似度）是常用的向量距离函数
    // IndexKind：Hnsw（Hierarchical Navigable Small World）是常用的向量索引类型
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    // 属性编制索引
    [VectorStoreData(IsIndexed = true)]
    public string[] Tags { get; set; } = Array.Empty<string>();
}