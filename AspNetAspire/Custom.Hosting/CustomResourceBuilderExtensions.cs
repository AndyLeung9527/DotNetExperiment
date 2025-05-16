using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Custom.Hosting;

public static class CustomResourceBuilderExtensions
{
    private const string UserEnvVarName = "CUSTOM_USER";
    private const string PasswordEnvVarName = "CUSTOM_PASS";

    public static IResourceBuilder<CustomResource> AddCustom(this IDistributedApplicationBuilder builder,
        string name,
        int? httpPort = null,
        int? smtpPort = null,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null)
    {
        var passwordParameter = password?.Resource ??
            ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var resource = new CustomResource(name, userName?.Resource, passwordParameter);

        return builder.AddResource(resource)
                        .WithImage("library/redis")// 镜像路径/镜像名
                        .WithImageRegistry("docker.io")
                        .WithImageTag("latest")// 镜像版本
                        .WithImagePullPolicy(ImagePullPolicy.Default)// 镜像拉取策略
                        .WithHttpEndpoint(targetPort: 1080, port: httpPort, name: CustomResource.HttpEndpointName)
                        .WithEndpoint(targetPort: 1025, port: smtpPort, name: CustomResource.SmtpEndpointName)
                        .WithEnvironment(context =>
                        {
                            context.EnvironmentVariables[UserEnvVarName] = resource.UserNameReference;
                            context.EnvironmentVariables[PasswordEnvVarName] = resource.PasswordParameter;
                        });
    }
}
