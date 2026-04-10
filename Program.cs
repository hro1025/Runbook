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

        var displayNames = scripts.ConvertAll(s => $"{s.Name, -30}{s.Type}");
        var dashboard = new Dashboard(displayNames, theme, confirmDialog);

        dashboard.ListView.OpenSelectedItem += (sender, e) =>
        {
            var selected = scripts[dashboard.ListView.SelectedItem];
            var confirmed = confirmDialog.Show("Run Script", $"Run {selected.Name}?");
            if (confirmed)
            {
                dashboard.TextView.Text = executor.Execute(selected);
            }
        };
        dashboard.ListView.SelectedItemChanged += (sender, e) =>
        {
            if (e.Item >= 0 && e.Item < scripts.Count)
            {
                var selected = scripts[e.Item];
                var lines = File.ReadAllLines(selected.Path!);

                var numbers = new string[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    numbers[i] = (i + 1).ToString().PadLeft(4);
                }

                dashboard.LineNumbers.Text = string.Join("\n", numbers);
                dashboard.TextView.Text = string.Join("\n", lines);
            }
        };
        Application.Run(dashboard.Window);
        Application.Shutdown();
    }
}
