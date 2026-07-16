using System;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Wizard.Mutate;

[DataDefinition]
[Serializable, NetSerializable]
public readonly partial record struct CustomBaseLayerInfo
{
    public CustomBaseLayerInfo(string? id, Color? color = null, string? shader = null)
    {
        Id = id;
        Color = color;
        Shader = shader;
    }

    [DataField] public string? Id { get; init; }
    [DataField] public Color? Color { get; init; }
    [DataField] public string? Shader { get; init; }
}
