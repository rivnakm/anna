using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Anna.IntegrationTests.Contexts;
using FluentAssertions;
using Reqnroll;

namespace Anna.IntegrationTests.Steps;

[Binding]
public sealed class HttpSteps
{
    private readonly HttpContext _httpContext;

    public HttpSteps(HttpContext httpContext)
    {
        this._httpContext = httpContext;
    }

    [When(@"^I make a (\w+) request to (.*)$")]
    public async Task WhenIMakeARequestTo(string method, string endpoint)
    {
        var endpointUri = new Uri(endpoint, UriKind.Relative);
        var req = new HttpRequestMessage
        {
            Method = HttpMethod.Parse(method),
            RequestUri = endpointUri
        };

        var resp = await this._httpContext.HttpClient.SendAsync(req);

        this._httpContext.Response.StatusCode = resp.StatusCode;
        this._httpContext.Response.Headers = resp.Headers;
        this._httpContext.Response.Content = resp.Content;
    }

    [Then(@"^The response status code should be (\d{3})$")]
    public void TheResponseStatusCodeShouldBe(int statusCode)
    {
        this._httpContext.Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"^The response content type should be ([a-zA-Z0-9/.\-+]+)$")]
    public void TheResponseContentTypeShouldBe(string contentType)
    {
        this._httpContext.Response.Content.Headers.ContentType.MediaType.Should().BeEquivalentTo(contentType);
    }
}
