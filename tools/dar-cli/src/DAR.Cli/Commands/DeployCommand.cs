using System.CommandLine;
using Spectre.Console;

namespace DAR.Cli.Commands;

/// <summary>
/// dar deploy &lt;sln&gt; &lt;config&gt;
/// Triggers an MSBuild Restore+Build (which runs AfterBuild deploy targets)
/// without needing the full `vs` bash alias.
/// </summary>
public static class DeployCommand
{
    public static Command Build()
    {
        var cmd = new Command("deploy", "Build and deploy a project (triggers AfterBuild targets)");

        var slnArg    = new Argument<string>("solution", "Path to the .sln file");
        var configArg = new Argument<string>("config",   "Build configuration (e.g. RVT2025, C3D2024, SAP2000_v26)");
        cmd.AddArgument(slnArg);
        cmd.AddArgument(configArg);

        cmd.SetHandler((string sln, string config) =>
        {
            var slnPath = Path.GetFullPath(sln);
            if (!File.Exists(slnPath))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Solution not found: {slnPath}");
                Environment.Exit(1);
            }

            // On WSL: use Windows MSBuild via the known path so AfterBuild deploy targets
            // (which write to Windows paths) actually work.
            var msbuild = "/mnt/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe";
            var useWinMsbuild = OperatingSystem.IsLinux() && File.Exists(msbuild);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[teal]Deploying[/] [white]{Path.GetFileName(slnPath)}[/] [{config}]");
            AnsiConsole.WriteLine();

            int exitCode;
            if (useWinMsbuild)
            {
                // Convert Linux path to Windows UNC path for MSBuild
                var slnWin = ConvertToWindowsPath(slnPath);
                exitCode = Run(msbuild, $"\"{slnWin}\" /t:Restore,Build /p:Configuration={config} /p:Platform=x64 /m /nologo /verbosity:minimal");
            }
            else
            {
                // Fall back to dotnet build (deploy targets skipped on Linux, but works on Windows)
                exitCode = Run("dotnet", $"build \"{slnPath}\" -c {config} /p:Platform=x64 --nologo");
            }

            AnsiConsole.WriteLine();
            if (exitCode == 0)
                AnsiConsole.MarkupLine("[teal]✓[/] Deploy succeeded.");
            else
            {
                AnsiConsole.MarkupLine("[red]✗[/] Deploy failed.");
                Environment.Exit(exitCode);
            }
        }, slnArg, configArg);

        return cmd;
    }

    private static string ConvertToWindowsPath(string linuxPath)
    {
        // /mnt/c/foo/bar → \\wsl.localhost\archlinux\... or C:\foo\bar
        // Try wslpath -w first
        try
        {
            var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName               = "wslpath",
                Arguments              = $"-w \"{linuxPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute        = false,
            })!;
            var result = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            if (!string.IsNullOrEmpty(result)) return result;
        }
        catch { }

        // Fallback: manual /mnt/c/ → C:\
        if (linuxPath.StartsWith("/mnt/") && linuxPath.Length > 6)
            return char.ToUpper(linuxPath[5]) + ":\\" + linuxPath[7..].Replace('/', '\\');

        return linuxPath;
    }

    private static int Run(string exe, string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(exe, args)
        {
            UseShellExecute = false,
        };
        using var proc = System.Diagnostics.Process.Start(psi)!;
        proc.WaitForExit();
        return proc.ExitCode;
    }
}
