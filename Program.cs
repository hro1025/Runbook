using Runbook.Core;
using Runbook.Interfaces;
using Runbook.Themes;
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
        Colors.ColorSchemes["Dialog"] = theme.Main();
        Colors.ColorSchemes["Base"] = theme.Main();

        var displayNames = scripts.ConvertAll(s => $"{s.Name} - {s.Type}");
        var dashboard = new Dashboard(displayNames, theme);

        dashboard.ListView.OpenSelectedItem += (sender, e) =>
        {
            var selected = scripts[dashboard.ListView.SelectedItem];
            MessageBox.Query("Selected", $"Run: {selected.Name}?", " ", "No", "Yes");
        };
        dashboard.ListView.SelectedItemChanged += (sender, e) =>
        {
            if (e.Item >= 1 && e.Item < scripts.Count)
            {
                var selected = scripts[e.Item];
                var lines = File.ReadAllLines(selected.Path!);
                var numbers = new string[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    numbers[i] = $"{i + 1} {lines[i]}";
                }
                dashboard.TextView.Text = string.Join("\n", numbers);
            }

            Application.Run(dashboard.Window);
            Application.Shutdown();
        };
    }
}
