using System;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Honmoon
{
    public enum HonmoonBand { Empty = 0, Critical = 20, Low = 40, Guarded = 60, Strong = 80, Full = 100 }

    /// <summary>Explicit scene-wide shared resource. No scene discovery or static singleton.</summary>
    [DisallowMultipleComponent]
    public sealed class HonmoonController2D : MonoBehaviour
    {
        [SerializeField] private HonmoonDefinition definition;
        private bool initialized;
        private bool publishing;
        public HonmoonDefinition Definition => definition;
        public float Value { get; private set; }
        public HonmoonBand Band { get; private set; }
        public bool IsInCrisis { get; private set; }
        public bool IsPublishing => publishing;
        public int CrisisGeneration { get; private set; }
        public event Action<float, float> ValueChanged;
        public event Action<bool> CrisisChanged;
        public event Action<HonmoonBand> BandChanged;
        private void Awake()
        {
            if (definition != null && definition.IsValid(out _)) Initialize(definition);
        }
        public bool TryConfigure(HonmoonDefinition value)
        {
            if (!isActiveAndEnabled || initialized || value == null || !value.IsValid(out _)) return false;
            Initialize(value); return true;
        }
        private void Initialize(HonmoonDefinition value)
        {
            definition = value;
            Value = value.InitialValue;
            Band = GetBand(Value);
            IsInCrisis = Value <= value.CrisisEnterValue;
            CrisisGeneration = IsInCrisis ? 1 : 0;
            initialized = true;
        }
        public bool TryChange(float delta)
        {
            if (!Finite(delta) || !Finite(Value + delta)) return false;
            return TrySetValue(Value + delta);
        }
        public bool TrySetValue(float value)
        {
            if (!isActiveAndEnabled || !initialized || publishing || definition == null ||
                !definition.IsValid(out _) || !Finite(value)) return false;
            float next = Mathf.Clamp(value, 0f, 100f);
            if (next == Value) return false;
            float previous = Value;
            HonmoonBand previousBand = Band;
            bool previousCrisis = IsInCrisis;
            Value = next;
            Band = GetBand(next);
            IsInCrisis = previousCrisis ? next < definition.CrisisExitValue : next <= definition.CrisisEnterValue;
            if (IsInCrisis != previousCrisis) CrisisGeneration = unchecked(CrisisGeneration + 1);
            publishing = true;
            try
            {
                Publish(ValueChanged, previous, next);
                if (Band != previousBand) Publish(BandChanged, Band);
                if (IsInCrisis != previousCrisis) Publish(CrisisChanged, IsInCrisis);
            }
            finally { publishing = false; }
            return true;
        }
        private static HonmoonBand GetBand(float value) => value >= 100f ? HonmoonBand.Full :
            value >= 80f ? HonmoonBand.Strong : value >= 60f ? HonmoonBand.Guarded :
            value >= 40f ? HonmoonBand.Low : value >= 20f ? HonmoonBand.Critical : HonmoonBand.Empty;
        private void Publish<T>(Action<T> handlers, T value)
        {
            if (handlers == null) return;
            foreach (Action<T> handler in handlers.GetInvocationList())
                try { handler(value); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        private void Publish(Action<float, float> handlers, float before, float after)
        {
            if (handlers == null) return;
            foreach (Action<float, float> handler in handlers.GetInvocationList())
                try { handler(before, after); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
