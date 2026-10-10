using System;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.Rivals
{
    public enum JinuEncounterStage { Uninitialized, Encounter, Pressure, Defeated }
    /// <summary>Authored narrative cue boundaries; no film dialogue or assumed canonical outcome.</summary>
    [DefaultExecutionOrder(300)]
    [DisallowMultipleComponent, RequireComponent(typeof(SajaRivalController2D))]
    public sealed class JinuEncounterController2D : MonoBehaviour
    {
        [SerializeField] private JinuEncounterDefinition definition;
        private SajaRivalController2D rival;
        private EnemyAgent2D agent;
        private DamageReceiver2D self;
        private bool subscribed;
        public JinuEncounterStage Stage { get; private set; }
        public event Action<JinuEncounterStage, string> NarrativeCueRequested;
        private void Start()
        {
            rival = GetComponent<SajaRivalController2D>();
            agent = GetComponent<EnemyAgent2D>();
            self = GetComponent<DamageReceiver2D>();
            if (definition == null || !definition.IsValid(out _) || rival == null || agent == null ||
                self == null || !self.IsAlive || !rival.TrySetDefinition(definition.Opening, out _) || !self.IsAlive)
            { Debug.LogError("Jinu encounter requires valid authored profiles and a live rival.", this); enabled = false; return; }
            Subscribe();
            Stage = JinuEncounterStage.Encounter;
            Publish(definition.EncounterCue);
        }
        private void OnEnable()
        {
            if (Stage == JinuEncounterStage.Uninitialized) return;
            Subscribe();
            if (self != null && !self.IsAlive) HandleDeath(self);
        }
        private void Subscribe()
        {
            if (subscribed || self == null) return;
            self.Died += HandleDeath;
            subscribed = true;
        }
        private void Update()
        {
            if (Stage != JinuEncounterStage.Encounter || self == null || !self.IsAlive || !rival.isActiveAndEnabled ||
                !agent.isActiveAndEnabled || self.CurrentHealth / self.MaximumHealth > definition.PressureHealthFraction) return;
            // Apply only at a safe boundary, without cancelling an in-flight telegraph/recovery.
            if (agent.State != EnemyState2D.Idle && agent.State != EnemyState2D.Chasing) return;
            if (!rival.TrySetDefinition(definition.Pressure, out _) || !isActiveAndEnabled ||
                Stage != JinuEncounterStage.Encounter || !self.IsAlive) return;
            Stage = JinuEncounterStage.Pressure;
            Publish(definition.PressureCue);
        }
        private void HandleDeath(DamageReceiver2D actor)
        {
            if (actor != self || Stage == JinuEncounterStage.Defeated) return;
            Stage = JinuEncounterStage.Defeated;
            Publish(definition.DefeatedCue);
        }
        private void OnDisable()
        {
            if (subscribed && self != null) self.Died -= HandleDeath;
            subscribed = false;
        }
        private void Publish(string cue)
        {
            Action<JinuEncounterStage,string> handlers = NarrativeCueRequested;
            if (handlers == null) return;
            JinuEncounterStage publishedStage = Stage;
            foreach (Action<JinuEncounterStage,string> handler in handlers.GetInvocationList())
                try { handler(publishedStage, cue); } catch (Exception error) { Debug.LogException(error,this); }
        }
    }
}
