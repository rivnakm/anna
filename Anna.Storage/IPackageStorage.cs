using Semver;

namespace Anna.Storage;

public interface IPackageStorage
{
    Stream GetPackage(string name, SemVersion version);
    void PutPackage(string name, SemVersion version, Stream data);
}
