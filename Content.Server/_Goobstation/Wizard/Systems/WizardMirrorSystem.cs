// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Guardian;
using Content.Server.Humanoid;
using Content.Server.Mind;
using Content.Server.Polymorph.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.MagicMirror;
using Content.Shared.Body;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Polymorph;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed partial class WizardMirrorSystem : SharedWizardMirrorSystem
{
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private HumanoidProfileSystem _humanoidProfile = default!;
    [Dependency] private SharedVisualBodySystem _visualBody = default!;
    [Dependency] private PolymorphSystem _polymorph = default!;
    [Dependency] private MetaDataSystem _meta = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<WizardMirrorComponent>(WizardMirrorUiKey.Key,
            subs =>
            {
                subs.Event<BoundUIClosedEvent>(OnUiClosed);
                subs.Event<WizardMirrorMessage>(OnMessage);
            });
    }

    private void OnMessage(Entity<WizardMirrorComponent> ent, ref WizardMirrorMessage args)
    {
        if (ent.Comp.Target == null)
            return;
        ForceLoadProfile(ent.Comp.Target.Value, ent.Comp, args.Profile);
    }

    private void OnUiClosed(Entity<WizardMirrorComponent> ent, ref BoundUIClosedEvent args)
    {
        ent.Comp.Target = null;
        Dirty(ent);
    }

    private void ForceLoadProfile(EntityUid target,
        WizardMirrorComponent component,
        HumanoidCharacterProfile profile)
    {
        if (component.AllowedSpecies.Contains(profile.Species) &&
            _proto.TryIndex(profile.Species, out var speciesProto))
        {
            if (HasComp<GuardianHostComponent>(target))
            {
                _popup.PopupEntity(Loc.GetString("wizard-mirror-guardian-change-species-fail"), target, target);
                return;
            }

            var config = new PolymorphConfiguration
            {
                Entity = speciesProto.Prototype,
                TransferName = true,
                TransferDamage = true,
                Forced = true,
                Inventory = PolymorphInventoryChange.Transfer,
                RevertOnCrit = false,
                RevertOnDeath = false,

            };
            var newUid = _polymorph.PolymorphEntity(target, config);
            if (newUid != null)
            {
                RemCompDeferred<PolymorphedEntityComponent>(newUid.Value);
                target = newUid.Value;
            }
        }

        _meta.SetEntityName(target, profile.Name);
        _humanoidProfile.ApplyProfileTo(target, profile);
        _visualBody.ApplyProfileTo(target, profile);

        if (_mind.TryGetMind(target, out var mind, out _) && TryComp(mind, out SoulBoundComponent? soulBound))
        {
            soulBound.Name = profile.Name;
            soulBound.Gender = profile.Gender;
            soulBound.Sex = profile.Sex;
            Dirty(mind, soulBound);
        }

            Dirty(target, (MetaDataComponent?)null!);
    }
}
