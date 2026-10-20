using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.FanEncounters;
using HuntrX.Gameplay.Honmoon;
using UnityEngine;

namespace HuntrX.Gameplay.Audio
{
    [DisallowMultipleComponent]
    public sealed class CombatAudioBridge2D : MonoBehaviour
    {
        private GameAudioManager2D audioManager;
        private CharacterManager2D manager;
        private HonmoonController2D honmoon;
        private FanRescueController2D[] rescues;
        private FanEncounterController2D encounter;
        private DamageReceiver2D actor;
        private AttackController2D attack;
        private DashController2D dash;
        private ZoeyRangedAttack2D ranged;
        private ParryController2D parry;
        private AttackState2D previousAttack;
        private int previousComboStep = -1;
        private bool previousParry;

        public bool Configure(GameAudioManager2D audio, CharacterManager2D slot, HonmoonController2D resource, FanRescueController2D[] sources)
        {
            if (audio == null || slot == null || resource == null || sources == null || sources.Length > 64) return false;
            for (int i = 0; i < sources.Length; i++)
            {
                if (sources[i] == null) return false;
                for (int j = 0; j < i; j++) if (sources[i] == sources[j]) return false;
            }
            Unbind();
            audioManager = audio;
            manager = slot;
            honmoon = resource;
            rescues = (FanRescueController2D[])sources.Clone();
            if (isActiveAndEnabled) Bind();
            return true;
        }

        public void BindEncounter(FanEncounterController2D value)
        {
            if (encounter != null) encounter.RescueCompleted -= HandleRescue;
            encounter = value;
            if (isActiveAndEnabled && encounter != null) encounter.RescueCompleted += HandleRescue;
        }

        private void Bind()
        {
            if (encounter != null) encounter.RescueCompleted += HandleRescue;
            manager.ActiveCharacterChanged += HandleActor;
            honmoon.BandChanged += HandleBand;
            foreach (var rescue in rescues) rescue.RescueCompleted += HandleRescue;
            HandleActor(null, manager.ActiveCharacter);
        }

        private void Unbind()
        {
            if (encounter != null) encounter.RescueCompleted -= HandleRescue;
            DetachActor();
            if (manager != null) manager.ActiveCharacterChanged -= HandleActor;
            if (honmoon != null) honmoon.BandChanged -= HandleBand;
            if (rescues != null) foreach (var rescue in rescues) if (rescue != null) rescue.RescueCompleted -= HandleRescue;
        }

        private void DetachActor()
        {
            if (actor != null)
            {
                actor.DamageDealt -= HandleHit;
                actor.DamageReceived -= HandleReceived;
            }
            if (dash != null) dash.DashStateChanged -= HandleDash;
            if (ranged != null) ranged.ShotFired -= HandleShot;
            actor = null;
            attack = null;
            dash = null;
            ranged = null;
            parry = null;
            previousComboStep = -1;
        }

        private void HandleActor(DamageReceiver2D previous, DamageReceiver2D current)
        {
            DetachActor();
            actor = current;
            if (actor == null) return;
            actor.DamageDealt += HandleHit;
            actor.DamageReceived += HandleReceived;
            attack = actor.GetComponent<AttackController2D>();
            dash = actor.GetComponent<DashController2D>();
            ranged = actor.GetComponent<ZoeyRangedAttack2D>();
            parry = actor.GetComponent<ParryController2D>();
            previousAttack = attack == null ? AttackState2D.Idle : attack.State;
            previousComboStep = attack == null ? -1 : attack.CurrentComboStepIndex;
            previousParry = parry != null && parry.IsWindowActive;
            if (dash != null) dash.DashStateChanged += HandleDash;
            if (ranged != null) ranged.ShotFired += HandleShot;
        }

        private void Play(AudioCue cue)
        {
            if (audioManager != null) audioManager.TryPlay(cue, actor == null ? transform.position : actor.transform.position);
        }

        private void HandleHit(CombatImpactEvent value) => Play(AudioCue.Hit);
        private void HandleReceived(CombatImpactEvent value) => Play(AudioCue.Enemy);
        private void HandleDash(bool active) { if (active) Play(AudioCue.Dash); }
        private void HandleShot(Vector2 origin, Vector2 direction) => Play(AudioCue.ZoeyShinkalThrow);
        private void HandleBand(HonmoonBand value) => Play(AudioCue.Honmoon);
        private void HandleRescue(FanActor2D fan, int amount) => Play(AudioCue.Rescue);

        private void Update()
        {
            if (attack != null)
            {
                bool stepStarted = (attack.State != previousAttack && (attack.State == AttackState2D.Startup || attack.State == AttackState2D.Active))
                                || (attack.State != AttackState2D.Idle && attack.CurrentComboStepIndex != previousComboStep);
                if (stepStarted)
                {
                    Play(ResolveHeroComboCue(attack.CurrentComboStepIndex));
                }
                previousAttack = attack.State;
                previousComboStep = attack.State == AttackState2D.Idle ? -1 : attack.CurrentComboStepIndex;
            }
            bool window = parry != null && parry.IsWindowActive;
            if (window && !previousParry) Play(AudioCue.Parry);
            previousParry = window;
        }

        private AudioCue ResolveHeroComboCue(int stepIndex)
        {
            string id = manager != null && manager.ActivePrefab != null
                ? manager.ActivePrefab.name
                : (actor != null ? actor.name : string.Empty);

            if (id.Contains("Mira"))
            {
                // Mira: 0 = Gokdo Polearm Sweep, 1 = Street Brawler Punches, 2+ = Airplane Stainless-Steel Coffee Pot CLANG!
                if (stepIndex == 0) return AudioCue.MiraGokdoSweep;
                if (stepIndex == 1) return AudioCue.MiraBrawlerPunch;
                return AudioCue.MiraCoffeePotClang;
            }
            if (id.Contains("Zoey"))
            {
                // Zoey: 0/1 = Shinkal Twin Slash, 2+ = Flying Leg-Scissor Chokehold Slam!
                if (stepIndex <= 1) return AudioCue.ZoeyShinkalSlash;
                return AudioCue.ZoeyLegChokeholdSlam;
            }
            // Rumi: 0 = Saingeom Slash 1, 1 = Saingeom Slash 2, 2 = Rumi Taekwondo High Kick, 3+ = Saingeom Finisher
            if (stepIndex == 0) return AudioCue.RumiBladeSlash1;
            if (stepIndex == 1) return AudioCue.RumiBladeSlash2;
            if (stepIndex == 2) return AudioCue.RumiTaekwondoKick;
            return AudioCue.RumiBladeSlash3;
        }

        private void OnEnable() { if (audioManager != null) Bind(); }
        private void OnDisable() { Unbind(); }
    }
}
