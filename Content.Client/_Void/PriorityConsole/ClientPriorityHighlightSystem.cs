using Content.Shared._Void.PriorityConsole;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Client._Void.PriorityConsole;

public sealed class ClientPriorityHighlightSystem : EntitySystem
{
    private readonly Dictionary<NetEntity, HashSet<ProtoId<JobPrototype>>> _highlights = new();

    public event Action? HighlightsUpdated;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<PriorityHighlightUpdateEvent>(OnUpdate);
    }

    private void OnUpdate(PriorityHighlightUpdateEvent ev)
    {
        _highlights.Clear();
        foreach (var (station, set) in ev.HighlightedByStation)
            _highlights[station] = set;

        HighlightsUpdated?.Invoke();
    }

    public bool IsHighlighted(NetEntity station, ProtoId<JobPrototype> job)
    {
        return _highlights.TryGetValue(station, out var set) && set.Contains(job);
    }
}
