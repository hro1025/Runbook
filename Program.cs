using Runbook.Core;
using Runbook.Interfaces;
using Runbook.Themes;
using Runbook.UI;
using Terminal.Gui;

namespace Runbook;

// Entry point of the application
static class Program
{
    static void Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // Build the path to the scripts folder (~/.scripts/runbook/)
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".scripts/runbook/";
        string fullPath = Path.Combine(homePath, folder);

        // Create the scripts folder if it doesn't exist
        Directory.CreateDirectory(fullPath);

        // Scan the scripts folder and load all scripts
        IScanner scanner = new Scanner();
        var scripts = scanner.Scan(fullPath);

        // Initialize the Terminal.Gui application
        Application.Init();

        // Set up the theme, dialogs, and executor
        ITheme theme = new CatppuccinMacchiato();
        var confirmDialog = new ConfirmationDialog(theme);
        IExecutor executor = new Executor();

        // Apply the theme to the global color schemes
        Colors.ColorSchemes["Dialog"] = theme.Main();
        Colors.ColorSchemes["Base"] = theme.Main();

        // Build the main dashboard with the loaded scripts
        var displayNames = scripts.ConvertAll(s => $"{s.Name}");
        var dashboard = new Dashboard(scripts, displayNames, theme, confirmDialog, executor);

        // Set up and register all keybinds
        var nameDialog = new NameDialog(theme);
        var keybindHandler = new KeybindHandler(
            dashboard,
            scripts,
            executor,
            confirmDialog,
            nameDialog,
            fullPath,
            scanner
        );
        keybindHandler.Register();

        // Start the UI and clean up on exit
        Application.Run(dashboard.Window);
        Application.Shutdown();
    }
}
