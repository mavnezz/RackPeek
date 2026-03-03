using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitRestoreSettings : CommandSettings
{
    [CommandArgument(0, "[path]")]
    [Description("File or directory to restore. Restores all if omitted.")]
    public string? Path { get; init; }

    [CommandOption("--staged")]
    [Description("Unstage changes (keep working tree modifications).")]
    public bool Staged { get; init; }
}

public sealed class GitRestoreCommand(IServiceProvider provider) : AsyncCommand<GitRestoreSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitRestoreSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var args = "restore";
        if (settings.Staged)
            args += " --staged";
        args += " " + (settings.Path ?? ".");

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
        {
            AnsiConsole.MarkupLine("[green]Restore completed.[/]");
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git restore failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
