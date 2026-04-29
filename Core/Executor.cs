using System.Diagnostics;
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
            _ => throw new NotSupportedException($"Unknown script type: {script.Type}"),
        };

        // Set up the process with the correct runner and script path
        var process = new Process();
        process.StartInfo.FileName = execute;
        process.StartInfo.Arguments = script.Path;
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
