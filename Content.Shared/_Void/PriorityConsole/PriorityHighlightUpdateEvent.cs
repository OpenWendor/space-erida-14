using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Void.PriorityConsole;

[Serializable, NetSerializable]
public sealed class PriorityHighlightUpdateEvent : EntityEventArgs
{
    public Dictionary<NetEntity, HashSet<ProtoId<JobPrototype>>> HighlightedByStation;

    public PriorityHighlightUpdateEvent(Dictionary<NetEntity, HashSet<ProtoId<JobPrototype>>> highlightedByStation)
    {
        HighlightedByStation = highlightedByStation;
    }
}
