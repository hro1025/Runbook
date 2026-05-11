using Runbook.Models;

namespace Runbook.Interfaces;

// Defines the contract for executing a script and streaming its output
public interface IExecutor
{
    Task<bool> IsRuntimeAvailable(ScriptType type);
    public Task Execute(Script script, Action<string> onOutput);
}
