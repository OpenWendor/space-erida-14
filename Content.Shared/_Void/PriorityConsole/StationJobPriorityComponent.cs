using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._Void.PriorityConsole;

[RegisterComponent]
public sealed partial class StationJobPriorityComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, bool> AutoDepartments = new();

    [DataField]
    public Dictionary<ProtoId<JobPrototype>, int> RequiredCounts = new();

    [DataField]
    public HashSet<ProtoId<JobPrototype>> IgnoredJobs = new()
    {
        "TechnicalAssistant",
        "ResearchAssistant",
        "MedicalIntern",
        "SecurityCadet",
        "Visitor",
        "Passenger",
        "CargoTechnician",
        "SalvageSpecialist",
        "ServiceWorker",
        "Lawyer",
    };

    [ViewVariables]
    public HashSet<ProtoId<JobPrototype>> Highlighted = new();
}
