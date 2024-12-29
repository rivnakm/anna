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

    [Given(@"^I set the request header (.*) to (.*)$")]
    public void GivenISetTheRequestHeaderTo(string key, string value)
    {
        this._httpContext.Request.Headers.TryAddWithoutValidation(key, value);
    }

    [When(@"^I make a (\w+) request to (.*)$")]
    public async Task WhenIMakeARequestTo(string method, string endpoint)
    {
        var endpointUri = new Uri(endpoint, UriKind.Relative);

        var req = this._httpContext.Request;
        req.Method = HttpMethod.Parse(method);
        req.RequestUri = endpointUri;

        this._httpContext.Response = await this._httpContext.HttpClient.SendAsync(req);

        this._httpContext.ResetRequest();
    }

    [Then(@"^The response status code should be (\d{3})$")]
    public void ThenTheResponseStatusCodeShouldBe(int statusCode)
    {
        this._httpContext.Response.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"^The response content type should be ([a-zA-Z0-9/.\-+]+)$")]
    public void ThenTheResponseContentTypeShouldBe(string contentType)
    {
        this._httpContext.Response.Content.Headers.ContentType.MediaType.Should().BeEquivalentTo(contentType);
    }
}
