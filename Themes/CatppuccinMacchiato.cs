using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.Themes;

// Implements the Catppuccin Macchiato color theme
public class CatppuccinMacchiato : ITheme
{
    // Base palette colors from the Catppuccin Macchiato spec
    private readonly Color _text = new(202, 211, 245); // #cad3f5
    private readonly Color _base = new(36, 39, 58); // #24273a
    private readonly Color _surface0 = new(54, 58, 79); // #363a4f
    private readonly Color _blue = new(138, 173, 244); // #8aadf4
    private readonly Color _overlay1 = new(110, 115, 141); // #6e738d
    private readonly Color _mantle = new(30, 32, 48); // #1e2030

    // Main UI area: text on base, blue highlight on focus
    public ColorScheme Main()
    {
        var normal = new Terminal.Gui.Attribute(_text, _base);
        var focus = new Terminal.Gui.Attribute(_blue, _base);
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
        var attr = new Terminal.Gui.Attribute(_text, _surface0);
        return new ColorScheme()
        {
            Normal = attr,
            Focus = attr,
            HotNormal = attr,
            HotFocus = attr,
        };
    }

    // Line number bar: dimmed overlay text on dark mantle background
    public ColorScheme NumberBar()
    {
        var attr = new Terminal.Gui.Attribute(_overlay1, _mantle);
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
        var normal = new Terminal.Gui.Attribute(_text, _base);
        var focus = new Terminal.Gui.Attribute(_text, _surface0);
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
        var normal = new Terminal.Gui.Attribute(Color.BrightRed, _base);
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
        var normal = new Terminal.Gui.Attribute(Color.BrightGreen, _base);
        return new ColorScheme()
        {
            Normal = normal,
            Focus = normal,
            HotNormal = normal,
            HotFocus = normal,
        };
    }
}
