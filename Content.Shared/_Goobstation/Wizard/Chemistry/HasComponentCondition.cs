using Content.Shared.EntityConditions;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.Chemistry;

public sealed partial class HasComponentCondition : EntityCondition
{
    [DataField(required: true)] public HashSet<string> Components = new();
    [DataField] public LocId? GuidebookComponentName;
    [DataField] public bool Invert;
    [DataField] public bool CheckMind;

    public override bool RaiseEvent(EntityUid target, IEntityConditionRaiser raiser)
    {
        EntityUid? mind = null;
        if (CheckMind)
        {
            var mindSystem = IoCManager.Resolve<IEntityManager>().System<SharedMindSystem>();
            if (mindSystem != null && mindSystem.TryGetMind(target, out var mindId, out _))
                mind = mindId;
        }

        var hasComp = false;
        var entMan = IoCManager.Resolve<IEntityManager>();
        foreach (var component in Components)
        {
            var comp = entMan.ComponentFactory.GetRegistration(component).Type;
            hasComp = entMan.HasComponent(target, comp) ||
                      (mind != null && entMan.HasComponent(mind.Value, comp));

            if (hasComp)
                break;
        }

        return hasComp ^ Invert;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        if (GuidebookComponentName == null)
            return string.Empty;

        return Loc.GetString("reagent-effect-condition-guidebook-has-component",
            ("comp", Loc.GetString(GuidebookComponentName)),
            ("invert", Invert));
    }
}
