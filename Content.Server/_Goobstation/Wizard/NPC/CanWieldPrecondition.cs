// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Hands.Systems;
using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;

namespace Content.Server._Goobstation.Wizard.NPC;

public sealed partial class CanWieldPrecondition : HTNPrecondition
{
    [Dependency] private IEntityManager _entManager = default!;

    [DataField]
    public bool Invert;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!blackboard.TryGetValue<string>(NPCBlackboard.ActiveHand, out var activeHand, _entManager))
            return false ^ Invert;

        var handsSystem = _entManager.System<HandsSystem>();
        if (!handsSystem.TryGetHeldItem(owner, activeHand, out var item) ||
            !_entManager.TryGetComponent(item, out WieldableComponent? wieldable))
            return false ^ Invert;

        var wieldableSystem = _entManager.System<SharedWieldableSystem>();

        if (!wieldableSystem.CanWield((item.Value, wieldable), owner, true))
            return false ^ Invert;

        var beforeWieldEv = new WieldAttemptEvent();
        _entManager.EventBus.RaiseLocalEvent(item.Value, ref beforeWieldEv);

        return !beforeWieldEv.Cancelled ^ Invert;
    }
}