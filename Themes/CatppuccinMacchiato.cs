using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.Themes;

// Implements the Catppuccin Macchiato color theme
public class CatppuccinMacchiato : ITheme
{
    // Base palette colors from the Catppuccin Macchiato spec
    private readonly Color text = new(202, 211, 245); // #cad3f5
    private readonly Color main = new(36, 39, 58); // #24273a
    private readonly Color surface0 = new(54, 58, 79); // #363a4f
    private readonly Color blue = new(138, 173, 244); // #8aadf4

    // Main UI area: text on base, blue highlight on focus
    public ColorScheme Main()
    {
        var normal = new Terminal.Gui.Attribute(text, main);
        var focus = new Terminal.Gui.Attribute(blue, main);
        return new ColorScheme()
        {
            Normal = normal,
            Focus = focus,
            HotNormal = normal,
            HotFocus = focus,
        };
    }

    // Status bar: text on slightly elevated surface
    public ColorScheme StatusBar()
    {
        var attr = new Terminal.Gui.Attribute(text, surface0);
        return new ColorScheme()
        {
            Normal = attr,
            Focus = attr,
            HotNormal = attr,
            HotFocus = attr,
        };
    }

    // Sidebar script list: highlights selected item with surface0 background
    public ColorScheme SideBar()
    {
        var normal = new Terminal.Gui.Attribute(text, main);
        var focus = new Terminal.Gui.Attribute(text, surface0);
        return new ColorScheme()
        {
            Normal = normal,
            Focus = focus,
            HotNormal = normal,
            HotFocus = focus,
        };
    }

    // Edit bar while actively editing: red to indicate unsaved changes
    public ColorScheme EditBarEditing()
    {
        var normal = new Terminal.Gui.Attribute(Color.BrightRed, main);
        return new ColorScheme()
        {
            Normal = normal,
            Focus = normal,
            HotNormal = normal,
            HotFocus = normal,
        };
    }

    // Edit bar after saving: green to confirm changes were saved
    public ColorScheme EditBarSaved()
    {
        var normal = new Terminal.Gui.Attribute(Color.BrightGreen, main);
        return new ColorScheme()
        {
            Normal = normal,
            Focus = normal,
            HotNormal = normal,
            HotFocus = normal,
        };
    }
}
