using System.Diagnostics;
using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

public class Executor : IExecutor
{
    public async Task Execute(Script script, Action<string> onOutput)
    {
        string execute = script.Type switch
        {
            ScriptType.Bash => "bash",
            ScriptType.CSharp => "dotnet-script",
            _ => throw new NotSupportedException($"Unknown script type: {script.Type}"),
        };

        var process = new Process();
        process.StartInfo.FileName = execute;
        process.StartInfo.Arguments = script.Path;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

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

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
    }
}
