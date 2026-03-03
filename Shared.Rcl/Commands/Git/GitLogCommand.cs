using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitLogSettings : CommandSettings
{
    [CommandOption("-n|--count")]
    [Description("Number of commits to show.")]
    [DefaultValue(10)]
    public int Count { get; init; } = 10;
}

public sealed class GitLogCommand(IServiceProvider provider) : AsyncCommand<GitLogSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitLogSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var result = await runner.RunAsync($"log --oneline -n {settings.Count}");

        if (result.Success)
            AnsiConsole.WriteLine(result.StandardOutput);
        else
            AnsiConsole.MarkupLine($"[red]git log failed:[/] {Markup.Escape(result.StandardError)}");

        return result.ExitCode;
    }
}
