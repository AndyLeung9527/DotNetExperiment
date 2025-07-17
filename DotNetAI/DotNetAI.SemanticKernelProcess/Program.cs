using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;

namespace DotNetAI.SemanticKernelProcess;

#pragma warning disable SKEXP0080
internal class Program
{
    public static class DocGenerationEvents
    {
        public const string StartDocumentGeneration = nameof(StartDocumentGeneration);
        public const string UserRejectedDocument = nameof(UserRejectedDocument);
        public const string UserApprovedDocument = nameof(UserApprovedDocument);
    }

    public static class DocGenerationTopics
    {
        public const string RequestUserReview = nameof(RequestUserReview);
        public const string PublishDocumentation = nameof(PublishDocumentation);
    }

    const string API_KEY = "sk-0d10b1fef51846c6b1881ef0a53b4f0e";
    const string URL = "https://dashscope.aliyuncs.com/compatible-mode/v1";
    const string MODEL = "qwen-turbo";

    static async Task Main(string[] args)
    {
        var kernel = CreateKernelBuilder().Build();

        // 生成流程
        var process = CreateProcessBuilder().Build();

        // 运行流程，初始事件为DocGenerationEvents.StartDocumentGeneration
        await process.StartAsync(kernel, new KernelProcessEvent
        {
            Id = DocGenerationEvents.StartDocumentGeneration,
            Data = "Contoso GlowBrew"
        }, new CustomEventClient());

        Console.ReadLine();
    }

    public static IKernelBuilder CreateKernelBuilder()
    {
        return Kernel.CreateBuilder().AddOpenAIChatCompletion(
            modelId: MODEL,
            endpoint: new Uri(URL),
            apiKey: API_KEY
        );
    }

    public static ProcessBuilder CreateProcessBuilder()
    {
        // 创建流程builder
        string processName = "DocumentationGeneration";
        ProcessBuilder processBuilder = new(processName);

        // 添加Step
        var gatherProductInfoStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
        var generateDocumentationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
        var proofReadDocumentationStep = processBuilder.AddStepFromType<ProofReadDocumentationStep>();
        var publishDocumentationStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

        // 添加proxy step，向外部发出事件
        var proxyStep = processBuilder.AddProxyStep(processName, [DocGenerationTopics.RequestUserReview, DocGenerationTopics.PublishDocumentation]);

        // 编排外部输入事件
        processBuilder
            .OnInputEvent(DocGenerationEvents.StartDocumentGeneration)
            .SendEventTo(new(gatherProductInfoStep));

        processBuilder
            .OnInputEvent(DocGenerationEvents.UserRejectedDocument)
            .SendEventTo(new(generateDocumentationStep, functionName: GenerateDocumentationStep.ProcessFunctions.ApplySuggestions));

        processBuilder
            .OnInputEvent(DocGenerationEvents.UserApprovedDocument)
            .SendEventTo(new(publishDocumentationStep, parameterName: "userApproval"));

        // 编排内部Step
        gatherProductInfoStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(generateDocumentationStep, functionName: GenerateDocumentationStep.ProcessFunctions.GenerateDocs));

        generateDocumentationStep
            .OnEvent(GenerateDocumentationStep.OutputEvents.DocumentationGenerated)
            .SendEventTo(new ProcessFunctionTargetBuilder(proofReadDocumentationStep));

        proofReadDocumentationStep
            .OnEvent(ProofReadDocumentationStep.OutputEvents.DocumentationRejected)
            .SendEventTo(new ProcessFunctionTargetBuilder(generateDocumentationStep, functionName: GenerateDocumentationStep.ProcessFunctions.ApplySuggestions));

        proofReadDocumentationStep
            .OnEvent(ProofReadDocumentationStep.OutputEvents.DocumentationApproved)
            .EmitExternalEvent(proxyStep, DocGenerationTopics.RequestUserReview)// 发送外部事件，主题：RequestUserReview
            .SendEventTo(new ProcessFunctionTargetBuilder(publishDocumentationStep));

        publishDocumentationStep
            .OnFunctionResult()
            .EmitExternalEvent(proxyStep, DocGenerationTopics.PublishDocumentation);// 发送外部事件，主题PublishDocumentation

        return processBuilder;
    }
}

// 收集产品信息的Step（无状态Step）
// 每个Step至少有一个KernelFunction，作为Step的可调用成员
public class GatherProductInfoStep : KernelProcessStep
{
    [KernelFunction]
    public ProductInfo GatherProductInformation(ProductInfo productInfo)
    {
        Console.WriteLine($"[{nameof(GatherProductInfoStep)}]:\tGathering product information for product named {productInfo.Title}");

        // 返回产品信息
        productInfo.Content = """
            Product Description:
            GlowBrew is a revolutionary AI driven coffee machine with industry leading number of LEDs and programmable light shows. The machine is also capable of brewing coffee and has a built in grinder.
            
            Product Features:
            1. **Luminous Brew Technology**: Customize your morning ambiance with programmable LED lights that sync with your brewing process.
            2. **AI Taste Assistant**: Learns your taste preferences over time and suggests new brew combinations to explore.
            3. **Gourmet Aroma Diffusion**: Built-in aroma diffusers enhance your coffee's scent profile, energizing your senses before the first sip.
            
            Troubleshooting:
            - **Issue**: LED Lights Malfunctioning
                - **Solution**: Reset the lighting settings via the app. Ensure the LED connections inside the GlowBrew are secure. Perform a factory reset if necessary.
            """;

        return productInfo;
    }
}

// 为产品生成文档的Step（有状态Step，使用泛型GeneratedDocumentationState来持久化ChatHistory和LastGeneratedDocument）
public class GenerateDocumentationStep : KernelProcessStep<GeneratedDocumentationState>
{
    public static class ProcessFunctions
    {
        public const string GenerateDocs = nameof(GenerateDocs);
        public const string ApplySuggestions = nameof(ApplySuggestions);
    }

    public static class OutputEvents
    {
        public const string DocumentationGenerated = nameof(DocumentationGenerated);
    }

    private GeneratedDocumentationState _state = new();

    private readonly string _systemPrompt = """
        Your job is to write high quality and engaging customer facing documentation for a new product from Contoso. You will be provide with information
        about the product in the form of internal documentation, specs, and troubleshooting guides and you must use this information and
        nothing else to generate the documentation. If suggestions are provided on the documentation you create, take the suggestions into account and
        rewrite the documentation. Make sure the product sounds amazing.
        """;

    // 当Step被激活时调用
    public override ValueTask ActivateAsync(KernelProcessStepState<GeneratedDocumentationState> state)
    {
        _state = state.State!;
        _state.ChatHistory ??= new ChatHistory(_systemPrompt);

        return base.ActivateAsync(state);
    }

    // KernelProcessStepContext参数实例会被框架自动注入
    [KernelFunction(ProcessFunctions.GenerateDocs)]
    public async Task GenerateDocumentationAsync(Kernel kernel, KernelProcessStepContext context, ProductInfo productInfo)
    {
        Console.WriteLine($"[{nameof(GenerateDocumentationStep)}]:\tGenerating documentation for provided productInfo...");

        // 历史对话中增加一个产品信息
        _state.ChatHistory!.AddUserMessage($"Product Info:\n{productInfo.Title} - {productInfo.Content}");

        // 从LLM获取结果
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(_state.ChatHistory!);

        DocumentInfo generatedContent = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            Title = $"Generated document - {productInfo.Title}",
            Content = generatedDocumentationResponse.Content ?? string.Empty
        };

        _state.LastGeneratedDocument = generatedContent;

        // 手动发出事件
        await context.EmitEventAsync(OutputEvents.DocumentationGenerated, generatedContent);
    }

    [KernelFunction(ProcessFunctions.ApplySuggestions)]
    public async Task ApplySuggestionsAsync(Kernel kernel, KernelProcessStepContext context, string suggestions)
    {
        Console.WriteLine($"[{nameof(GenerateDocumentationStep)}]:\tRewriting documentation with provided suggestions...");

        // 增加一个产品信息
        _state.ChatHistory!.AddUserMessage($"Rewrite the documentation with the following suggestions:\n\n{suggestions}");

        // 从LLM获取结果
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(_state.ChatHistory!);

        DocumentInfo updatedContent = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            Title = $"Revised - {_state.LastGeneratedDocument.Title}",
            Content = generatedDocumentationResponse.Content ?? string.Empty
        };

        _state.LastGeneratedDocument = updatedContent;

        await context.EmitEventAsync(OutputEvents.DocumentationGenerated, updatedContent);
    }
}

// 文档校对Step
public class ProofReadDocumentationStep : KernelProcessStep
{
    public static class OutputEvents
    {
        public const string DocumentationRejected = nameof(DocumentationRejected);
        public const string DocumentationApproved = nameof(DocumentationApproved);
    }

    private readonly string _systemPrompt = """"
        Your job is to proofread customer facing documentation for a new product from Contoso. You will be provide with proposed documentation
        for a product and you must do the following things:

        1. Determine if the documentation is passes the following criteria:
            1. Documentation must use a professional tone.
            1. Documentation should be free of spelling or grammar mistakes.
            1. Documentation should be free of any offensive or inappropriate language.
            1. Documentation should be technically accurate.
        2. If the documentation does not pass 1, you must write detailed feedback of the changes that are needed to improve the documentation. 
        """";

    [KernelFunction]
    public async Task ProofreadDocumentationAsync(Kernel kernel, KernelProcessStepContext context, DocumentInfo document)
    {
        ChatHistory chatHistory = new ChatHistory(_systemPrompt);
        chatHistory.AddUserMessage(document.Content);

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var proofreadResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings
        {
            ResponseFormat = typeof(ProofreadingResponse)
        });
        var formattedResponse = JsonSerializer.Deserialize<ProofreadingResponse>(proofreadResponse.Content ?? string.Empty);

        Console.WriteLine($"[{nameof(ProofReadDocumentationStep)}]:\n\tGrade = {(formattedResponse!.MeetsExpectations ? "Pass" : "Fail")}\n\tExplanation = {formattedResponse.Explanation}\n\tSuggestions = {string.Join("\n\t\t", formattedResponse.Suggestions)}");

        // 根据LLM对生成文档的评分，发出Approved或Rejected事件
        if (formattedResponse.MeetsExpectations)
        {
            await context.EmitEventAsync(OutputEvents.DocumentationApproved,
                data: document,
                visibility: KernelProcessEventVisibility.Public// 将事件标记为公开，默认Internal
            );
        }
        else
        {
            await context.EmitEventAsync(OutputEvents.DocumentationRejected, data: $"Explanation = {formattedResponse.Explanation}, Suggestions = {string.Join(",", formattedResponse.Suggestions)} ");
        }
    }
}

// 发布文档的Step（无状态Step）
public class PublishDocumentationStep : KernelProcessStep
{
    [KernelFunction]
    public DocumentInfo OnPublishDocumentation(DocumentInfo document, bool userApproval)
    {
        // 当approved才发布
        // process第一次运行时没有此参数，直接return，结束
        // process第二次运行（由外部事件IExternalKernelProcessMessageChannel触发执行），userApproval为true则执行发布Step
        if (userApproval)
        {
            Console.WriteLine($"[{nameof(PublishDocumentationStep)}]:\tPublishing product documentation approved by user: \n{document.Title}\n{document.Content}");
        }

        return document;
    }
}

// 自定义外部的事件处理（可用作外部人工干预，是否同意process的后续操作）
public class CustomEventClient : IExternalKernelProcessMessageChannel
{
    public ValueTask Initialize()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Uninitialize()
    {
        return ValueTask.CompletedTask;
    }

    public async Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        switch (externalTopicEvent)
        {
            case "RequestUserReview":
                var requestDocument = message.EventData?.ToObject() as DocumentInfo;
                // 实际应该使用中介者模式，使用相同的kernel，重新调用process执行
                {
                    var kernel = Program.CreateKernelBuilder().Build();
                    var process = Program.CreateProcessBuilder().Build();
                    // 运行流程，初始事件为DocGenerationEvents.UserApprovedDocument或UserRejectedDocument
                    await process.StartAsync(kernel, new KernelProcessEvent
                    {
                        Id = Program.DocGenerationEvents.UserApprovedDocument,// UserRejectedDocument
                        Data = true
                    });
                }
                return;
            case "PublishDocumentation":
                Console.WriteLine("外部已接收文档发布消息");
                return;
        }
    }
}

public class GeneratedDocumentationState
{
    public DocumentInfo LastGeneratedDocument { get; set; } = new();
    public ChatHistory? ChatHistory { get; set; }
}

public class DocumentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ProductInfo
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserInput { get; set; } = string.Empty;
}

public class ProofreadingResponse
{
    [Description("Specifies if the proposed documentation meets the expected standards for publishing.")]
    public bool MeetsExpectations { get; set; }

    [Description("An explanation of why the documentation does or does not meet expectations.")]
    public string Explanation { get; set; } = "";

    [Description("A lis of suggestions, may be empty if there no suggestions for improvement.")]
    public List<string> Suggestions { get; set; } = [];
}
#pragma warning restore SKEXP0080