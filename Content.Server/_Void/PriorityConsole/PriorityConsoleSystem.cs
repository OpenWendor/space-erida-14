using System.Linq;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Void.PriorityConsole;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Void.PriorityConsole;

public sealed partial class PriorityConsoleSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private StationJobsSystem _stationJobs = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;
    [Dependency] private ISharedPlayerManager _player = default!;

    private const float RefreshInterval = 2f;
    private float _refreshAccumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        SubscribeLocalEvent<PriorityConsoleComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<PriorityConsoleComponent, PriorityConsoleSetAutoMessage>(OnSetAuto);
        SubscribeLocalEvent<PriorityConsoleComponent, PriorityConsoleSetRequiredMessage>(OnSetRequired);
    }

    public override void Update(float frameTime)
    {
        _refreshAccumulator += frameTime;
        if (_refreshAccumulator < RefreshInterval)
            return;

        _refreshAccumulator = 0f;

        var refreshed = new HashSet<EntityUid>();
        var query = EntityQueryEnumerator<PriorityConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (!_ui.IsUiOpen(uid, PriorityConsoleUiKey.Key))
                continue;

            if (_station.GetOwningStation(uid) is not { } station)
                continue;

            if (!refreshed.Add(station) || !HasComp<StationJobsComponent>(station))
                continue;

            var priority = EnsureComp<StationJobPriorityComponent>(station);
            Recompute(station, priority);
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        if (!HasComp<StationJobsComponent>(ev.Station))
            return;

        var priority = EnsureComp<StationJobPriorityComponent>(ev.Station);
        Recompute(ev.Station, priority);
    }

    private void OnUiOpened(Entity<PriorityConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (!TryGetStation(ent, out var station, out var priority))
            return;

        Recompute(station, priority);
    }

    private void OnSetAuto(Entity<PriorityConsoleComponent> ent, ref PriorityConsoleSetAutoMessage args)
    {
        if (!TryGetStation(ent, out var station, out var priority))
            return;

        priority.AutoDepartments[args.Department] = args.Auto;
        Recompute(station, priority);
    }

    private void OnSetRequired(Entity<PriorityConsoleComponent> ent, ref PriorityConsoleSetRequiredMessage args)
    {
        if (!TryGetStation(ent, out var station, out var priority))
            return;

        var required = Math.Max(0, args.Required);
        var cap = GetSlotCap(station, args.Job);
        if (cap.HasValue)
            required = Math.Min(required, cap.Value);

        if (required == 0)
            priority.RequiredCounts.Remove(args.Job);
        else
            priority.RequiredCounts[args.Job] = required;

        Recompute(station, priority);
    }

    private bool TryGetStation(EntityUid console, out EntityUid station, out StationJobPriorityComponent priority)
    {
        priority = default!;
        station = default;

        var owning = _station.GetOwningStation(console);
        if (owning == null || !HasComp<StationJobsComponent>(owning))
            return false;

        station = owning.Value;
        priority = EnsureComp<StationJobPriorityComponent>(station);
        return true;
    }

    private void Recompute(EntityUid station, StationJobPriorityComponent priority)
    {
        var slots = _stationJobs.GetJobs(station);
        var current = CountCrewByJob(station);
        var highlighted = new HashSet<ProtoId<JobPrototype>>();
        var departments = new List<PriorityDepartmentInfo>();

        foreach (var department in _prototype.EnumeratePrototypes<DepartmentPrototype>())
        {
            if (department.EditorHidden)
                continue;

            var auto = priority.AutoDepartments.GetValueOrDefault(department.ID, false);

            var availableRoles = department.Roles
                .Where(slots.ContainsKey)
                .ToList();

            if (availableRoles.Count == 0)
                continue;

            var departmentStaffed = availableRoles.Any(role =>
                !priority.IgnoredJobs.Contains(role) && current.GetValueOrDefault(role) > 0);

            var jobs = new List<PriorityJobInfo>();

            foreach (var role in availableRoles)
            {
                var hasFreeSlot = slots[role] is null or > 0;
                var cap = GetSlotCap(station, role);
                var count = current.GetValueOrDefault(role);
                var required = priority.RequiredCounts.GetValueOrDefault(role);

                bool isHighlighted;
                if (auto)
                    isHighlighted = !departmentStaffed && hasFreeSlot;
                else
                    isHighlighted = required > 0 && count < required && hasFreeSlot;

                if (isHighlighted)
                    highlighted.Add(role);

                jobs.Add(new PriorityJobInfo(role, required, count, cap, isHighlighted));
            }

            departments.Add(new PriorityDepartmentInfo(department.ID, auto, jobs));
        }

        priority.Highlighted = highlighted;

        UpdateConsoles(station, departments);
        Broadcast();
    }

    private int? GetSlotCap(EntityUid station, ProtoId<JobPrototype> job)
    {
        if (!TryComp<StationJobsComponent>(station, out var jobs)
            || !jobs.SetupAvailableJobs.TryGetValue(job, out var setup))
            return null;

        var roundStart = setup[0];
        var midRound = setup.Length > 1 ? setup[1] : roundStart;

        if (roundStart < 0 || midRound < 0)
            return null;

        return Math.Max(roundStart, midRound);
    }

    private Dictionary<ProtoId<JobPrototype>, int> CountCrewByJob(EntityUid station)
    {
        var counts = new Dictionary<ProtoId<JobPrototype>, int>();

        if (!TryComp<StationJobsComponent>(station, out var jobs))
            return counts;

        foreach (var roles in jobs.PlayerJobs.Values)
        {
            foreach (var role in roles)
            {
                counts[role] = counts.GetValueOrDefault(role) + 1;
            }
        }

        return counts;
    }

    private void UpdateConsoles(EntityUid station, List<PriorityDepartmentInfo> departments)
    {
        var query = EntityQueryEnumerator<PriorityConsoleComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            if (_station.GetOwningStation(uid) != station)
                continue;

            _ui.SetUiState(uid, PriorityConsoleUiKey.Key, new PriorityConsoleBoundUserInterfaceState(departments));
        }
    }

    private void Broadcast()
    {
        var byStation = new Dictionary<NetEntity, HashSet<ProtoId<JobPrototype>>>();

        var query = EntityQueryEnumerator<StationJobPriorityComponent>();
        while (query.MoveNext(out var uid, out var priority))
        {
            byStation[GetNetEntity(uid)] = priority.Highlighted;
        }

        RaiseNetworkEvent(new PriorityHighlightUpdateEvent(byStation), Filter.Empty().AddPlayers(_player.Sessions));
    }
}
