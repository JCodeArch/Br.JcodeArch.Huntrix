using HuntrX.Gameplay.Combat;

namespace HuntrX.Gameplay.Protection
{
    /// <summary>External opt-in protection sources must revalidate live eligibility on every damage request.</summary>
    public interface IDamageProtectionSource2D
    {
        // Pure eligibility query: implementations must not mutate registration collections here.
        bool Covers(DamageReceiver2D receiver);
    }
}
