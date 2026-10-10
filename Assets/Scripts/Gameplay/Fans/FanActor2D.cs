using System;
using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    public enum FanState { Normal, Drained, Critical, Corrupted }
    public enum FanContentIntensity { Gentle, Standard, Intense }
    public interface IFanProtectionSource2D { bool Covers(FanActor2D fan); }
    /// <summary>Independent fan soul/state: intentionally not a combat damage receiver or faction.</summary>
    [DisallowMultipleComponent]
    public sealed class FanActor2D : MonoBehaviour
    {
        [SerializeField] private FanDefinition definition;
        [SerializeField] private FanRegistry2D registry;
        [SerializeField] private FanContentIntensity contentIntensity = FanContentIntensity.Gentle;
        private readonly HashSet<MonoBehaviour> protectionSources = new HashSet<MonoBehaviour>();
        public FanDefinition Definition => definition;
        public float Soul { get; private set; }
        public FanState State { get; private set; }
        public FanContentIntensity ContentIntensity => contentIntensity;
        public bool IsConfigurationValid => definition != null && definition.IsValid(out _);
        public event Action<FanActor2D, FanState, FanState> StateChanged;
        public event Action<FanActor2D, float, float> SoulChanged;
        public event Action<FanContentIntensity> PresentationChanged;
        private bool changing;
        private bool changingPresentation;
        private void Awake() { Soul = IsConfigurationValid ? definition.MaximumSoul : 0f; State = IsConfigurationValid ? FanState.Normal : FanState.Corrupted; }
        private void OnEnable() { if (registry != null && !registry.Register(this)) Debug.LogWarning("Fan registry refused capacity or configuration.", this); }
        private void OnDisable() { if (registry != null) registry.Unregister(this); protectionSources.Clear(); }
        public bool TryDrain(float amount)
        {
            if (!CanChange(amount) || IsProtected || Soul <= 0f) return false;
            return Change(Mathf.Max(0f, Soul - amount));
        }
        public bool TryRecover(float amount)
        {
            if (!CanChange(amount) || Soul >= definition.MaximumSoul) return false;
            return Change(Mathf.Min(definition.MaximumSoul, Soul + amount));
        }
        private bool CanChange(float amount) => !changing && isActiveAndEnabled && IsConfigurationValid &&
            !float.IsNaN(amount) && !float.IsInfinity(amount) && amount > 0f;
        private bool Change(float value)
        {
            if (value == Soul) return false;
            changing = true;
            float before = Soul; FanState previous = State;
            Soul = value;
            State = Soul <= 0f ? FanState.Corrupted : Soul >= definition.MaximumSoul ? FanState.Normal :
                Soul <= definition.MaximumSoul * definition.CriticalFraction ? FanState.Critical : FanState.Drained;
            try
            {
                Publish(SoulChanged, callback => callback(this, before, Soul));
                if (previous != State) Publish(StateChanged, callback => callback(this, previous, State));
                return true;
            }
            finally { changing = false; }
        }
        public bool TrySetContentIntensity(FanContentIntensity value)
        {
            if (changingPresentation || !Enum.IsDefined(typeof(FanContentIntensity), value) || value == contentIntensity) return false;
            changingPresentation = true;
            contentIntensity = value;
            try { Publish(PresentationChanged, callback => callback(value)); return true; }
            finally { changingPresentation = false; }
        }
        public bool IsProtected
        {
            get
            {
                if (!isActiveAndEnabled) return false;
                foreach (MonoBehaviour source in protectionSources)
                    if (source != null && source.isActiveAndEnabled && source is IFanProtectionSource2D provider && provider.Covers(this)) return true;
                return false;
            }
        }
        public bool RegisterProtection(MonoBehaviour source) => isActiveAndEnabled && source != null &&
            source is IFanProtectionSource2D && protectionSources.Count < 16 && protectionSources.Add(source);
        public void UnregisterProtection(MonoBehaviour source) => protectionSources.Remove(source);
        private void Publish<T>(T handlers, Action<T> invoke) where T : Delegate
        {
            if (handlers == null) return;
            foreach (Delegate handler in handlers.GetInvocationList())
                try { invoke((T)handler); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
    }
}
