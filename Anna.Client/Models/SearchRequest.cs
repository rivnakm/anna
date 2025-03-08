using System.Collections.Generic;

namespace Anna.Client.Models;

public class SearchRequest
{
    public string? Query { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public bool? Prerelease { get; set; }
    public string? SemVerLevel { get; set; }
    public string? PackageType { get; set; }

    public IEnumerable<KeyValuePair<string, string>> ToQueryParams()
    {
        if (this.Query is { } query)
        {
            yield return new KeyValuePair<string, string>("q", query);
        }

        if (this.Skip is { } skip)
        {
            yield return new KeyValuePair<string, string>("skip", skip.ToString());
        }

        if (this.Take is { } take)
        {
            yield return new KeyValuePair<string, string>("take", take.ToString());
        }

        if (this.Prerelease is { } prerelease)
        {
            yield return new KeyValuePair<string, string>("prerelease", prerelease.ToString());
        }

        if (this.SemVerLevel is { } semVerLevel)
        {
            yield return new KeyValuePair<string, string>("semVerLevel", semVerLevel);
        }

        if (this.PackageType is { } packageType)
        {
            yield return new KeyValuePair<string, string>("packageType", packageType);
        }
    }
}
