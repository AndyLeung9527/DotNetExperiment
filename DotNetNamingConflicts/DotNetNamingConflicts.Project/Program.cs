extern alias LibraryA;
extern alias LibraryB;

namespace DotNetNamingConflicts.Project;

internal class Program
{
    static void Main(string[] args)
    {
        string name = "n";
        LibraryA::DotNetNamingConflicts.StringExtensions.ToMyString(name);
        LibraryB::DotNetNamingConflicts.StringExtensions.ToMyString(name);
    }
}
