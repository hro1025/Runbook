using Runbook.Core;
using Runbook.Interfaces;
using Runbook.Models;
using Runbook.Themes;
using Runbook.UI;
using Terminal.Gui;

namespace Runbook;

// Entry point of the application
static class Program
{
    static async Task Main()
    {
        var executor = new Executor();

        // Build the path to the scripts folder (~/.scripts/runbook/)
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".scripts/runbook/";
        string fullPath = Path.Combine(homePath, folder);

        // Create the scripts folder if it doesn't exist
        Directory.CreateDirectory(fullPath);

        // Seed a defauldt test script on first launch if the folder is empty
        if (
            await executor.IsRuntimeAvailable(ScriptType.Bash)
            && Directory.GetFiles(fullPath).Length == 0
        )
        {
            if (await executor.IsRuntimeAvailable(ScriptType.Bash))
            {
                var testScript = Path.Combine(fullPath, "test.sh");
                File.WriteAllText(
                    testScript,
                    "#!/bin/bash\necho \"=== Runbook Test Script ===\"\necho \"Runtime: Bash\"\necho \"User: $(whoami)\"\necho \"Host: $(hostname)\"\necho \"OS: $(uname -o)\"\necho \"Uptime: $(uptime -p)\"\necho \"=== Runbook is working correctly ===\""
                );
                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    File.SetUnixFileMode(
                        testScript,
                        UnixFileMode.UserRead
                            | UnixFileMode.UserWrite
                            | UnixFileMode.UserExecute
                            | UnixFileMode.GroupRead
                            | UnixFileMode.GroupExecute
                            | UnixFileMode.OtherRead
                            | UnixFileMode.OtherExecute
                    );
                }
            }

            if (await executor.IsRuntimeAvailable(ScriptType.Python))
            {
                var testScript = Path.Combine(fullPath, "test.py");
                File.WriteAllText(
                    testScript,
                    "import platform, getpass, socket\nprint(\"=== Runbook Test Script ===\")\nprint(f\"Runtime: Python {platform.python_version()}\")\nprint(f\"User: {getpass.getuser()}\")\nprint(f\"Host: {socket.gethostname()}\")\nprint(f\"OS: {platform.system()} {platform.release()}\")\nprint(f\"Machine: {platform.machine()}\")\nprint(\"=== Runbook is working correctly ===\")"
                );
            }

            if (await executor.IsRuntimeAvailable(ScriptType.CSharp))
            {
                var testScript = Path.Combine(fullPath, "test.csx");
                File.WriteAllText(
                    testScript,
                    "using System.Runtime.InteropServices;\nConsole.WriteLine(\"=== Runbook Test Script ===\");\nConsole.WriteLine($\"Runtime: dotnet-script / .NET {Environment.Version}\");\nConsole.WriteLine($\"User: {Environment.UserName}\");\nConsole.WriteLine($\"Host: {Environment.MachineName}\");\nConsole.WriteLine($\"OS: {RuntimeInformation.OSDescription}\");\nConsole.WriteLine($\"Architecture: {RuntimeInformation.OSArchitecture}\");\nConsole.WriteLine(\"=== Runbook is working correctly ===\");"
                );
            }
        }
        // Scan the scripts folder and load all scripts
        IScanner scanner = new Scanner();
        var scripts = scanner.Scan(fullPath);

        // Initialize the Terminal.Gui application
        Application.Init();

        // Set up the theme, dialogs, and executor
        ITheme theme = new CatppuccinMacchiato();
        var confirmDialog = new ConfirmationDialog(theme);
        var messageDialog = new MessageDialog(theme);

        // Apply the theme to the global color schemes
        Colors.ColorSchemes["Dialog"] = theme.Main();
        Colors.ColorSchemes["Main"] = theme.Main();

        // Build the main dashboard with the loaded scripts
        var displayNames = scripts.ConvertAll(s => $"{s.Name}");
        var dashboard = new Dashboard(
            scripts,
            displayNames,
            theme,
            confirmDialog,
            executor,
            messageDialog
        );

        // Set up and register all keybinds
        var nameDialog = new NameDialog(theme);
        var keybindHandler = new KeybindHandler(
            dashboard,
            scripts,
            executor,
            confirmDialog,
            nameDialog,
            fullPath,
            scanner,
            messageDialog
        );
        keybindHandler.Register();

        // Start the UI and clean up on exit
        Application.Run(dashboard.Window);
        Application.Shutdown();
    }
}
