using Runbook.Interfaces;
using Runbook.Models;
using Runbook.UI;

namespace Runbook.Core;

public class KeyHandler
{
    public KeyHandler(List<Script> scripts, IExecutor executor, Dashboard dashboard)
    {
        scripts = scripts;
        executor = executor;
        dashboard = dashboard;
    }
}
