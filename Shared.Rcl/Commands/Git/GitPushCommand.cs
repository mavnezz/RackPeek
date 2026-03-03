using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitPushCommand(IServiceProvider provider) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var result = await runner.RunAsync("push");

        if (result.Success)
        {
            AnsiConsole.MarkupLine("[green]Push completed successfully.[/]");
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                AnsiConsole.WriteLine(result.StandardOutput);
            if (!string.IsNullOrWhiteSpace(result.StandardError))
                AnsiConsole.WriteLine(result.StandardError);
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]git push failed:[/] {Markup.Escape(result.StandardError)}");
        }

        return result.ExitCode;
    }
}
