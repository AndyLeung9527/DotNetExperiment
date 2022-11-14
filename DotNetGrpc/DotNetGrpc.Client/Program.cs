// See https://aka.ms/new-console-template for more information
using DotNetGrpc.Client;
using DotNetGrpc.Server;
using Microsoft.Extensions.DependencyInjection;

#region Normal
GrpcRequestTest grpcRequestTest = new GrpcRequestTest();
grpcRequestTest.CreateOrder();
#endregion

#region IOC
IServiceCollection services = new ServiceCollection();
services.AddTransient<GrpcRequestIOCTest>();
services.AddGrpcClient<Order.OrderClient>(options =>
{
    options.Address = new Uri("https://localhost:7242");
}).ConfigureChannel(grpcOptions =>
{

});

IServiceProvider serviceProvider = services.BuildServiceProvider();
var grpcRequestIOCTest = serviceProvider.GetRequiredService<GrpcRequestIOCTest>();
grpcRequestIOCTest.CreateOrder();
#endregion

Console.ReadLine();