using Custom.Hosting;// ��Aspire����ʱ���IsAspireProjectResource="false", �Ա�ע�����Ƿ�����

var builder = DistributedApplication.CreateBuilder(args);

//������Դ, �������ļ�appsettings.json��secret.json��ȡ
var username = builder.AddParameter("custom-username");
var password = builder.AddParameter("custom-password");

var customResource = builder.AddCustom("customresource", userName: username, password: password);// ����Զ���������Դ

var api = builder.AddProject<Projects.Custom_Api>("customapi")// ���ӷ���
            .WithReference(customResource)// ����������Դ
            .WithCommand// �ڲ�������Actions������һ���Զ�����Դ����
            (
                name: "clear-cache",// �������������
                displayName: "Clear Cache",// �����������ʾ�����������
                executeCommand: context => Task.FromResult(CommandResults.Success()),// ��������ʱ����
                commandOptions: new CommandOptions
                {
                    Description = "Clear the cache",// �����������ʾ�����������
                    Parameter = null,// ���ú�̨���ݵĲ���, Ҳ����������ʹ��context.Parameters["param"]��ȡ
                    ConfirmationMessage = "Are you sure you want to clear the cache?",// ��ִ������ǰ����ȷ����Ϣ
                    IconName = "AnimalRabbitOff",// �����������ʾ��ͼ�������
                    IconVariant = IconVariant.Filled,// �����������ʾ��ͼ�����ʽ
                    IsHighlighted = true,// �Ƿ������ʾ
                    UpdateState = context => ResourceCommandState.Enabled,// ���ûص���ȷ�������"����"״̬
                }
            )
            .WithHttpCommand// �ڲ�������Actions������һ���Զ���http��Դ����
            (
                path: "/httpCommand",// http����·��
                displayName: "Http Command",
                commandOptions: new HttpCommandOptions
                {
                    Description = "Http Command",
                    // ��������ǰ�Ļص�����
                    PrepareRequest = context =>
                    {
                        context.Request.Headers.Add("X-Invalidation-Key", "123456");
                        return Task.CompletedTask;
                    }
                }
            );
//.WaitFor(customResource);

// ���url
api.WithUrl($"{api.GetEndpoint("https")}/", "Visit hello word");
// �Զ���Endpoint��url
api.WithUrlForEndpoint("https", url =>
{
    url.DisplayText = "HTTPS";
});


// ������Դ, �������ļ�appsettings.json��secret.json��ȡ
var nginxVersion = builder.AddParameter("nginxversion");
var container1 = builder.AddDockerfile("mynginx", "dockerfiles/1")// ��Dockerfile��ӵ���Դ, ���·��Ϊ��ǰ.csproj��·��
                        .WithBuildArg("NGINX_VERSION", nginxVersion)// ������Դ������ִ�а汾, ����ʱָ��, ��ӦDockerfile�е�ARG����; Ҳ��ֱ�Ӵ��ַ���ָ���汾
                        .WithBindMount("/VolumeMount/Nginx/home/web", "/home/web");// �������ݾ�, ��һ������Ϊ���ش洢λ��, �ڶ�������Ϊ������·��
//.WithDockerfile("path");// ͨ��Dockerfile�Զ�������������Դ, ���·��Ϊ��ǰ.csproj��·��
//.WithBuildSecret("ACCESS_TOKEN", accessToken);// ָ��������Կ

var connectionString = builder.AddConnectionString("mysql");// ���������ַ���, �������ļ������ã��ڶ�������֧�ִ���������

// ֧������docker-compose.yml�ļ�������ĿĿ¼ִ��'dotnet run --publisher docker-compose --output-path ./docker-compose'�������л�����docker����
builder.AddDockerComposePublisher();

builder.Build().Run();

/*
Aspire�����嵥����:
dotnet run --project AspNetAspire.AppHost/AspNetAspire.AppHost.csproj -- --publisher manifest --output-path aspire-manifest.json
*/