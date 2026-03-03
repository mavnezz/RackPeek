using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitInitCommand(IServiceProvider provider) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var result = await runner.RunAsync("init");

        if (result.Success)
        {
            AnsiConsole.MarkupLine($"[green]Git repository initialized in:[/] {Markup.Escape(configDir.DirectoryPath)}");
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git init failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
