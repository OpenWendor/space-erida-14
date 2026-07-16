// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Chat.Systems;
using Content.Server.Humanoid;
using Content.Server.Popups;
using Content.Server.Toolshed.Commands.Misc;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Wizard.Mutate;
using Content.Shared.Chat;
using Content.Shared.Humanoid;
using Content.Shared.Sprite;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.Console.Commands;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed partial class HulkSystem : SharedHulkSystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PhysicsSystem _physics = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private GunSystem _gun = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HulkComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HulkComponent, ComponentRemove>(OnRemove);
    }

    private void OnRemove(Entity<HulkComponent> ent, ref ComponentRemove args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        Scale(ent, 0.8f);

        _popup.PopupEntity(Loc.GetString("hulk-unhulked"), uid, uid);

        if (!ent.Comp.LaserEyes)
            return;

        RemComp<GunComponent>(ent);
        RemComp<BasicHitscanAmmoProviderComponent>(ent);
    }

    private void OnInit(Entity<HulkComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;

        Scale(uid, 1.25f);

        if (!comp.LaserEyes)
            return;

        RemComp<GunComponent>(uid);
        var gun = AddComp<GunComponent>(uid);
        gun.FireRate = 1.5f;
        gun.UseKey = false;
        gun.ClumsyProof = true;
        gun.SoundGunshot = comp.SoundGunshot;
        _gun.RefreshModifiers((uid, gun));
        var hitscan = EntityManager.ComponentFactory.GetComponent<BasicHitscanAmmoProviderComponent>();
        hitscan.Proto = comp.ShotProto;
        AddComp(uid, hitscan, true);
    }

    public override void Roar(Entity<HulkComponent> hulk, float prob = 1f)
    {
        base.Roar(hulk, prob);

        var (uid, comp) = hulk;

        if (comp.NextRoar >= _timing.CurTime)
            return;

        if (prob < 1f && !_random.Prob(prob))
            return;

        comp.NextRoar = _timing.CurTime + comp.RoarDelay;

        var speech = _random.Pick(comp.Roars);

        _chat.TrySendInGameICMessage(uid, Loc.GetString(speech), InGameICChatType.Speak, false);
    }

    private void Scale(EntityUid uid, float scale)
    {
        EnsureComp<ScaleVisualsComponent>(uid);
        var ev = new ScaleEntityEvent();
        RaiseLocalEvent(uid, ref ev);

        var appearanceComponent = EnsureComp<AppearanceComponent>(uid);
        if (!_appearance.TryGetData<Vector2>(uid, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
            oldScale = TryComp(uid, out ScaleDataComponent? scaleData) ? scaleData.Scale : Vector2.One;

        _appearance.SetData(uid, ScaleVisuals.Scale, oldScale * scale, appearanceComponent);

        if (!TryComp(uid, out FixturesComponent? manager))
            return;

        foreach (var (id, fixture) in manager.Fixtures)
        {
            switch (fixture.Shape)
            {
                case PhysShapeCircle circle:
                    _physics.SetPositionRadius(uid,
                        id,
                        fixture,
                        circle,
                        circle.Position * scale,
                        circle.Radius * scale,
                        manager);
                    break;
                case PolygonShape poly:
                    var verts = poly.Vertices;

                    for (var i = 0; i < poly.VertexCount; i++)
                    {
                        verts[i] *= scale;
                    }

                    _physics.SetVertices(uid, id, fixture, poly, verts, manager);
                    break;
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HulkComponent>();
        while (query.MoveNext(out var ent, out var hulk))
        {
            if (hulk.Duration == null)
                continue;

            hulk.Duration = hulk.Duration.Value - frameTime;

            if (hulk.Duration >= 0)
                continue;

            RemCompDeferred<HulkComponent>(ent);
        }
    }
}
