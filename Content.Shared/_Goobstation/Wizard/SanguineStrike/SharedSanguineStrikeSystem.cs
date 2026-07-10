using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Wizard.SanguineStrike;

public abstract partial class SharedSanguineStrikeSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;

    public void LifeSteal(EntityUid uid, FixedPoint2 amount, DamageableComponent? damageable = null)
    {
        if (!Resolve(uid, ref damageable, false))
            return;

        var totalUserDamage = _damageable.GetTotalDamage((uid, damageable));
        if (totalUserDamage <= FixedPoint2.Zero)
            return;

        DamageSpecifier toHeal;
        if (amount < totalUserDamage)
            toHeal = _damageable.GetAllDamage((uid, damageable)) * amount / totalUserDamage;
        else
            toHeal = _damageable.GetAllDamage((uid, damageable));

        _damageable.TryChangeDamage(uid, -toHeal, true, false);
    }
}
