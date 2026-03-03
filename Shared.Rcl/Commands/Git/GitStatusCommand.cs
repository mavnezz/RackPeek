using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Shared.Rcl.Commands.Git;

public sealed class GitStatusCommand(IServiceProvider provider) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var configDir = provider.GetRequiredService<ConfigDirectoryProvider>();
        var runner = new GitProcessRunner(configDir.DirectoryPath);

        var result = await runner.RunAsync("status");

        if (result.Success)
            AnsiConsole.WriteLine(result.StandardOutput);
        else
            AnsiConsole.MarkupLine($"[red]git status failed:[/] {Markup.Escape(result.StandardError)}");

        return result.ExitCode;
    }
}
