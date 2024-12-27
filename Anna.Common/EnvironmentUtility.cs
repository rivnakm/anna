namespace Anna.Common;

public static class EnvironmentUtility
{
    public static string GetRequiredEnvironmentVariable(string key)
    {
        return Environment.GetEnvironmentVariable(key)
            ?? throw new InvalidOperationException($"Environment variable '{key}' is not set");
    }
}
