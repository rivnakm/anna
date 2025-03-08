using System.Threading.Tasks;
using Anna.IntegrationTests.Contexts;
using Reqnroll;
using Shouldly;

namespace Anna.IntegrationTests.Steps;

[Binding]
public sealed class HealthCheckSteps
{
    private readonly AnnaClientContext _clientContext;
    private readonly HealthCheckContext _healthCheckContext;

    public HealthCheckSteps(AnnaClientContext clientContext, HealthCheckContext healthCheckContext)
    {
        this._clientContext = clientContext;
        this._healthCheckContext = healthCheckContext;
    }

    [When("I check the service health")]
    public async Task WhenICheckTheServiceHealth()
    {
        this._healthCheckContext.Healthy = await this._clientContext.AnnaClient.CheckHealth();
    }

    [Then("the service should be healthy")]
    public void ThenTheServiceHealthy()
    {
        this._healthCheckContext.Healthy.ShouldBeTrue();
    }
}
