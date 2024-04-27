using Google.Protobuf;
using Grpc.Core;

namespace DotNetGrpc.Server.Services
{
    public class DownloadFileService : DownloadFile.DownloadFileBase
    {
        private readonly ILogger<DownloadFileService> _logger;

        public DownloadFileService(ILogger<DownloadFileService> logger)
        {
            _logger = logger;
        }

        public override async Task ReadFile(ReadFileRequest request, IServerStreamWriter<ReadFileReply> responseStream, ServerCallContext context)
        {
            try
            {
                if (!File.Exists(request.FileFullName)) return;
                using var fileStream = File.OpenRead(request.FileFullName);
                var received = 0L;
                var totalLength = fileStream.Length;
                var bufferLength = 1024 * 1024;
                while (received < totalLength)
                {
                    var readLength = totalLength - received;
                    if (readLength > bufferLength) readLength = bufferLength;
                    var buffer = new byte[readLength];
                    var length = await fileStream.ReadAsync(buffer);
                    received += length;
                    var response = new ReadFileReply
                    {
                        TotalSize = totalLength,
                        Content = ByteString.CopyFrom(buffer)
                    };
                    await responseStream.WriteAsync(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取文件异常，文件{request.FileFullName}");
            }
        }
    }
}
