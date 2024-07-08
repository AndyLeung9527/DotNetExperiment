namespace AspNetProxy;

public sealed class ProxyService : IDisposable
{
    public HttpClient HttpClient { get; private set; }

    public ProxyService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    public void Dispose() => HttpClient.Dispose();
}

