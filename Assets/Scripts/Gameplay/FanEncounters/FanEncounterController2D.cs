using System;
using HuntrX.Data;
using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.FanEncounters
{
    [DisallowMultipleComponent]
    public sealed class FanEncounterController2D : MonoBehaviour
    {
        [SerializeField] private FanEncounterDefinition definition;
        private GameObject owned;
        private FanRescueController2D[] rescues = Array.Empty<FanRescueController2D>();
        private bool creating;
        public GameObject RuntimeRoot => owned;
        public FanRegistry2D Registry { get; private set; }
        public int RescuedCount { get; private set; }
        public event Action<FanActor2D, int> RescueCompleted;
        public FanRescueController2D GetRescueAt(int index) => index >= 0 && index < rescues.Length ? rescues[index] : null;
        public int RescueSourceCount => rescues.Length;
        public bool TryStart(FanEncounterDefinition profile, Vector3 position)
        {
            if (!isActiveAndEnabled || creating || owned != null || profile == null || !profile.IsValid(out _) ||
                !Finite(position.x) || !Finite(position.y) || !Finite(position.z)) return false;
            creating = true; GameObject candidate = null;
            try
            {
                candidate = Instantiate(profile.GroupPrefab, position, Quaternion.identity);
                candidate.SetActive(true);
                FanRegistry2D registry = candidate.GetComponent<FanRegistry2D>();
                if (!isActiveAndEnabled || registry == null || registry.Count == 0)
                { candidate.SetActive(false); Destroy(candidate); return false; }
                owned = candidate; Registry = registry; definition = profile; RescuedCount = 0;
                rescues = candidate.GetComponentsInChildren<FanRescueController2D>(true);
                foreach (FanRescueController2D rescue in rescues) rescue.RescueCompleted += HandleRescued;
                for (int i = 0; i < registry.Count; i++)
                {
                    if (!isActiveAndEnabled || owned != candidate || Registry != registry) return false;
                    FanActor2D fan = registry.GetAt(i);
                    if (fan != null && profile.InitialSoulLoss > 0f) fan.TryDrain(profile.InitialSoulLoss);
                }
                return isActiveAndEnabled && owned != null;
            }
            finally { creating = false; }
        }
        private void HandleRescued(FanActor2D fan, int reward)
        {
            if (!isActiveAndEnabled || owned == null) return;
            RescuedCount = Mathf.Min(1000000, RescuedCount + 1);
            Action<FanActor2D, int> handlers = RescueCompleted;
            if (handlers != null) foreach (Action<FanActor2D, int> handler in handlers.GetInvocationList())
                try { handler(fan, reward); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        public void Stop()
        {
            GameObject previous = owned; owned = null; Registry = null;
            foreach (FanRescueController2D rescue in rescues) if (rescue != null) rescue.RescueCompleted -= HandleRescued;
            rescues = Array.Empty<FanRescueController2D>();
            if (previous != null) { previous.SetActive(false); Destroy(previous); }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable() => Stop();
        private void OnDestroy() => Stop();
    }
}
