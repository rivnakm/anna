using NuGet.Versioning;

namespace Anna.Index.Db.Models;

public class Version
{
    public int Id { get; set; }
    public NuGetVersion PackageVersion { get; set; } = null!;
    public bool Unlisted { get; set; }
}
