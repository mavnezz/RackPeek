using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitDiffSettings : CommandSettings
{
    [CommandOption("--staged")]
    [Description("Show staged changes instead of working tree changes.")]
    public bool Staged { get; init; }
}

public sealed class GitDiffCommand(IServiceProvider provider) : AsyncCommand<GitDiffSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitDiffSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var args = settings.Staged ? "diff --staged" : "diff";

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
            AnsiConsole.WriteLine(result.StandardOutput);
        else
            AnsiConsole.MarkupLine($"[red]git diff failed:[/] {Markup.Escape(result.StandardError)}");

        return result.ExitCode;
    }
}
