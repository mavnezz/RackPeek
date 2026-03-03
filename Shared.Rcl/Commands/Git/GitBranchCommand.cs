using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitBranchSettings : CommandSettings
{
    [CommandArgument(0, "[name]")]
    [Description("Name of the branch to create. Lists branches if omitted.")]
    public string? Name { get; init; }

    [CommandOption("-d|--delete")]
    [Description("Delete the specified branch.")]
    public bool Delete { get; init; }

    [CommandOption("-a|--all")]
    [Description("List both local and remote branches.")]
    public bool All { get; init; }
}

public sealed class GitBranchCommand(IServiceProvider provider) : AsyncCommand<GitBranchSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GitBranchSettings settings, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        string args;
        if (settings.Delete && !string.IsNullOrWhiteSpace(settings.Name))
            args = $"branch -d {settings.Name}";
        else if (!string.IsNullOrWhiteSpace(settings.Name))
            args = $"branch {settings.Name}";
        else if (settings.All)
            args = "branch -a";
        else
            args = "branch";

        var result = await runner.RunAsync(args, cancellationToken);

        if (result.Success)
        {
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git branch failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
