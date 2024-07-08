namespace AspNetProxy;

public class ProxyMiddleware
{
    private readonly string _prefix = "/proxy";
    private readonly string _newHost = "http://127.0.0.1:5000";
    private readonly RequestDelegate _next;

    public ProxyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ProxyService proxyService)
    {
        if (context.Request.Path.StartsWithSegments(_prefix))
        {
            var newUri = context.Request.Path.Value?.Remove(0, _prefix.Length) + context.Request.QueryString;
            var targetUri = new Uri(_newHost + newUri);
            using var requestMessage = GenerateProxifiedRequest(context, targetUri);
            using var responseMessage = await proxyService.HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            context.Response.StatusCode = (int)responseMessage.StatusCode;
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
            if (!context.Response.Headers.ContainsKey("Cache-Control"))
            {
                context.Response.Headers.TryAdd("Cache-Control", "no-cache, no-store");
            }
            await responseMessage.Content.CopyToAsync(context.Response.Body);
        }

        await _next(context);
    }

    private static HttpRequestMessage GenerateProxifiedRequest(HttpContext context, Uri targetUri)
    {
        var requestMessage = new HttpRequestMessage();

        if (!(HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) || HttpMethods.IsDelete(context.Request.Method) || HttpMethods.IsTrace(context.Request.Method)))
        {
            requestMessage.Content = new StreamContent(context.Request.Body);
        }

        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Connection") || header.Key.Equals("Host")) continue;

            if (header.Key.Equals("User-Agent"))
            {
                string userAgent = header.Value.Any() ? $"{header.Value.First()} {context.TraceIdentifier}" : string.Empty;
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, userAgent)) requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, userAgent);
            }
            else
            {
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray())) requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        requestMessage.RequestUri = targetUri;
        requestMessage.Headers.Host = targetUri.Host;

        if (HttpMethods.IsDelete(context.Request.Method)) requestMessage.Method = HttpMethod.Delete;
        else if (HttpMethods.IsGet(context.Request.Method)) requestMessage.Method = HttpMethod.Get;
        else if (HttpMethods.IsHead(context.Request.Method)) requestMessage.Method = HttpMethod.Head;
        else if (HttpMethods.IsOptions(context.Request.Method)) requestMessage.Method = HttpMethod.Options;
        else if (HttpMethods.IsPost(context.Request.Method)) requestMessage.Method = HttpMethod.Post;
        else if (HttpMethods.IsPut(context.Request.Method)) requestMessage.Method = HttpMethod.Put;
        else if (HttpMethods.IsTrace(context.Request.Method)) requestMessage.Method = HttpMethod.Trace;
        else if (HttpMethods.IsPatch(context.Request.Method)) requestMessage.Method = HttpMethod.Patch;
        else if (HttpMethods.IsConnect(context.Request.Method)) requestMessage.Method = HttpMethod.Connect;
        else requestMessage.Method = new HttpMethod(context.Request.Method);

        return requestMessage;
    }
}
