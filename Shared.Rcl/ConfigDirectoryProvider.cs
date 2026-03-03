namespace Shared.Rcl;

public sealed class ConfigDirectoryProvider(string directoryPath)
{
    public string DirectoryPath { get; } = directoryPath;
}
