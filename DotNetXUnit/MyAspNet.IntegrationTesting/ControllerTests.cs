using Microsoft.AspNetCore.Mvc.Testing;

namespace MyAspNet.IntegrationTesting;

public class ControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webAppFactory;

    public ControllerTests(WebApplicationFactory<Program> webAppFactory)
    {
        _webAppFactory = webAppFactory;
    }

    [Fact]
    public async Task Test1()
    {
        // 获取DI容器
        //_webAppFactory.Services.GetRequiredService<IOptions<object>>();

        var client = _webAppFactory.CreateClient();
        string s = await (await client.GetAsync("/Add?i=1&j=2")).Content.ReadAsStringAsync();
    }
}

