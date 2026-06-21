using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Void.PriorityConsole;

[Serializable, NetSerializable]
public sealed class PriorityConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<PriorityDepartmentInfo> Departments;

    public PriorityConsoleBoundUserInterfaceState(List<PriorityDepartmentInfo> departments)
    {
        Departments = departments;
    }
}

[Serializable, NetSerializable]
public sealed class PriorityDepartmentInfo
{
    public ProtoId<DepartmentPrototype> Department;
    public bool Auto;
    public List<PriorityJobInfo> Jobs;

    public PriorityDepartmentInfo(ProtoId<DepartmentPrototype> department, bool auto, List<PriorityJobInfo> jobs)
    {
        Department = department;
        Auto = auto;
        Jobs = jobs;
    }
}

[Serializable, NetSerializable]
public sealed class PriorityJobInfo
{
    public ProtoId<JobPrototype> Job;
    public int Required;
    public int Current;
    public int? Slots;
    public bool Highlighted;

    public PriorityJobInfo(ProtoId<JobPrototype> job, int required, int current, int? slots, bool highlighted)
    {
        Job = job;
        Required = required;
        Current = current;
        Slots = slots;
        Highlighted = highlighted;
    }
}

[Serializable, NetSerializable]
public sealed class PriorityConsoleSetAutoMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<DepartmentPrototype> Department;
    public readonly bool Auto;

    public PriorityConsoleSetAutoMessage(ProtoId<DepartmentPrototype> department, bool auto)
    {
        Department = department;
        Auto = auto;
    }
}

[Serializable, NetSerializable]
public sealed class PriorityConsoleSetRequiredMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<JobPrototype> Job;
    public readonly int Required;

    public PriorityConsoleSetRequiredMessage(ProtoId<JobPrototype> job, int required)
    {
        Job = job;
        Required = required;
    }
}
