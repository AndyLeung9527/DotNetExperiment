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

        // ���API�汾����
        builder.Services.AddApiVersioning(options =>
        {
            // Ĭ�ϰ汾
            options.DefaultApiVersion = new ApiVersion(1.0);
            // ��Ӧͷ�г�֧�ֵ����а汾
            options.ReportApiVersions = true;
            // ��δָ���汾ʱ��ʹ��Ĭ�ϰ汾������404
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(), // ��URL�ζ�ȡ�汾, https://xxx/api/v1/xxxxxx
                new QueryStringApiVersionReader("api-version"),// �Ӳ�ѯ�ַ�����ȡ�汾, https://xxx?api-version=1
                new HeaderApiVersionReader("x-api-version"),// ������ͷ��ȡ�汾, https://xxx, Header: x-api-version: 1
                new MediaTypeApiVersionReader("ver")// ��ý�����Ͷ�ȡ�汾, // https://xxx, Header: Content-Type: application/json; ver=1
            );
        })
        // ���API�汾��������ͨ����������API�ĵ���OpenAPI/Swagger��
        .AddApiExplorer(options =>
        {
            // �汾������������ǰ׺��Ĭ��ApiVersion.ToString()������"1.0"��"2.0"
            options.GroupNameFormat = "'v'VVV";// �汾�ĸ�ʽ���ַ�����"'v'major[.minor]"������ "v1.0" �� "v2.0"
            // ·��ģ��İ汾ռλ���ᱻ�滻Ϊʵ�ʰ汾�ţ���������API�ĵ�
            options.SubstituteApiVersionInUrl = true;
        });

        // OpenApi�ĵ�����
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
        // /scalar/v1��/scalar/v2
        app.MapScalarApiReference();

        // ����һ�����汾���Ƶ�API·�ɷ��飬·�ɡ�api/v/demo�������ð汾1.0��֧��2.0
        var demo = app.NewVersionedApi("Demo")
            .MapGroup("api/v{apiVersion:apiVersion}/demo")
            .HasDeprecatedApiVersion(new ApiVersion(1.0))
            .HasApiVersion(new ApiVersion(2.0));

        // demo.MapGet("/", () => "Hello World!");// ��ֻ��һ���汾ʱ������ʡ�԰汾��
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
