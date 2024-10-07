using DotNetPlugins.PluginBase;

namespace DotNetPlugins.Plugin1;

public class Plugin1Command : ICommand
{
    public string Name { get => "Plugin1"; }

    public string Description { get => "Display hello plugin1"; }

    public int Execute()
    {
        Console.WriteLine("Hello plugin1.");
        return 0;
    }
}

