// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Lightning;
using Content.Shared._Goobstation.Wizard.TeslaBlast;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed partial class TeslaBlastSystem : SharedTeslaBlastSystem
{
    [Dependency] private LightningSystem _lightning = default!;

    public override void ShootRandomLightnings(EntityUid performer,
        float power,
        float range,
        int boltCount,
        int arcDepth,
        string lightningPrototype,
        Vector2 minMaxDamage,
        Vector2 minMaxStunTime)
    {
        base.ShootRandomLightnings(performer,
            power,
            range,
            boltCount,
            arcDepth,
            lightningPrototype,
            minMaxDamage,
            minMaxStunTime);

        _lightning.ShootRandomLightnings(performer,
            range,
            boltCount,
            lightningPrototype,
            arcDepth,
            false);
    }

    public override void ShootLightning(EntityUid performer,
        EntityUid target,
        string lightningPrototype,
        float damage)
    {
        base.ShootLightning(performer, target, lightningPrototype, damage);

        _lightning.ShootLightning(performer, target, lightningPrototype, false);
    }
}