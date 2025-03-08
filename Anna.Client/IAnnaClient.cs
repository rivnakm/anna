using System.IO;
using System.Threading.Tasks;
using Anna.Client.Models;
using Anna.Common.Models.RegistrationIndex;
using Anna.Common.Models.SearchQueryService;
using Index = Anna.Common.Models.Index;

namespace Anna.Client;

public interface IAnnaClient
{
    // TODO: doc comments
    Task<bool> CheckHealth();
    Task<Index> GetIndex();

    Task<DownloadPackageResponse> DownloadPackage(string packageId, string packageVersion);
    Task<DownloadPackageResponse> DownloadPackageSpec(string packageId, string packageVersion);

    Task UploadPackage(Stream package);
    
    Task DeletePackage(string packageId,  string packageVersion);
    Task RestorePackage(string packageId,  string packageVersion);

    Task<RegistrationIndex> GetRegistrationIndex(string packageId);
    
    Task<SearchResponse> Search(SearchRequest request);
}
