using Robust.Shared.Containers;
using Content.Shared.Slippery;
using Robust.Shared.Physics.Events;

namespace Content.Shared._Goobstation.Wizard.SlipOnCollide;

public sealed partial class  SlipOnCollideSystem : EntitySystem
{
    [Dependency] private SlipperySystem _slippery = default!;
    [Dependency] private SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlipOnCollideComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<SlipOnCollideComponent> ent, ref StartCollideEvent args)
    {
        var (uid, comp) = ent;

        if (_container.IsEntityInContainer(uid))
            return;

        if (!TryComp(uid, out SlipperyComponent? slippery))
            return;

        _slippery.TrySlip(uid, slippery, args.OtherEntity);
    }
}
