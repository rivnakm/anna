using System.IO;
using Anna.IntegrationTests.Contexts;
using Reqnroll;

namespace Anna.IntegrationTests.Hooks;

[Binding]
public class PackageCleanupHook
{
    [AfterScenario]
    public void Cleanup(PackageContext packageContext)
    {
        foreach (var pkgFile in packageContext.AvailableLocalPackages.Values)
        {
            File.Delete(pkgFile);
        }
    }
}
