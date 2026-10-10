using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Protection;
using UnityEngine;
namespace HuntrX.Gameplay.Honmoon
{
    /// <summary>Provisional union protection for explicit slots; trio indicators also work in solo.</summary>
    [DisallowMultipleComponent]
    public sealed class CrisisTeamUnion2D : MonoBehaviour, ITeamUnionAction2D, IDamageProtectionSource2D
    {
        [SerializeField] private HonmoonCrisisDefinition definition;
        [SerializeField] private CharacterManager2D[] participants = new CharacterManager2D[0];
        private readonly List<DamageProtection2D> protectedActors = new List<DamageProtection2D>(3);
        private readonly List<LineRenderer> indicators = new List<LineRenderer>(3);
        private float remaining;
        private bool activating;
        public bool IsActive => remaining > 0f;
        public event Action Activated;
        public bool TryConfigure(HonmoonCrisisDefinition profile, CharacterManager2D[] slots)
        {
            if (!isActiveAndEnabled || IsActive || activating || profile == null || !profile.IsValid(out _) ||
                slots == null || slots.Length == 0 || slots.Length > 3) return false;
            for (int i = 0; i < slots.Length; i++)
            { if (slots[i] == null) return false; for (int j = 0; j < i; j++) if (slots[i] == slots[j]) return false; }
            definition = profile; participants = (CharacterManager2D[])slots.Clone(); return true;
        }
        public bool TryExecuteTeamUnion()
        {
            if (!isActiveAndEnabled || activating || IsActive || definition == null || !definition.IsValid(out _) ||
                participants == null || participants.Length == 0 || participants.Length > 3) return false;
            DamageReceiver2D[] actors = new DamageReceiver2D[participants.Length];
            for (int i = 0; i < participants.Length; i++)
            {
                CharacterManager2D manager = participants[i];
                DamageReceiver2D actor = manager != null && manager.isActiveAndEnabled ? manager.ActiveCharacter : null;
                if (actor == null || !actor.isActiveAndEnabled || !actor.IsAlive || actor.Faction != CombatFaction2D.HuntrX) return false;
                for (int j = 0; j < i; j++) if (actor == actors[j]) return false;
                actors[i] = actor;
            }
            activating = true;
            try
            {
                foreach (DamageReceiver2D actor in actors)
                {
                    DamageProtection2D marker = actor.GetComponent<DamageProtection2D>();
                    if (marker == null) marker = actor.gameObject.AddComponent<DamageProtection2D>();
                    if (!marker.RegisterExternalSource(this)) { Clear(); return false; }
                    protectedActors.Add(marker);
                }
                remaining = definition.UnionProtectionDuration;
                CreateIndicators();
                Action handlers = Activated;
                if (handlers != null) foreach (Action handler in handlers.GetInvocationList())
                    try { handler(); } catch (Exception exception) { Debug.LogException(exception, this); }
                return true;
            }
            finally { activating = false; }
        }
        public bool Covers(DamageReceiver2D receiver)
        {
            if (!isActiveAndEnabled || !IsActive || receiver == null || !receiver.isActiveAndEnabled || !receiver.IsAlive) return false;
            foreach (DamageProtection2D marker in protectedActors)
                if (marker != null && marker.isActiveAndEnabled && marker.GetComponent<DamageReceiver2D>() == receiver) return true;
            return false;
        }
        private void Update()
        {
            if (!IsActive) return;
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            if (remaining <= 0f) { Clear(); return; }
            DamageProtection2D anchor = null;
            foreach (DamageProtection2D marker in protectedActors)
                if (marker != null && Covers(marker.GetComponent<DamageReceiver2D>())) { anchor = marker; break; }
            if (anchor == null) { Clear(); return; }
            for (int i = 0; i < indicators.Count; i++)
            {
                LineRenderer line = indicators[i];
                if (line == null) continue;
                Vector3 center = anchor.transform.position + new Vector3((i - 1) * .45f, 1f, 0f);
                line.SetPosition(0, center + Vector3.down * .22f);
                line.SetPosition(1, center + Vector3.left * .22f);
                line.SetPosition(2, center + Vector3.up * .22f);
                line.SetPosition(3, center + Vector3.right * .22f);
            }
        }
        private void CreateIndicators()
        {
            Color[] colors = { new Color(.6f,.3f,1f), new Color(1f,.4f,.65f), new Color(.3f,.8f,1f) };
            for (int i = 0; i < 3; i++)
            {
                GameObject indicator = new GameObject("Union_Performer_" + i);
                indicator.transform.SetParent(transform, false);
                LineRenderer line = indicator.AddComponent<LineRenderer>();
                line.positionCount = 4; line.loop = true; line.useWorldSpace = true;
                line.startWidth = line.endWidth = .06f; line.startColor = line.endColor = colors[i];
                line.sortingOrder = 15;
                Shader shader = Shader.Find("Sprites/Default");
                if (shader != null) line.material = new Material(shader);
                indicators.Add(line);
            }
        }
        private void Clear()
        {
            remaining = 0f;
            foreach (DamageProtection2D marker in protectedActors) if (marker != null) marker.UnregisterExternalSource(this);
            protectedActors.Clear();
            foreach (LineRenderer line in indicators)
                if (line != null) { line.gameObject.SetActive(false); if (line.sharedMaterial != null) Destroy(line.sharedMaterial); Destroy(line.gameObject); }
            indicators.Clear();
        }
        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
    }
}
