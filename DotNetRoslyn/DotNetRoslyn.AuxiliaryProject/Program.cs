using System.Collections.Immutable;

namespace DotNetRoslyn.AuxiliaryProject;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var array = ImmutableArray.Create(1, 2, 3);

        var array2 = array.Add(4);

        var array3 = ImmutableArray<int>.Empty.Add(1);//分析警告行
    }
}

