using System.Linq;
using Anna.Api.Attributes;
using Anna.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Anna.Api.Conventions;

public class ResourceAttributeMetadataConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers.Where(c =>
                     c.Attributes.OfType<ResourceAttribute>().Any()))
        {
            var attr = controller.Attributes.OfType<ResourceAttribute>().Single();
            controller.Properties.Add(typeof(ResourceAttribute), attr);
        }
    }
}