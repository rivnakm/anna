using NuGet.Versioning;

namespace Anna.Index.Models;

public class CatalogEntry
{
    public required string PackageId { get; set; }
    public required NuGetVersion Version { get; set; }
}
