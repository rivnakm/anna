using System.Collections.Generic;

namespace Anna.Api.Models;

public class GetPackageVersionsResponse
{
    public required List<string> Versions { get; init; }
}
