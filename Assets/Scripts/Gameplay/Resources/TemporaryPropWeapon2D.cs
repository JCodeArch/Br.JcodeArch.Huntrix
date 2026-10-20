using System;
using UnityEngine;

namespace HuntrX.Gameplay.Resources
{
    public enum StagePropType2D
    {
        None = 0,
        AirlineCoffeePot = 1,
        VipGoldenMicStand = 2,
        MyeongdongBoilingKettle = 3
    }

    /// <summary>
    /// Capcom Final Fight / Mega Man style temporary equippable stage weapon system.
    /// Allows Rumi, Zoey, or Mira to pick up interactive stage props (like the iconic Airplane Coffee Pot)
    /// and use them for a limited number of empowered strikes before breaking.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TemporaryPropWeapon2D : MonoBehaviour
    {
        public StagePropType2D EquippedProp { get; private set; } = StagePropType2D.None;
        public int RemainingUses { get; private set; }
        public int MaxUses { get; private set; }
        public string EquippedDisplayName { get; private set; } = string.Empty;

        public event Action<StagePropType2D, string, int> PropEquipped;
        public event Action<StagePropType2D, int> PropUsed;
        public event Action<StagePropType2D> PropDepleted;

        public bool HasEquippedProp => EquippedProp != StagePropType2D.None && RemainingUses > 0;

        public void EquipProp(StagePropType2D propType, int uses = 5)
        {
            if (propType == StagePropType2D.None || uses <= 0)
            {
                ClearProp();
                return;
            }

            EquippedProp = propType;
            MaxUses = Mathf.Max(1, uses);
            RemainingUses = MaxUses;
            EquippedDisplayName = ResolveDisplayName(propType);
            PropEquipped?.Invoke(EquippedProp, EquippedDisplayName, RemainingUses);
        }

        public bool TryConsumeStrike(out StagePropType2D usedProp, out int remainingAfter)
        {
            if (!HasEquippedProp)
            {
                usedProp = StagePropType2D.None;
                remainingAfter = 0;
                return false;
            }

            usedProp = EquippedProp;
            RemainingUses = Mathf.Max(0, RemainingUses - 1);
            remainingAfter = RemainingUses;
            PropUsed?.Invoke(usedProp, remainingAfter);

            if (RemainingUses == 0)
            {
                StagePropType2D finished = EquippedProp;
                EquippedProp = StagePropType2D.None;
                EquippedDisplayName = string.Empty;
                PropDepleted?.Invoke(finished);
            }

            return true;
        }

        public void ClearProp()
        {
            EquippedProp = StagePropType2D.None;
            RemainingUses = 0;
            MaxUses = 0;
            EquippedDisplayName = string.Empty;
        }

        public static string ResolveDisplayName(StagePropType2D propType)
        {
            switch (propType)
            {
                case StagePropType2D.AirlineCoffeePot:
                    return "GARRAFA DE CAFE DO AVIAO";
                case StagePropType2D.VipGoldenMicStand:
                    return "PEDESTAL DE MICROFONE VIP";
                case StagePropType2D.MyeongdongBoilingKettle:
                    return "CHALEIRA QUENTE MYEONGDONG";
                default:
                    return string.Empty;
            }
        }
    }
}
