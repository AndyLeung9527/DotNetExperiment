using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;
using Scalar.AspNetCore;
using System.Text;

namespace AspNetMvcVersiong;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = false;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("ver")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        })
        // MvcÊ¹ÓÃ
        .AddMvc();

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
        app.MapScalarApiReference();

        app.MapControllers();

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
