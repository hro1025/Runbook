using Runbook.Core;
using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook;

static class Program
{
    static void Main(string[] args)
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        const string folder = ".scripts/runbook/";
        string fullPath = Path.Combine(homePath, folder);
        Directory.CreateDirectory(fullPath);

        IScanner scanner = new Scanner();
        var scripts = scanner.Scan(fullPath);

        Application.Init();

        var guiColor = new GuiColor();
        var displayNames = scripts.ConvertAll(s => $"{s.Name} - {s.Type}");
        var dashboard = new Dashboard(displayNames, guiColor.Colors());

        dashboard.ListView.OpenSelectedItem += (sender, e) =>
        {
            var selected = scripts[dashboard.ListView.SelectedItem];
            MessageBox.Query("Selected", $"Run: {selected.Name} with {selected.Type}?", "OK");
        };

        Application.Run(dashboard.Window);
        Application.Shutdown();
    }
}
