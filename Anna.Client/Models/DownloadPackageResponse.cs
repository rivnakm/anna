using System.IO;

namespace Anna.Client.Models;

public class DownloadPackageResponse
{
    public Stream Content { get; }
    public string FileName { get; }
    
    internal DownloadPackageResponse(Stream content, string fileName)
    {
        this.Content = content;
        this.FileName = fileName;
    }
}
