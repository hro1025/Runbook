using System.Diagnostics;
using System.Runtime.InteropServices;
using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

// Handles execution of scripts as external processes
public class Executor : IExecutor
{
    public async Task Execute(Script script, Action<string> onOutput)
    {
        // Determine the runner based on script type
        string execute = script.Type switch
        {
            ScriptType.Bash => "bash",
            ScriptType.CSharp => "dotnet-script",
            ScriptType.Python => "python",
            _ => throw new NotSupportedException($"Unknown script type: {script.Type}"),
        };
        // Check if the required runtime is installed
        if (script.Type == ScriptType.Python || script.Type == ScriptType.CSharp)
        {
            string checker = script.Type == ScriptType.Python ? "python3" : "dotnet-script";

            // Windows uses "python" instead of "python3"
            if (
                script.Type == ScriptType.Python
                && RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            )
            {
                checker = "python";
            }

            try
            {
                var check = new Process();
                check.StartInfo.FileName = checker;
                check.StartInfo.Arguments = "--version";
                check.StartInfo.RedirectStandardOutput = true;
                check.StartInfo.UseShellExecute = false;
                check.Start();
                await check.WaitForExitAsync();
            }
            catch
            {
                throw new Exception(
                    $"'{checker}' is not installed or not in PATH. Please install it to run this script."
                );
            }
        }

        // Set up the process with the correct runner and script path
        var process = new Process();
        process.StartInfo.FileName = execute;
        process.StartInfo.Arguments =
            script.Type == ScriptType.Python ? $"-u {script.Path}" : script.Path;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        // Stream stdout and stderr lines back via the onOutput callback
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                onOutput(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                onOutput(e.Data);
        };

        // Start the process and wait for it to finish
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
    }
}
