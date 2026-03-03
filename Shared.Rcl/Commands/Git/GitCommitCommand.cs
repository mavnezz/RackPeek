using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitCommitSettings : CommandSettings
{
    [CommandOption("-m|--message <MESSAGE>")]
    [Description("Commit message.")]
    public string? Message { get; init; }
}

public sealed class GitCommitCommand(IServiceProvider provider) : AsyncCommand<GitCommitSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitCommitSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var addResult = await runner.RunAsync("add -A");
        if (!addResult.Success)
        {
            AnsiConsole.MarkupLine($"[red]git add failed:[/] {Markup.Escape(addResult.StandardError)}");
            return addResult.ExitCode;
        }

        var message = string.IsNullOrWhiteSpace(settings.Message)
            ? $"rpk: inventory update {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
            : settings.Message;

        var escapedMessage = message.Replace("\"", "\\\"");
        var commitResult = await runner.RunAsync($"commit -m \"{escapedMessage}\"");

        if (commitResult.Success)
        {
            AnsiConsole.MarkupLine("[green]Changes committed successfully.[/]");
            if (!string.IsNullOrWhiteSpace(commitResult.StandardOutput))
                AnsiConsole.WriteLine(commitResult.StandardOutput);
        }
        else if (commitResult.StandardOutput.Contains("nothing to commit"))
        {
            AnsiConsole.MarkupLine("[yellow]Nothing to commit, working tree clean.[/]");
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git commit failed:[/] {Markup.Escape(commitResult.StandardError)}");
        }

        return commitResult.ExitCode;
    }
}
