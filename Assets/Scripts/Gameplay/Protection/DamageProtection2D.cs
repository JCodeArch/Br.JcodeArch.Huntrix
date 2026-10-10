using System.Collections.Generic;
using HuntrX.Gameplay.Combat;
using UnityEngine;

namespace HuntrX.Gameplay.Protection
{
    /// <summary>Explicit opt-in for temporary protection, retaining each source's collider membership.</summary>
    [DisallowMultipleComponent]
    public sealed class DamageProtection2D : MonoBehaviour
    {
        private readonly Dictionary<MiraProtectionField2D, HashSet<Collider2D>> sources =
            new Dictionary<MiraProtectionField2D, HashSet<Collider2D>>();

        private readonly HashSet<MonoBehaviour> externalSources = new HashSet<MonoBehaviour>();

        public bool IsProtected
        {
            get
            {
                if (!isActiveAndEnabled)
                {
                    return false;
                }

                DamageReceiver2D receiver = GetComponent<DamageReceiver2D>();
                foreach (MonoBehaviour source in externalSources)
                    if (source != null && source.isActiveAndEnabled && source is IDamageProtectionSource2D provider &&
                        provider.Covers(receiver)) return true;

                // A seeded pair may separate before physics creates it. Validate registered pairs on demand;
                // trigger callbacks and lifecycle changes still own the collections, with no per-frame query.
                foreach (KeyValuePair<MiraProtectionField2D, HashSet<Collider2D>> source in sources)
                {
                    if (source.Key == null)
                    {
                        continue;
                    }
                    foreach (Collider2D collider in source.Value)
                    {
                        if (source.Key.ContainsOverlap(collider))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private void Awake() => GetComponent<DamageReceiver2D>()?.RegisterProtection(this);

        internal void Register(MiraProtectionField2D source, Collider2D overlap)
        {
            if (!isActiveAndEnabled || source == null || !source.IsActive || overlap == null)
            {
                return;
            }

            if (!sources.TryGetValue(source, out HashSet<Collider2D> colliders))
            {
                colliders = new HashSet<Collider2D>();
                sources.Add(source, colliders);
                source.Track(this);
            }
            colliders.Add(overlap);
        }

        internal void Unregister(MiraProtectionField2D source, Collider2D overlap)
        {
            if (!sources.TryGetValue(source, out HashSet<Collider2D> colliders))
            {
                return;
            }

            colliders.Remove(overlap);
            if (colliders.Count == 0)
            {
                UnregisterSource(source);
            }
        }

        internal void UnregisterSource(MiraProtectionField2D source)
        {
            if (sources.Remove(source) && source != null)
            {
                source.Untrack(this);
            }
        }

        public bool RegisterExternalSource(MonoBehaviour source)
        {
            if (!isActiveAndEnabled || source == null || !(source is IDamageProtectionSource2D)) return false;
            return externalSources.Add(source);
        }

        public void UnregisterExternalSource(MonoBehaviour source) => externalSources.Remove(source);

        private void ClearSources()
        {
            foreach (MiraProtectionField2D source in sources.Keys)
            {
                if (source != null)
                {
                    source.Untrack(this);
                }
            }
            sources.Clear();
            externalSources.Clear();
        }

        private void OnDisable() => ClearSources();
        private void OnDestroy() => ClearSources();
    }
}
