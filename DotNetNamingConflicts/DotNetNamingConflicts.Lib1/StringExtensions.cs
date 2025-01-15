namespace DotNetNamingConflicts;

public static class StringExtensions
{
    public static string ToMyString(this string value)
    {
        return value + " (MyString)";
    }
}
