using Semver;

namespace Anna.Index.Db.Models;

public class Version
{
    public int Id { get; set; }
    public SemVersion SemanticVersion { get; set; } = null!;
}
