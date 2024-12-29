using Anna.IntegrationTests.Contexts;
using Reqnroll;

namespace Anna.IntegrationTests.Hooks;

[Binding]
public class HttpContextCleanupHook
{
    [AfterScenario]
    public void Cleanup(HttpContext httpContext)
    {
        httpContext.Reset();
    }
}
