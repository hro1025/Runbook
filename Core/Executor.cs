using System.Diagnostics;
using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

// Handles execution of scripts as detached background processes
public class Executor : IExecutor
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".scripts",
        "runbook",
        "logs"
    );

    private static readonly string PidDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".scripts",
        "runbook",
        "pids"
    );

    public Executor()
    {
        Directory.CreateDirectory(LogDir);
        Directory.CreateDirectory(PidDir);
    }

    public async Task<bool> IsRuntimeAvailable(ScriptType type)
    {
        string checker = type switch
        {
            ScriptType.Python => "python3",
            ScriptType.CSharp => "dotnet-script",
            ScriptType.Bash => "bash",
            _ => "",
        };

        try
        {
            var check = new Process();
            check.StartInfo.FileName = checker;
            check.StartInfo.Arguments = "--version";
            check.StartInfo.RedirectStandardOutput = true;
            check.StartInfo.UseShellExecute = false;
            check.Start();
            await check.WaitForExitAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsRunning(Script script)
    {
        string pidFile = PidFile(script);
        if (!File.Exists(pidFile))
            return false;

        string pidText = File.ReadAllText(pidFile).Trim();
        if (!int.TryParse(pidText, out int pid))
            return false;

        try
        {
            var p = Process.GetProcessById(pid);
            return !p.HasExited;
        }
        catch
        {
            return false;
        }
    }

    public void Kill(Script script)
    {
        string pidFile = PidFile(script);
        if (!File.Exists(pidFile))
            return;

        string pidText = File.ReadAllText(pidFile).Trim();
        if (!int.TryParse(pidText, out int pid))
            return;

        try
        {
            var p = Process.GetProcessById(pid);
            p.Kill(entireProcessTree: true);
        }
        catch { }

        File.Delete(pidFile);
    }

    public async Task Execute(
        Script script,
        Action<string> onOutput,
        CancellationToken cancellationToken,
        bool reattach = false
    )
    {
        string runner = script.Type switch
        {
            ScriptType.Bash => "bash",
            ScriptType.CSharp => "dotnet-script",
            ScriptType.Python => "python3",
            _ => throw new NotSupportedException($"Unknown script type: {script.Type}"),
        };

        string logFile = LogFile(script);
        string pidFile = PidFile(script);

        if (!reattach)
        {
            // Fresh run — check runtime, clear log, launch process
            if (!await IsRuntimeAvailable(script.Type))
            {
                throw new Exception(
                    $"'{runner}' is not installed or not in PATH.\nPlease install it to run this script."
                );
            }

            File.WriteAllText(logFile, "");

            string args =
                script.Type == ScriptType.Python ? $"-u \"{script.Path}\"" : $"\"{script.Path}\"";

            string shellCmd = $"{runner} {args} >> \"{logFile}\" 2>&1 & echo $!";

            var launcher = new Process();
            launcher.StartInfo.FileName = "bash";
            launcher.StartInfo.Arguments = $"-c \"{shellCmd.Replace("\"", "\\\"")}\"";
            launcher.StartInfo.RedirectStandardOutput = true;
            launcher.StartInfo.UseShellExecute = false;
            launcher.Start();

            string pidText = (await launcher.StandardOutput.ReadToEndAsync()).Trim();
            await launcher.WaitForExitAsync();

            File.WriteAllText(pidFile, pidText);
        }

        // Tail the log file and stream output
        await TailLog(logFile, pidFile, onOutput, cancellationToken);
    }

    private async Task TailLog(
        string logFile,
        string pidFile,
        Action<string> onOutput,
        CancellationToken cancellationToken
    )
    {
        // Wait for log file to exist
        int wait = 0;
        while (!File.Exists(logFile) && wait < 50)
        {
            await Task.Delay(100, cancellationToken);
            wait++;
        }

        using var reader = new StreamReader(
            new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        );

        while (!cancellationToken.IsCancellationRequested)
        {
            string? line = await reader.ReadLineAsync();
            if (line != null)
            {
                onOutput(line);
            }
            else
            {
                if (!IsRunningByPidFile(pidFile))
                    break;

                await Task.Delay(100, cancellationToken);
            }
        }
    }

    private bool IsRunningByPidFile(string pidFile)
    {
        if (!File.Exists(pidFile))
            return false;
        string pidText = File.ReadAllText(pidFile).Trim();
        if (!int.TryParse(pidText, out int pid))
            return false;
        try
        {
            var p = Process.GetProcessById(pid);
            return !p.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private string LogFile(Script script) =>
        Path.Combine(LogDir, $"{Path.GetFileName(script.Path)}.log");

    private string PidFile(Script script) =>
        Path.Combine(PidDir, $"{Path.GetFileName(script.Path)}.pid");
}
