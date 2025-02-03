using System;
using System.Collections.Generic;

namespace Anna.IntegrationTests.Contexts;

public class PackageContext
{
    public Dictionary<Tuple<string, string>, string> AvailableLocalPackages { get; } = new();
    public HashSet<Tuple<string, string>> AvailableRemotePackages { get; } = [];
    public List<string>? VersionList { get; set; }
}
