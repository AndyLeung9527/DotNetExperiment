using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace DotNetWebSocketServer
{
    internal class Program
    {
        private const int PORT = 8080;

        static async Task Main(string[] args)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add($"http://127.0.0.1:{PORT}/");
            httpListener.Start();
            Console.WriteLine($"WebSocket服务器启动，监听端口：{8080}");

            while (true)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessRequest(context);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                }
            }
        }

        private static async void ProcessRequest(HttpListenerContext context)
        {
            WebSocketContext? wsContext;
            try
            {
                wsContext = await context.AcceptWebSocketAsync(null);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
                Console.WriteLine($"无法建立WebSocket链接：{ex}");
                return;
            }

            WebSocket webSocket = wsContext.WebSocket;
            try
            {
                byte[] buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine($"收到消息：{Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "内部服务器错误", CancellationToken.None);
                }
                Console.WriteLine($"WebSocket 连接异常:{ex}");
            }
        }
    }
}
