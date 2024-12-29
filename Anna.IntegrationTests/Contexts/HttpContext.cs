using System.Net.Http;

namespace Anna.IntegrationTests.Contexts;

public class HttpContext
{
    public HttpClient HttpClient { get; set; }

    public HttpRequestMessage Request { get; private set; } = new HttpRequestMessage();
    public HttpResponseMessage Response { get; set; } = new HttpResponseMessage();

    public void Reset()
    {
        this.Request = new HttpRequestMessage();
        this.Response = new HttpResponseMessage();
    }

    public void ResetRequest()
    {
        this.Request = new HttpRequestMessage();
    }
}
