using Terminal.Gui;

namespace Runbook.Interfaces;

// Defines the contract for a theme, exposing color schemes for each UI component
public interface ITheme
{
    ColorScheme Main();
    ColorScheme StatusBar();
    ColorScheme SideBar();
    ColorScheme NumberBar();
    ColorScheme EditBarEditing();
    ColorScheme EditBarSaved();
}
