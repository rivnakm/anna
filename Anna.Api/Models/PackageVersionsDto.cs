using System.Collections.Generic;

namespace Anna.Api.Models;

public class PackageVersionsDto
{
    public required List<string> Versions { get; init; }
}
