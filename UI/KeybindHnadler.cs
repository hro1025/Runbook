using Runbook.Interfaces;
using Runbook.Models;
using Terminal.Gui;

namespace Runbook.UI;

public class KeybindHandler(
    Dashboard dashboard,
    List<Script> scripts,
    IExecutor executor,
    ConfirmationDialog confirmDialog,
    ITheme theme
)
{
    private readonly Dashboard dashboard = dashboard;
    private readonly List<Script> scripts = scripts;
    private readonly IExecutor executor = executor;
    private readonly ConfirmationDialog confirmDialog = confirmDialog;
    private bool isEditing;

    private bool isDialogOpen;

    public void Register()
    {
        Application.KeyDown += async (sender, e) =>
        {
            if (e.KeyCode == KeyCode.Esc && !isDialogOpen)
            {
                if (isEditing)
                {
                    isEditing = false;
                    dashboard.EditBarEditing.Visible = false;
                    dashboard.TextView.ReadOnly = true;
                    dashboard.ListView.SetFocus();
                    e.Handled = true;
                }
                else
                {
                    isDialogOpen = true;
                    var confirmed = confirmDialog.Show("Quit", "Exit Runbook?");
                    isDialogOpen = false;
                    if (confirmed)
                        Application.RequestStop();
                    e.Handled = true;
                }
            }

            if (e.KeyCode == KeyCode.E && !isEditing && dashboard.ListView.HasFocus)
            {
                isDialogOpen = true;
                var confirmed = confirmDialog.Show("Edit", "Edit the script?");
                isDialogOpen = false;
                if (confirmed)
                {
                    isEditing = true;
                    dashboard.EditBarEditing.Visible = true;
                    dashboard.TextView.ReadOnly = false;
                    dashboard.TextView.SetFocus();
                }
                e.Handled = true;
            }

            if (e.KeyCode == KeyCode.S && !isEditing && dashboard.ListView.HasFocus)
            {
                var selected = scripts[dashboard.ListView.SelectedItem];
                isDialogOpen = true;
                var settings = new ScriptSettingsWindow(selected, theme);
                settings.Show();
                isDialogOpen = false;
                e.Handled = true;
            }
            if (e.KeyCode == (KeyCode.S | KeyCode.CtrlMask) && isEditing)
            {
                var selected = scripts[dashboard.ListView.SelectedItem];
                File.WriteAllText(selected.Path!, dashboard.TextView.Text);
                dashboard.EditBarSaved.Visible = true;
                Application.AddTimeout(
                    TimeSpan.FromSeconds(1),
                    () =>
                    {
                        dashboard.EditBarSaved.Visible = false;
                        return false;
                    }
                );
                e.Handled = true;
            }
        };
    }
}
