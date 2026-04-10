using System.Diagnostics;
using Runbook.Interfaces;
using Runbook.Models;

namespace Runbook.Core;

public class Executor : IExecutor
{
    public string Execute(Script script)
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
        process.StartInfo.UseShellExecute = false;

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }
}
