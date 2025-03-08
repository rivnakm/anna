using System.Text.Json.Serialization;

namespace Anna.Common.Models.RegistrationIndex;

/// <summary>
///     A document specific to a single package version.
/// </summary>
/// <remarks>
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-index
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-leaf-object-in-a-page
/// </remarks>
public class RegistrationLeaf
{
    /// <summary>
    ///     The URL to the registration leaf
    /// </summary>
    [JsonPropertyName("@id")]
    public required string Id { get; set; }

    /// <summary>
    ///     The catalog entry containing the package metadata
    /// </summary>
    public required CatalogEntry CatalogEntry { get; set; }

    /// <summary>
    ///     The URL to the package content (.nupkg)
    /// </summary>
    public required string PackageContent { get; set; }
}
