using Aspire.Hosting.ApplicationModel;

namespace Custom.Hosting;

/// <summary>
/// 自定义容器资源
/// </summary>
public sealed class CustomResource(string name,
    ParameterResource? username,
    ParameterResource password) : ContainerResource(name), IResourceWithConnectionString
{
    internal const string SmtpEndpointName = "smtp";
    internal const string HttpEndpointName = "http";

    private const string DefaultUsername = "custom-user";

    private EndpointReference? _smptReference;

    public ParameterResource? UsernameParameter { get; } = username;

    internal ReferenceExpression UserNameReference =>
        UsernameParameter is not null ?
        ReferenceExpression.Create($"{UsernameParameter}") :
        ReferenceExpression.Create($"{DefaultUsername}");

    internal ParameterResource PasswordParameter { get; } = password;

    public EndpointReference SmptEndPoint =>
        _smptReference ??= new(this, SmtpEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"smtp://{SmptEndPoint.Property(EndpointProperty.Host)}:{SmptEndPoint.Property(EndpointProperty.Port)};Username={UserNameReference};Password={PasswordParameter}");
}
