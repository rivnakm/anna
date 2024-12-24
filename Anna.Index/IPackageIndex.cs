using Semver;

namespace Anna.Index;

public interface IPackageIndex
{
    IEnumerable<SemVersion> GetVersions(string lowerId);
}
