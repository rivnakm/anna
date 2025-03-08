using System.Collections.Generic;

namespace Anna.Common.Models.RegistrationIndex;

/// <summary>
///     The entry point for package metadata, shared by all packages on a source with the same package ID
/// </summary>
/// <remarks>
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#registration-index
///     https://learn.microsoft.com/en-us/nuget/api/registration-base-url-resource#response
/// </remarks>
public class RegistrationIndex
{
    /// <summary>
    ///     The number of registration pages in the index
    /// </summary>
    public required int Count { get; set; }

    /// <summary>
    ///     The array of registration pages
    /// </summary>
    public required List<RegistrationPage> Items { get; set; }
}
