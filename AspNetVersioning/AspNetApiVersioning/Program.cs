using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;
using Scalar.AspNetCore;
using System.Text;

namespace AspNetApiVersioning;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 添加API版本控制
        builder.Services.AddApiVersioning(options =>
        {
            // 默认版本
            options.DefaultApiVersion = new ApiVersion(1.0);
            // 响应头列出支持的所有版本
            options.ReportApiVersions = true;
            // 当未指定版本时，使用默认版本，否则404
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(), // 从URL段读取版本, https://xxx/api/v1/xxxxxx
                new QueryStringApiVersionReader("api-version"),// 从查询字符串读取版本, https://xxx?api-version=1
                new HeaderApiVersionReader("x-api-version"),// 从请求头读取版本, https://xxx, Header: x-api-version: 1
                new MediaTypeApiVersionReader("ver")// 从媒体类型读取版本, // https://xxx, Header: Content-Type: application/json; ver=1
            );
        })
        // 添加API版本描述器，通常用于生成API文档（OpenAPI/Swagger）
        .AddApiExplorer(options =>
        {
            // 版本描述器的名称前缀，默认ApiVersion.ToString()，比如"1.0"或"2.0"
            options.GroupNameFormat = "'v'VVV";// 版本的格式化字符串，"'v'major[.minor]"，例如 "v1.0" 或 "v2.0"
            // 路由模板的版本占位符会被替换为实际版本号，用于生成API文档
            options.SubstituteApiVersionInUrl = true;
        });

        // OpenApi文档生成
        string[] versions = ["v1", "v2"];
        foreach (var version in versions)
        {
            builder.Services.AddOpenApi(version, options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    var versionDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    var apiDescription = versionDescriptionProvider?.ApiVersionDescriptions.SingleOrDefault(description => description.GroupName == context.DocumentName);
                    if (apiDescription is null)
                    {
                        return Task.CompletedTask;
                    }
                    document.Info.Version = apiDescription.ApiVersion.ToString();
                    document.Info.Title = $"Demo API {apiDescription.ApiVersion}";
                    document.Info.Description = BuildDescription(apiDescription, $"Demo API for version {apiDescription.ApiVersion}");
                    return Task.CompletedTask;
                });
            });
        }


        var app = builder.Build();
        app.MapOpenApi();
        // /scalar/v1或/scalar/v2
        app.MapScalarApiReference();

        // 创建一个带版本控制的API路由分组，路由“api/v/demo”，弃用版本1.0，支持2.0
        var demo = app.NewVersionedApi("Demo")
            .MapGroup("api/v{apiVersion:apiVersion}/demo")
            .HasDeprecatedApiVersion(new ApiVersion(1.0))
            .HasApiVersion(new ApiVersion(2.0));

        // demo.MapGet("/", () => "Hello World!");// 当只有一个版本时，可以省略版本号
        demo.MapGet("/", () => "Hello World v1.0!").MapToApiVersion(1);
        demo.MapGet("/", () => "Hello World v2.0!").MapToApiVersion(2);

        app.Run();
    }

    private static string BuildDescription(ApiVersionDescription api, string description)
    {
        var text = new StringBuilder(description);

        if (api.IsDeprecated)
        {
            if (text.Length > 0)
            {
                if (text[^1] != '.')
                {
                    text.Append('.');
                }

                text.Append(' ');
            }

            text.Append("This API version has been deprecated.");
        }

        if (api.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }

                text.Append("The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
                {
                    if (!rendered)
                    {
                        text.Append("<h4>Links</h4><ul>");
                        rendered = true;
                    }

                    text.Append("<li><a href=\"");
                    text.Append(link.LinkTarget.OriginalString);
                    text.Append("\">");
                    text.Append(
                        StringSegment.IsNullOrEmpty(link.Title)
                        ? link.LinkTarget.OriginalString
                        : link.Title.ToString());
                    text.Append("</a></li>");
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        return text.ToString();
    }
}
