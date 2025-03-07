using System;
using Anna.Index.Models;

namespace Anna.Index.Extensions;

public static class PackageExtensions
{
    public static bool MatchesSearch(this Package package, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }
        
        return package.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase);
    }
}
