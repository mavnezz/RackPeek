using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitResetSettings : CommandSettings
{
    [CommandArgument(0, "[target]")]
    [Description("Commit or file to reset to (e.g. HEAD~1, a file path).")]
    public string? Target { get; init; }

    [CommandOption("--hard")]
    [Description("Discard all changes (hard reset).")]
    public bool Hard { get; init; }

    [CommandOption("--soft")]
    [Description("Keep changes staged (soft reset).")]
    public bool Soft { get; init; }
}

public sealed class GitResetCommand(IServiceProvider provider) : AsyncCommand<GitResetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitResetSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var args = "reset";
        if (settings.Hard)
            args += " --hard";
        else if (settings.Soft)
            args += " --soft";

        if (!string.IsNullOrWhiteSpace(settings.Target))
            args += $" {settings.Target}";

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
        {
            AnsiConsole.MarkupLine("[green]Reset completed.[/]");
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git reset failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
