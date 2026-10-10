using System;
using HuntrX.Data;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.Honmoon
{
    [DefaultExecutionOrder(200)]
    [DisallowMultipleComponent]
    public sealed class HonmoonCrisisController2D : MonoBehaviour
    {
        [SerializeField] private HonmoonController2D honmoon;
        [SerializeField] private HonmoonCrisisDefinition definition;
        [SerializeField] private HordeController2D horde;
        [SerializeField] private MonoBehaviour teamUnionAction;
        private bool subscribed;
        private bool uniting;
        private bool usedThisCrisis;
        private bool observedCrisis;
        private int crisisGeneration;
        private int observedResourceGeneration = -1;
        public bool IsIntensifying { get; private set; }
        public bool CanUnite => isActiveAndEnabled && honmoon != null && honmoon.isActiveAndEnabled &&
            honmoon.IsInCrisis && !honmoon.IsPublishing && !uniting && !usedThisCrisis && definition != null && definition.IsValid(out _) &&
            teamUnionAction != null && teamUnionAction.isActiveAndEnabled && teamUnionAction is ITeamUnionAction2D;
        public event Action<bool> IntensityChanged;
        public event Action TeamUnited;
        public bool TryConfigure(HonmoonController2D resource, HonmoonCrisisDefinition profile,
            HordeController2D swarm, MonoBehaviour unionAction)
        {
            if (!isActiveAndEnabled || subscribed || resource == null || profile == null || !profile.IsValid(out _) ||
                swarm == null || unionAction == null || !(unionAction is ITeamUnionAction2D)) return false;
            honmoon = resource; definition = profile; horde = swarm; teamUnionAction = unionAction;
            Subscribe(); ApplyCrisis(honmoon.IsInCrisis); return true;
        }
        private void OnEnable() { Subscribe(); if (honmoon != null) ApplyCrisis(honmoon.IsInCrisis); }
        private void Subscribe()
        {
            if (subscribed || honmoon == null) return;
            honmoon.CrisisChanged += ApplyCrisis; subscribed = true;
        }
        private void ApplyCrisis(bool active)
        {
            if (honmoon != null && observedResourceGeneration != honmoon.CrisisGeneration)
            { observedResourceGeneration = honmoon.CrisisGeneration; crisisGeneration++; usedThisCrisis = false; }
            if (active != observedCrisis) { observedCrisis = active; crisisGeneration++; }
            bool previous = IsIntensifying;
            if (!active) { usedThisCrisis = false; if (horde != null) horde.ClearIntensity(this); IsIntensifying = false; }
            else if (isActiveAndEnabled && honmoon != null && honmoon.isActiveAndEnabled && definition != null &&
                definition.IsValid(out _) && horde != null)
                IsIntensifying = horde.TrySetIntensity(this, definition.SpawnIntervalMultiplier, definition.ExtraConcurrentEnemies);
            if (previous != IsIntensifying) Publish(IntensityChanged, IsIntensifying);
        }
        private void Update()
        {
            if (honmoon == null || !honmoon.isActiveAndEnabled || definition == null || !definition.IsValid(out _))
            { if (horde != null) horde.ClearIntensity(this); IsIntensifying = false; return; }
            if (honmoon.IsInCrisis && !IsIntensifying) ApplyCrisis(true);
        }
        public bool TryUnite()
        {
            if (!CanUnite) return false;
            int generation = crisisGeneration;
            HonmoonController2D resource = honmoon;
            HonmoonCrisisDefinition profile = definition;
            MonoBehaviour action = teamUnionAction;
            usedThisCrisis = true;
            uniting = true;
            try
            {
                if (!((ITeamUnionAction2D)action).TryExecuteTeamUnion())
                { if (generation == crisisGeneration) usedThisCrisis = false; return false; }
                if (!isActiveAndEnabled || generation != crisisGeneration || resource == null ||
                    !resource.isActiveAndEnabled || profile == null || !profile.IsValid(out _)) return true;
                resource.TryChange(profile.UnionRecoveryValue);
                Action handlers = TeamUnited;
                if (handlers != null) foreach (Action handler in handlers.GetInvocationList())
                    try { handler(); } catch (Exception exception) { Debug.LogException(exception, this); }
                return true;
            }
            finally { uniting = false; }
        }
        private void Publish(Action<bool> handlers, bool value)
        {
            if (handlers == null) return;
            foreach (Action<bool> handler in handlers.GetInvocationList())
                try { handler(value); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        private void OnDisable()
        {
            if (subscribed && honmoon != null) honmoon.CrisisChanged -= ApplyCrisis;
            subscribed = false;
            if (horde != null) horde.ClearIntensity(this);
            IsIntensifying = false;
        }
    }
}
