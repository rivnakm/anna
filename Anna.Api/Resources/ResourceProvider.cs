using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Anna.Api.Attributes;

namespace Anna.Api.Resources;

public class ResourceProvider : IResourceProvider
{
    private readonly List<Resource> _resources;

    public ResourceProvider()
    {
        var assembly = Assembly.Load("Anna.Api");
        Debug.Assert(assembly is not null);

        var resources = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<ResourceAttribute>() is not null)
            .Select(t => {
                var resAttr = t.GetCustomAttribute<ResourceAttribute>()!;

                return new Resource
                {
                    Id = resAttr.Path,
                    TypeName = resAttr.ResourceName,
                    TypeVersion = resAttr.ResourceVersion
                };
            });

        this._resources = resources.ToList();
    }

    public IEnumerable<Resource> GetResources()
    {
        return this._resources;
    }
}
