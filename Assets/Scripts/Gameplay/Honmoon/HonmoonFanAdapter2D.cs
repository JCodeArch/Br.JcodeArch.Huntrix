using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.Honmoon
{
    /// <summary>Resource effects occur after accepted fan transitions, not overlap alone.</summary>
    [DisallowMultipleComponent]
    public sealed class HonmoonFanAdapter2D : MonoBehaviour
    {
        [SerializeField] private HonmoonController2D honmoon;
        [SerializeField] private FanRescueController2D rescue;
        [SerializeField] private FanRescueController2D[] additionalRescues = new FanRescueController2D[0];
        [SerializeField] private FanSoulDrainController2D drain;
        [SerializeField] private FanDrainEncounterBootstrap2D drainEncounter;
        [SerializeField] private float rescuedFanGain = 10f;
        [SerializeField] private float drainLossPerSoul = .25f;
        private bool subscribed;
        private bool Valid => honmoon != null && additionalRescues != null && additionalRescues.Length <= 64 && Finite(rescuedFanGain) && rescuedFanGain >= 0f &&
            Finite(drainLossPerSoul) && drainLossPerSoul >= 0f;
        private void OnEnable()
        {
            if (!Valid || subscribed) return;
            if (rescue != null) rescue.RescueCompleted += HandleRescued;
            for (int i = 0; i < additionalRescues.Length; i++)
                if (UniqueRescue(i)) additionalRescues[i].RescueCompleted += HandleRescued;
            if (drain != null) drain.FanDrainAccepted += HandleDrained;
            if (drainEncounter != null)
            {
                drainEncounter.DrainerCreated += BindDrainer;
                if (drainEncounter.ActiveDrainer != null) BindDrainer(drainEncounter.ActiveDrainer);
            }
            subscribed = true;
        }
        private bool UniqueRescue(int index)
        {
            FanRescueController2D source = additionalRescues[index];
            if (source == null || source == rescue) return false;
            for (int i = 0; i < index; i++) if (additionalRescues[i] == source) return false;
            return true;
        }
        private void BindDrainer(FanSoulDrainController2D source)
        {
            if (drain == source) return;
            if (drain != null) drain.FanDrainAccepted -= HandleDrained;
            drain = source;
            if (drain != null) drain.FanDrainAccepted += HandleDrained;
        }
        private void HandleRescued(FanActor2D fan, int reward)
        { if (isActiveAndEnabled && Valid && fan != null) honmoon.TryChange(rescuedFanGain); }
        private void HandleDrained(FanActor2D fan, float actualSoulLost)
        { if (isActiveAndEnabled && Valid && fan != null && Finite(actualSoulLost) && actualSoulLost > 0f) honmoon.TryChange(-actualSoulLost * drainLossPerSoul); }
        private void OnDisable()
        {
            if (!subscribed) return;
            if (rescue != null) rescue.RescueCompleted -= HandleRescued;
            if (additionalRescues != null) for (int i = 0; i < additionalRescues.Length; i++)
                if (UniqueRescue(i)) additionalRescues[i].RescueCompleted -= HandleRescued;
            if (drain != null) drain.FanDrainAccepted -= HandleDrained;
            if (drainEncounter != null) drainEncounter.DrainerCreated -= BindDrainer;
            subscribed = false;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
