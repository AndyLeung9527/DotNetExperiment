using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DotNetSocketClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);

            // Send message
            using Socket client = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(ipEndPoint);
            var sendMessage = "Hello world!";
            var sendMessageBytes = Encoding.UTF8.GetBytes(sendMessage);
            await client.SendAsync(sendMessageBytes);
            Console.WriteLine($"发送：{sendMessage}");

            // Receive message
            var buffer = new byte[1024];
            var receivedLength = await client.ReceiveAsync(buffer, SocketFlags.None);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            Console.WriteLine($"接收：{receivedMessage}");

            client.Shutdown(SocketShutdown.Both);
        }
    }
}
