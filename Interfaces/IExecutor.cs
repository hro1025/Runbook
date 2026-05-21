using Runbook.Models;

namespace Runbook.Interfaces;

// Defines the contract for executing a script and streaming its output
public interface IExecutor
{
    Task<bool> IsRuntimeAvailable(ScriptType type);
    bool IsRunning(Script script);
    void Kill(Script script);
    Task Execute(
        Script script,
        Action<string> onOutput,
        CancellationToken cancellationToken,
        bool reattach = false
    );
}
