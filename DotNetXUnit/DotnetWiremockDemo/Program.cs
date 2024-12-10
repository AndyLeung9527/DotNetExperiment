using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace DotnetWiremockDemo;

internal class Program
{
    static void Main(string[] args)
    {
        using var server = WireMockServer.Start();
        server.Given(Request.Create().WithPath("/add").UsingGet().WithParam("i", "1").WithParam("j", "2"))
            .RespondWith(Response.Create().WithBody("3").WithStatusCode(200));
        server.Given(Request.Create().WithPath("/add").UsingGet().WithParam("i", "100").WithParam("j", "1"))
            .RespondWith(Response.Create().WithBody("3").WithStatusCode(200));
        server.Given(Request.Create().WithPath("/add").UsingGet().WithParam("i", "-1"))
            .RespondWith(Response.Create().WithStatusCode(400).WithBody("negative is not allowed"));

        // Assert某个请求被调用
        //server.LogEntries.Should().Contain(e => e.RequestMessage.Path == "/add" && e.RequestMessage.Method == "GET"
        //                                    && 200.Equals(e.ResponseMessage.StatusCode));

        Console.WriteLine(server.Url);//Mock服务器的地址
        Console.ReadLine();
    }
}
