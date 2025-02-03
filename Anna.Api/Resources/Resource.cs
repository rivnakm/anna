namespace Anna.Api.Resources;

public record Resource
{
    public required string Id { get; init; }
    public required string TypeName { get; init; }
    public required string TypeVersion { get; init; }
    public string? Comment { get; init; }
}
