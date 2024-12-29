using System;
using System.Collections.Generic;

namespace Anna.IntegrationTests.Contexts;

public class PackageContext
{
    public Dictionary<Tuple<string, string>, string> AvailableLocalPackages { get; } = new();
    public List<string> VersionList { get; set; } = null;
}
