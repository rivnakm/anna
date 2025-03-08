using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Anna.Common.Models.RegistrationIndex;

/// <summary>
///     A grouping of package versions. The number of package versions in a page is defined by server implementation.
/// </summary>
/// <remarks>
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-index
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-page-object
/// </remarks>
public class RegistrationPage
{
    /// <summary>
    ///     The URL to the registration page
    /// </summary>
    [JsonPropertyName("@id")]
    public required string Id { get; set; }

    /// <summary>
    ///     The number of registration leaves in the page
    /// </summary>
    public required int Count { get; set; }

    /// <summary>
    ///     The array of registration leave and their associate metadata
    /// </summary>
    public List<RegistrationLeaf>? Items { get; set; }

    /// <summary>
    ///     The lowest SemVer 2.0.0 version in the page (inclusive)
    /// </summary>
    public required string Lower { get; set; }

    /// <summary>
    ///     The highest SemVer 2.0.0 version in the page (inclusive)
    /// </summary>
    public required string Upper { get; set; }

    /// <summary>
    ///     The URL to the registration index
    /// </summary>
    public string? Parent { get; set; }
}
