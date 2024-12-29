using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anna.Api.Attributes;
using Anna.Index;
using Anna.Index.Exceptions;
using Anna.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using NuGet.Versioning;

namespace Anna.Api.Controllers;

[Route("/api/v2/package")]
[Resource("/api/v2/package", "PackagePublish", "2.0.0")]
[ApiController]
public class PackagePublishResourceController : ResourceController
{
    private readonly IPackageIndex _packageIndex;
    private readonly IPackageStorage _packageStorage;

    public PackagePublishResourceController(IPackageIndex packageIndex, IPackageStorage packageStorage)
    {
        this._packageIndex = packageIndex;
        this._packageStorage = packageStorage;
    }

    // TODO: api key authentication/authorization (with middleware)
    // I'll need to add a way to figure out which api keys are valid, registration or something
    // Also use a swashbuckle filter to add auth status codes to methods with [Authorize] (this might already happen automatically, idk)
    [HttpPut]
    [Consumes(MimeTypes.Multipart.FormData)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutPackage()
    {
        // Create temp file to store package while reading metadata
        // Network streams may not be seekable so we wouldn't be able to read it multiple times
        var tempPath = Path.GetTempFileName();
        await using (var fileStream = System.IO.File.Open(tempPath, FileMode.Open))
        {
            // Read package from form data
            var formContent = await this.HttpContext.Request.ReadFormAsync();
            var packageFormFile = formContent.Files.FirstOrDefault();
            if (packageFormFile is null)
            {
                return new BadRequestResult();
            }

            // Copy package to temp file
            await packageFormFile.CopyToAsync(fileStream);
            fileStream.Position = 0;

            // use NuGet.Packaging to read the package name and version
            Stream nuspec = new MemoryStream();

            using var packageReader = new PackageArchiveReader(fileStream);
            var nuspecReader = packageReader.NuspecReader;
            var name = nuspecReader.GetId();
            var version = nuspecReader.GetVersion();

            // TODO: use request cancellation token?
            await (await packageReader.GetNuspecAsync(CancellationToken.None)).CopyToAsync(nuspec);

            fileStream.Position = 0;
            nuspec.Position = 0;

            // Add package to the index and copy to storage
            try
            {
                await this._packageIndex.AddPackage(name, version);
            }
            catch (PackageExistsException)
            {
                return new ConflictResult();
            }

            await this._packageStorage.PutPackage(name, version, fileStream);
            await this._packageStorage.PutPackageManifest(name, version, nuspec);
        }

        // Delete temp file
        System.IO.File.Delete(tempPath);

        return new AcceptedResult();
    }

    [Route("{id}/{version}")]
    [HttpDelete]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePackage(string id, string version)
    {
        // nuget.org interprets a DELETE as an "unlist" to avoid breaking existing programs but not showing unlisted versions in search results.
        // Implementers are allowed to interpret this as a hard delete, but I will be treating it as an unlist, with a custom header to override, if desired

        var nuGetVersion = NuGetVersion.Parse(version);
        try
        {
            if (HttpMethods.IsDelete(this.HttpContext.Request.Method))
            {
                if (this.HttpContext.Request.Headers.TryGetValue("x-anna-hard-delete", out var stringVal)
                        && bool.TryParse(stringVal, out var hardDelete)
                        && hardDelete)
                {
                    await this._packageIndex.RemovePackage(id, nuGetVersion);
                    this._packageStorage.DeletePackage(id, nuGetVersion);
                }
                else
                {
                    await this._packageIndex.UnlistPackage(id, nuGetVersion);
                }
            }
            else if (HttpMethods.IsPost(this.HttpContext.Request.Method))
            {
                await this._packageIndex.RelistPackage(id, nuGetVersion);
            }
        }
        catch (PackageNotFoundException)
        {
            return new NotFoundResult();
        }

        return new NoContentResult();
    }
}
