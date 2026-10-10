using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent, RequireComponent(typeof(FanActor2D))]
    public sealed class FanRescueController2D : MonoBehaviour, IFanProtectionSource2D
    {
        [SerializeField] private FanRescueDefinition definition;
        private FanActor2D fan;
        private DamageReceiver2D rescuer;
        private float recoveryRate;
        private bool completing;
        public bool IsRecovering { get; private set; }
        public int CompletedRescueCount { get; private set; }
        public event Action<FanActor2D, int> RescueCompleted;
        private void Awake() => fan = GetComponent<FanActor2D>();
        public bool TryBeginRescue(DamageReceiver2D source)
        {
            if (!isActiveAndEnabled || IsRecovering || completing || fan == null || !fan.isActiveAndEnabled ||
                !fan.IsConfigurationValid || fan.State == FanState.Normal || definition == null || !definition.IsValid(out _) ||
                !Eligible(source)) return false;
            rescuer = source;
            recoveryRate = (fan.Definition.MaximumSoul - fan.Soul) / definition.RecoveryDuration;
            if (float.IsNaN(recoveryRate) || float.IsInfinity(recoveryRate) || recoveryRate <= 0f) { rescuer = null; return false; }
            IsRecovering = true;
            if (!fan.RegisterProtection(this)) { IsRecovering = false; rescuer = null; return false; }
            return true;
        }
        private void Update()
        {
            if (!IsRecovering) return;
            if (!Covers(fan)) { Cancel(); return; }
            if (Time.deltaTime <= 0f) return;
            fan.TryRecover(recoveryRate * Time.deltaTime);
            if (!IsRecovering || !Covers(fan)) return;
            if (fan.State == FanState.Normal) Complete();
        }
        public bool Covers(FanActor2D target) => isActiveAndEnabled && IsRecovering && target == fan &&
            target != null && target.isActiveAndEnabled && target.IsConfigurationValid && definition != null && definition.IsValid(out _) && Eligible(rescuer);
        private bool Eligible(DamageReceiver2D source) => source != null && source.isActiveAndEnabled && source.IsAlive &&
            source.Faction == CombatFaction2D.HuntrX &&
            Vector2.Distance(transform.position, source.transform.position) <= definition.InteractionRange;
        private void Complete()
        {
            if (completing || !IsRecovering || CompletedRescueCount == int.MaxValue) { Cancel(); return; }
            completing = true;
            Cancel();
            CompletedRescueCount++;
            Action<FanActor2D, int> handlers = RescueCompleted;
            int reward = definition.RewardAmount;
            try
            {
                if (handlers != null)
                    foreach (Action<FanActor2D, int> handler in handlers.GetInvocationList())
                        try { handler(fan, reward); } catch (Exception exception) { Debug.LogException(exception, this); }
            }
            finally { completing = false; }
        }
        public void Cancel()
        {
            IsRecovering = false;
            if (fan != null) fan.UnregisterProtection(this);
            rescuer = null; recoveryRate = 0f;
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
