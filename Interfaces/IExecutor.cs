using Runbook.Models;

namespace Runbook.Interfaces;

public interface IExecutor
{
    public Task Execute(Script script, Action<string> onOutput);
}
