using Runbook.Models;

namespace Runbook.Interfaces;

public interface IExecutor
{
    public string Execute(Script script);
}
