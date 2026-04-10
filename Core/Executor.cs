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
    }
}
