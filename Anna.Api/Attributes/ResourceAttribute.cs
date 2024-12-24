using System;

namespace Anna.Api.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceAttribute(string path, string resourceName, string resourceVersion) : Attribute
{
    public readonly string Path = path;
    public string ResourceType => $"{resourceName}/{resourceVersion}";
}
