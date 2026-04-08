using Terminal.Gui;

namespace Runbook.Core;

public class GuiColor
{
    public ColorScheme Colors()
    {
        var normal = new Terminal.Gui.Attribute(new Color(202, 211, 245), new Color(36, 39, 58));
        var focus = new Terminal.Gui.Attribute(new Color(138, 173, 244), new Color(36, 39, 58));

        return new ColorScheme()
        {
            Normal = normal,
            Focus = focus,
            HotNormal = normal,
            HotFocus = focus,
        };
    }
}
