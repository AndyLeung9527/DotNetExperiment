namespace DotNetApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(GetNumber());
                Thread.Sleep(1000);
            }
        }

        private static string GetNumber() => "0";
    }
}
