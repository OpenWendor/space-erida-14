using Content.Shared._Void.PriorityConsole;
using Robust.Client.UserInterface;

namespace Content.Client._Void.PriorityConsole;

public sealed class PriorityConsoleBoundUserInterface : BoundUserInterface
{
    private PriorityConsoleMenu? _menu;

    public PriorityConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<PriorityConsoleMenu>();
        _menu.OnSetAuto += t => SendMessage(new PriorityConsoleSetAutoMessage(t.Item1, t.Item2));
        _menu.OnSetRequired += t => SendMessage(new PriorityConsoleSetRequiredMessage(t.Item1, t.Item2));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is PriorityConsoleBoundUserInterfaceState priorityState)
            _menu?.UpdateState(priorityState.Departments);
    }
}
