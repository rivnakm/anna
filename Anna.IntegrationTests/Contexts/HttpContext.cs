using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Anna.IntegrationTests.Contexts;

public class HttpContext
{
    public HttpClient HttpClient { get; set; }

    public HttpMessageContext Request { get; } = new HttpMessageContext();
    public HttpMessageContext Response { get; } = new HttpMessageContext();
}

public class HttpMessageContext
{
    public HttpStatusCode? StatusCode { get; set; } = null;
    public HttpHeaders Headers { get; set; }
    public HttpContent Content { get; set; }
}
