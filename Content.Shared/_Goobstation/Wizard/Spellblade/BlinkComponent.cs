using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlinkComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float Distance = 3f;

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);

    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public float KnockdownRadius = 1f;

    [DataField, AutoNetworkedField]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(5);
}
