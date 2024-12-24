using System.Collections.Generic;
using System.Reflection;
using Anna.Api.Attributes;
using Anna.Api.Controllers;
using Anna.Api.Conventions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Anna.Api.Test.Conventions;

public class ResourceAttributeMetadataConventionTest
{
    [Fact]
    public void TestApply()
    {
        var convention = new ResourceAttributeMetadataConvention();

        var applicationModel = new ApplicationModel();
        var resourceAttr = new ResourceAttribute("/path/", "Name", "1.0.0");
        applicationModel.Controllers.Add(
                new ControllerModel(
                    typeof(PackageBaseAddressResourceController).GetTypeInfo(),
                    new List<object> { resourceAttr }.AsReadOnly()));

        convention.Apply(applicationModel);

        applicationModel.Controllers.Should().HaveCount(1);

        var controller = applicationModel.Controllers[0];
        controller.Properties.Should().ContainKey(typeof(ResourceAttribute));
        controller.Properties.Should().Contain(typeof(ResourceAttribute), resourceAttr);
    }
}
