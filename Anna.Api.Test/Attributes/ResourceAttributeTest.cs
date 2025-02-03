using Anna.Api.Attributes;
using Shouldly;

namespace Anna.Api.Test.Attributes;

public class ResourceAttributeTest
{
    [Fact]
    public void TestResourceType()
    {
        var attr = new ResourceAttribute("/path/", "Name", "1.0.0");

        attr.ResourceType.ShouldBe("Name/1.0.0");
    }

    [Fact]
    public void TestPath()
    {
        var attr = new ResourceAttribute("/path/", "Name", "1.0.0");

        attr.Path.ShouldBe("/path/");
    }
}
