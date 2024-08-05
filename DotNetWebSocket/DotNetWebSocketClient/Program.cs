using System.Net.WebSockets;
using System.Text;

namespace DotNetWebSocketClient
{
    internal class Program
    {
        private const string URI = "ws://localhost:8080/";

        static async Task Main(string[] args)
        {
            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(URI), CancellationToken.None);
            Console.WriteLine("WebSocket客户端已连接");

            string message = "Hello, WebSocket!";
            var buffer = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"发送消息：{message}");

            buffer = new byte[1024];
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine($"收到消息：{Encoding.UTF8.GetString(buffer, 0, result.Count)}");

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            Console.ReadLine();
        }
    }
}
