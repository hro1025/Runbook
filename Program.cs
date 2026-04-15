using Runbook.Core;
using Runbook.Interfaces;
using Runbook.Themes;
using Runbook.UI;
using Terminal.Gui;

namespace Runbook;

static class Program
{
    static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".scripts/runbook/";
        string fullPath = Path.Combine(homePath, folder);
        Directory.CreateDirectory(fullPath);

        IScanner scanner = new Scanner();
        var scripts = scanner.Scan(fullPath);

        Application.Init();
        ITheme theme = new CatppuccinMacchiato();
        var confirmDialog = new ConfirmationDialog(theme);
        IExecutor executor = new Executor();

        Colors.ColorSchemes["Dialog"] = theme.Main();
        Colors.ColorSchemes["Base"] = theme.Main();

        var displayNames = scripts.ConvertAll(s => $"{s.Name}");
        var dashboard = new Dashboard(scripts, displayNames, theme, confirmDialog, executor);

        Application.Run(dashboard.Window);
        Application.Shutdown();
    }
}
