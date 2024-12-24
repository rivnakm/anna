using System.Threading.Tasks;
using Anna.Api.Controllers;
using Anna.Index;
using FakeItEasy;

namespace Anna.Api.Test.Controllers;

public class PackageBaseAddressResourceControllerTest
{
    [Fact]
    public async Task TestGetPackageVersions()
    {
        var packageIndex = A.Fake<IPackageIndex>();
        var controller = new PackageBaseAddressResourceController(packageIndex);
        var resp = await controller.GetPackageVersions("foo");


    }
}
