namespace DotNetTemplate.Project.Demo;

internal class Program
{
    static async Task Main(string[] args)
    {
        await Console.Out.WriteAsync("Hello, DotNetTemplate.Project.Demo!");
        await Console.In.ReadLineAsync();
    }
}
