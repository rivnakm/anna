using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Anna.Test.Common;

public class BoundPortHttpRequestWaitStrategy : IWaitUntil
{
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly int _port;

    public BoundPortHttpRequestWaitStrategy(int port, string endpoint)
    {
        this._httpClient = new HttpClient();
        this._port = port;
        this._endpoint = endpoint;
    }

    public async Task<bool> UntilAsync(IContainer container)
    {
        var port = container.GetMappedPublicPort(this._port);
        var req = new UriBuilder("http", container.Hostname, port, this._endpoint).Uri;
        try
        {
            var res = await this._httpClient.GetAsync(req);

            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
