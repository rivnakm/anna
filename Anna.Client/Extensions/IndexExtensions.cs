using System;
using System.Linq;
using Index = Anna.Common.Models.Index;

namespace Anna.Client.Extensions;

public static class IndexExtensions
{
    public static Uri GetResourceUrl(this Index index, string resource)
    {
        return new Uri(index.Resources.Single(r => r.Type == resource).Id);
    }

    public static Uri GetResourceUrl(this Index index, string resourceName, string resourceVersion)
    {
        return index.GetResourceUrl($"{resourceName}/{resourceVersion}");
    }
}
