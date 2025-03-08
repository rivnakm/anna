using Anna.Client;
using JetBrains.Annotations;

namespace Anna.IntegrationTests.Contexts;

[UsedImplicitly]
public class AnnaClientContext
{
    public IAnnaClient AnnaClient { get; set; } = null!;
}
