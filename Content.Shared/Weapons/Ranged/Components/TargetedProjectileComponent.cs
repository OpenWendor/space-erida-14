using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGunSystem), Other = AccessPermissions.ReadWriteExecute)]
public sealed partial class TargetedProjectileComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Target;
}
