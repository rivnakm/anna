using System;
using System.Collections.Generic;

namespace Anna.IntegrationTests.Contexts;

public class PackageContext
{
    public static Dictionary<Tuple<string, string>, string> AvailableLocalPackages { get; } = new();
    public static HashSet<Tuple<string, string>> AvailableRemotePackages { get; } = new();
    public List<string>? VersionList { get; set; }
}
