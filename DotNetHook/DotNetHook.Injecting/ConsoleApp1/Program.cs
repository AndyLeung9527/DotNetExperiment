namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            File.WriteAllText("C:\\Users\\LWB\\Desktop\\a.txt", "heelll");
        }

        public static int DoPatching(string msg)
        {
            File.WriteAllText("C:\\Users\\LWB\\Desktop\\a.txt", "heelll");
            return 0;
        }
    }
}
