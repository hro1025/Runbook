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

        var window = new Window()
        {
            Title = "",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.None,
        };

        var guiColor = new GuiColor();
        window.ColorScheme = guiColor.Colors();

        var displayNames = scripts.ConvertAll(s => $"{s.Name} - {s.Type}");

        var listView = new ListView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        listView.SetSource(
            new System.Collections.ObjectModel.ObservableCollection<string>(displayNames)
        );

        var textView = new TextView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        listView.OpenSelectedItem += (sender, e) =>
        {
            var selected = scripts[listView.SelectedItem];
            MessageBox.Query("Selected", $"Run: {selected.Name} with {selected.Type}?", "OK");
        };

        var sidebar = new FrameView()
        {
            Title = "Scripts",
            X = 0,
            Y = 1,
            Width = Dim.Percent(20),
            Height = Dim.Fill(0),
        };

        var detail = new FrameView()
        {
            Title = "Preview",
            X = Pos.Right(sidebar),
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill(0),
        };

        sidebar.Add(listView);
        detail.Add(textView);
        window.Add(sidebar, detail);

        window.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == KeyCode.Esc)
            {
                Application.RequestStop();
                e.Handled = true;
            }
        };

        Application.Run(window);
        Application.Shutdown();
    }
}
