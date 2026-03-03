using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitCheckoutSettings : CommandSettings
{
    [CommandArgument(0, "<branch>")]
    [Description("Branch name to switch to.")]
    public string Branch { get; init; } = default!;

    [CommandOption("-b")]
    [Description("Create a new branch and switch to it.")]
    public bool CreateNew { get; init; }
}

public sealed class GitCheckoutCommand(IServiceProvider provider) : AsyncCommand<GitCheckoutSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitCheckoutSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var args = settings.CreateNew
            ? $"checkout -b {settings.Branch}"
            : $"checkout {settings.Branch}";

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
        {
            AnsiConsole.MarkupLine($"[green]Switched to branch '{Markup.Escape(settings.Branch)}'.[/]");
            if (!string.IsNullOrWhiteSpace(result.StandardError))
                AnsiConsole.WriteLine(result.StandardError);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git checkout failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
