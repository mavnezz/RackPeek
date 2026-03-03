using System.Diagnostics;

namespace Shared.Rcl.Commands.Git;

public sealed class GitProcessRunner(string workingDirectory)
{
    public async Task<GitResult> RunAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return new GitResult(-1, string.Empty,
                $"Failed to run git: {ex.Message}. Is git installed and on the PATH?");
        }

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new GitResult(process.ExitCode, stdout.TrimEnd(), stderr.TrimEnd());
    }
}

public readonly record struct GitResult(int ExitCode, string StandardOutput, string StandardError)
{
    public bool Success => ExitCode == 0;
}
