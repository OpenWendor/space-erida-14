using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<string> VoidPriorityHighlightColor =
        CVarDef.Create("void.priority_highlight_color", "#3FB950FF", CVar.CLIENTONLY | CVar.ARCHIVE);
}
