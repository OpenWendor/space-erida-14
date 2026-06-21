using Robust.Shared.Serialization;

namespace Content.Shared._Void.PriorityConsole;

[RegisterComponent]
public sealed partial class PriorityConsoleComponent : Component
{
}

[Serializable, NetSerializable]
public enum PriorityConsoleUiKey
{
    Key
}
