using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitRemoteSettings : CommandSettings
{
    [CommandOption("--url <URL>")]
    [Description("The remote repository URL to set as origin.")]
    public string Url { get; init; } = default!;
}

public sealed class GitRemoteCommand(IServiceProvider provider) : AsyncCommand<GitRemoteSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitRemoteSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            AnsiConsole.MarkupLine("[red]Please provide --url <URL>.[/]");
            return 1;
        }

        var result = await runner.RunAsync($"remote set-url origin {settings.Url}");

        if (!result.Success)
            result = await runner.RunAsync($"remote add origin {settings.Url}");

        if (result.Success)
            AnsiConsole.MarkupLine($"[green]Remote origin set to:[/] {Markup.Escape(settings.Url)}");
        else
            AnsiConsole.MarkupLine($"[red]git remote failed:[/] {Markup.Escape(result.StandardError)}");

        return result.ExitCode;
    }
}
