using Runbook.Interfaces;
using Terminal.Gui;

namespace Runbook.Themes;

public class CatppuccinMacchiato : ITheme
{
    private readonly Color _text = new(202, 211, 245); // #cad3f5
    private readonly Color _base = new(36, 39, 58); // #24273a
    private readonly Color _surface0 = new(54, 58, 79); // #363a4f
    private readonly Color _blue = new(138, 173, 244); // #8aadf4
    private readonly Color _overlay1 = new(110, 115, 141); // #6e738d
    private readonly Color _mantle = new(30, 32, 48); // #1e2030

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
