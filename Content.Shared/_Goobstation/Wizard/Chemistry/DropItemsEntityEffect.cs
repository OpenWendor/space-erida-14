using Content.Shared.EntityEffects;
using Content.Shared.Standing;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Chemistry;

public sealed partial class DropItemsEntityEffect : EntityEffect
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        var ev = new DropHandItemsEvent();
        IoCManager.Resolve<IEntityManager>().EventBus.RaiseLocalEvent(target, ref ev);
    }

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-drop-items", ("chance", Probability));
    }
}
