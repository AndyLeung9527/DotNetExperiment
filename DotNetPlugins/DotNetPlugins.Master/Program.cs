using DotNetPlugins.PluginBase;
using System.Reflection;

namespace DotNetPlugins.Master;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            string[] pluginPaths = { @"DotNetPlugins.Plugin1\bin\Debug\net8.0\DotNetPlugins.Plugin1.dll" };
            IEnumerable<ICommand> commands = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return CreateCommands(pluginAssembly);
            }).ToList();
            foreach (var command in commands)
            {
                Console.WriteLine($"{command.Name}\t - {command.Description}");
            }
        }
        catch { }
    }

    static Assembly LoadPlugin(string relativePath)
    {
        string root = Path.GetFullPath(
            Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(typeof(Program).Assembly.Location)))))!);

        string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        Console.WriteLine($"Loading commands from: {pluginLocation}");
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }

    static IEnumerable<ICommand> CreateCommands(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(ICommand).IsAssignableFrom(type))
            {
                ICommand? result = Activator.CreateInstance(type) as ICommand;
                if (result != null)
                {
                    count++;
                    yield return result;
                }
            }
        }
    }
}