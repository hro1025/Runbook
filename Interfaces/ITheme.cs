using Terminal.Gui;

namespace Runbook.Interfaces;

public interface ITheme
{
    ColorScheme Main();
    ColorScheme StatusBar();

    ColorScheme SideBar();
    ColorScheme NumberBar();

    ColorScheme EditBarEditing();
    ColorScheme EditBarSaved();
}
