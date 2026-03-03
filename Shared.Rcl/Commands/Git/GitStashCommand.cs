using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitStashSettings : CommandSettings
{
    [CommandArgument(0, "[action]")]
    [Description("Stash action: pop, list, drop, apply. Defaults to stash (push).")]
    public string? Action { get; init; }
}

public sealed class GitStashCommand(IServiceProvider provider) : AsyncCommand<GitStashSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitStashSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var action = settings.Action?.ToLowerInvariant();
        var args = action switch
        {
            "pop" => "stash pop",
            "list" => "stash list",
            "drop" => "stash drop",
            "apply" => "stash apply",
            null or "" => "stash",
            _ => $"stash {action}"
        };

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
        {
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
            else
                AnsiConsole.MarkupLine("[green]Stash operation completed.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git stash failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
