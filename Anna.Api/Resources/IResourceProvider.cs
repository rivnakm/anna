using System.Collections.Generic;

namespace Anna.Api.Resources;

public interface IResourceProvider
{
    IEnumerable<Resource> GetResources();
}
