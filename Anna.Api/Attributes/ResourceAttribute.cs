using System;

namespace Anna.Api.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceAttribute : Attribute
{
    public string Path { get; }
    public string ResourceName { get; }
    public string ResourceVersion { get; }
    public string ResourceType => $"{this.ResourceName}/{this.ResourceVersion}";

    public ResourceAttribute(string path, string resourceName, string resourceVersion)
    {
        this.Path = path;
        this.ResourceName = resourceName;
        this.ResourceVersion = resourceVersion;
    }
}
