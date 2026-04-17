using Runbook.Models;

namespace Runbook.Interfaces;

public interface IExecutor
{
    public Task<string> Execute(Script script);
}
