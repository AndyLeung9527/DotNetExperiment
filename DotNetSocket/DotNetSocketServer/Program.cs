using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotNetSocketServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

            using Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(100);

            var handler = await listener.AcceptAsync();

            // Receive message
            var buffer = new byte[1024];
            var receivedLength = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            Console.WriteLine($"接收：{receivedMessage}");

            // Response message
            var responseMessage = "Hello world, too";
            var responseMessageBytes = Encoding.UTF8.GetBytes(responseMessage);
            await handler.SendAsync(responseMessageBytes);
            Console.WriteLine($"响应：{responseMessage}");
        }
    }
}
