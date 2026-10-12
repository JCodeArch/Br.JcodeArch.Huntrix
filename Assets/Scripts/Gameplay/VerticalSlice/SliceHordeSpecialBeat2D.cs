using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Honmoon;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceHordeSpecialBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private HordeDefinition hordeProfile;
        [SerializeField] private GameObject flyingEnemy;
        [SerializeField] private HonmoonCrisisDefinition crisisProfile;
        [SerializeField] private float demonstrationHonmoonValue = 20f;
        private HonmoonCrisisController2D crisis;
        private VerticalSliceSession2D session;
        private HordeController2D horde;
        private GameObject owner;
        private bool specialSucceeded;
        private float refresh;
        public HordeController2D CurrentHorde => horde;
        public bool SpecialDemonstrated => specialSucceeded;
        public override string Title => "Horda: L Special; E unir equipe na crise Honmoon";
        public override bool TryBegin(VerticalSliceSession2D value)
        {
            if (float.IsNaN(demonstrationHonmoonValue) || float.IsInfinity(demonstrationHonmoonValue) || demonstrationHonmoonValue < 0f || demonstrationHonmoonValue > 100f || crisisProfile == null || !crisisProfile.IsValid(out _) || value == null || hordeProfile == null || !hordeProfile.IsValid(out _) || flyingEnemy == null || !value.TryLoadStage(stage)) return false;
            session = value; IsComplete = false; specialSucceeded = false;
            owner = new GameObject("Slice bounded horde"); horde = owner.AddComponent<HordeController2D>();
            var points = new Transform[stage.EnemySpawnCount];
            for (int i = 0; i < points.Length; i++) { var point = new GameObject("Horde spawn "+i); point.transform.SetParent(owner.transform); point.transform.position = stage.GetEnemySpawn(i); points[i] = point.transform; }
            if (!horde.TryConfigure(hordeProfile, points, value.Manager, null) || !horde.TryStart() ||
                !value.Aerial.Configure(value.Stage, value.Manager, flyingEnemy) || !value.Aerial.TrySpawn()) return false;
            // This demonstration starts charged; production progression still uses accepted defeat charge.
            value.Special.TryAddCharge(value.Special.Definition.MaximumCharge);
            if (session != value || !value.IsRunning || !value.isActiveAndEnabled || owner == null) return false;
            var union = owner.AddComponent<CrisisTeamUnion2D>(); crisis = owner.AddComponent<HonmoonCrisisController2D>();
            if (!union.TryConfigure(crisisProfile, new[] { value.Manager }) || !crisis.TryConfigure(value.Honmoon, crisisProfile, horde, union) ||
                (demonstrationHonmoonValue > value.Honmoon.Definition.CrisisEnterValue || (value.Honmoon.Value != demonstrationHonmoonValue && !value.Honmoon.TrySetValue(demonstrationHonmoonValue)) || !value.Honmoon.IsInCrisis) || session != value || !value.IsRunning || !value.isActiveAndEnabled) return false;
            value.Special.Completed += SpecialCompleted; refresh = 0f;
            return true;
        }
        public override void OnInput(SliceInputFrame frame) { if (frame.Interact && crisis != null) crisis.TryUnite(); }
        private void SpecialCompleted(bool cancelled) { if (!cancelled) specialSucceeded = true; }
        public override void Tick(float deltaTime)
        {
            if (session == null || horde == null) return;
            refresh -= deltaTime;
            if (refresh <= 0f) { refresh = .25f; var targets = new List<DamageReceiver2D>(64); for (int i = 0; i < horde.AliveCount; i++) { var enemy = horde.GetOwnedAt(i); if (enemy != null) targets.Add(enemy.Self); } session.TrySetExternalSpecialTargets(targets.ToArray()); }
            bool aerialAlive = false; for (int i = 0; i < session.Aerial.Count; i++) if (session.Aerial.GetAt(i) != null && session.Aerial.GetAt(i).Self.IsAlive) aerialAlive = true;
            // Clearing is natural wave completion; actual Special use is recorded separately by the playtest recorder.
            IsComplete = horde.IsComplete && !aerialAlive;
        }
        public override void Exit()
        {
            if (session != null && session.Special != null) session.Special.Completed -= SpecialCompleted;
            if (owner != null) owner.SetActive(false);
            if (horde != null) horde.Stop(); if (owner != null) Destroy(owner); owner = null; horde = null; crisis = null; session = null; base.Exit();
        }
    }
}
