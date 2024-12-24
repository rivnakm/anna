using Anna.Api.Attributes;
using FluentAssertions;

namespace Anna.Api.Test.Attributes;

public class ResourceAttributeTest
{
    [Fact]
    public void TestResourceType()
    {
        var attr = new ResourceAttribute("/path/", "Name", "1.0.0");

        attr.ResourceType.Should().Be("Name/1.0.0");
    }

    [Fact]
    public void TestPath()
    {
        var attr = new ResourceAttribute("/path/", "Name", "1.0.0");

        attr.Path.Should().Be("/path/");
    }
}
