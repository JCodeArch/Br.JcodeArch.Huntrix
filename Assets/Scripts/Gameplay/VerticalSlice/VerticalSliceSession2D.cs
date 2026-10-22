using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.FanEncounters;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Levels;
using HuntrX.Gameplay.Movement;
using HuntrX.Gameplay.Performance;
using HuntrX.Gameplay.Resources;
using HuntrX.Gameplay.Special;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    /// <summary>One explicit solo prototype session; beats own their boundaries and report completion.</summary>
    [DisallowMultipleComponent]
    public sealed class VerticalSliceSession2D : MonoBehaviour
    {
        [SerializeField] private GameObject[] playerPrefabs = Array.Empty<GameObject>();
        [SerializeField] private SliceBeat2D[] beats = Array.Empty<SliceBeat2D>();
        [SerializeField] private Camera camera;
        [SerializeField] private FanEncounterDefinition fanProfile;
        [SerializeField] private FoodDefinition ramen;
        [SerializeField] private HonmoonDefinition honmoonProfile;
        [SerializeField] private HuntrXSpecialDefinition specialProfile;
        [SerializeField] private CombatPerformanceDefinition performanceProfile;
        [SerializeField] private bool autoStart = true;
        private readonly List<EnemyAgent2D> ownedEnemies = new List<EnemyAgent2D>(32);
        private DamageReceiver2D[] externalTargets = Array.Empty<DamageReceiver2D>();
        private GameObject slot, ownedCamera;
        private bool transitioning, stopping;
        /// <summary>Seconds enemies hold back after the player returns to a checkpoint (death or fall).</summary>
        public const float RespawnBreathingRoom = 1.5f;
        private float targetRefresh;
        public CharacterManager2D Manager { get; private set; }
        public CheckpointAttemptFlow2D CheckpointFlow { get; private set; }
        public StageBlockoutBuilder2D Stage { get; private set; }
        public SlicePlayerInput2D Input { get; private set; }
        public FoodInventory2D Inventory { get; private set; }
        public FoodChoiceController2D FoodChoice { get; private set; }
        public ScoreMilestoneFoodDelivery2D FoodDelivery { get; private set; }
        public FoodDefinition Ramen => ramen;
        public HonmoonController2D Honmoon { get; private set; }
        public HuntrXSpecialController2D Special { get; private set; }
        public CombatPerformanceController2D Performance { get; private set; }
        public FanEncounterController2D Fans { get; private set; }
        public AerialStageEncounter2D Aerial { get; private set; }
        public BossArenaController2D BossArena { get; private set; }
        public int BeatIndex { get; private set; } = -1;
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public bool Falling { get; set; }
        public string BeatTitle => BeatIndex >= 0 && BeatIndex < beats.Length && beats[BeatIndex] != null ? beats[BeatIndex].Title : "Preparando";
        public event Action<StageBlockoutBuilder2D> StageLoaded;
        public event Action<int> BeatChanged;
        public event Action<SliceInputFrame> InputObserved;
        private void Start() { if (autoStart && !TryBegin()) Debug.LogError("Vertical slice could not initialize its explicit composition.", this); }
        public bool TryBegin()
        {
            if (!isActiveAndEnabled || IsRunning || transitioning || stopping || slot != null || playerPrefabs == null ||
                playerPrefabs.Length != 3 || beats == null || beats.Length == 0 || beats.Length > 16 ||
                honmoonProfile == null || !honmoonProfile.IsValid(out _) || specialProfile == null || !specialProfile.IsValid(out _) ||
                performanceProfile == null || !performanceProfile.IsValid(out _) || fanProfile == null || !fanProfile.IsValid(out _) || ramen == null || !ramen.IsValid(out _)) return false;
            foreach (GameObject prefab in playerPrefabs) if (prefab == null || prefab.scene.IsValid()) return false;
            foreach (SliceBeat2D beat in beats) if (beat == null || !beat.isActiveAndEnabled) return false;
            slot = new GameObject("Vertical slice player slot");
            CheckpointFlow = slot.AddComponent<CheckpointAttemptFlow2D>(); Manager = slot.AddComponent<CharacterManager2D>();
            var switching = slot.AddComponent<CharacterSwitchController2D>();
            if (!switching.Configure(Manager, playerPrefabs, out _)) { Stop(); return false; }
            Input = slot.AddComponent<SlicePlayerInput2D>(); Input.TryBind(Manager, switching); Input.ActionsRequested += HandleInput;
            if (camera == null) { ownedCamera = new GameObject("Vertical slice camera"); camera = ownedCamera.AddComponent<Camera>(); }
            if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
            if(!slot.AddComponent<HuntrX.Presentation.Effects.StageLightingRig2D>().TryBind(camera,
                UnityEngine.Resources.Load<UnityEngine.Rendering.VolumeProfile>("Presentation/HuntrxBloom")))
            { Stop(); return false; }
            camera.orthographic = true; camera.orthographicSize = 3.7f; camera.backgroundColor = new Color(.05f,.06f,.14f);
            var presentation = slot.AddComponent<SlicePlayerPresentation2D>();
            if (!presentation.TryBind(Manager, camera) ||
                !slot.AddComponent<SliceCameraDirector2D>().TryBind(this, Manager, presentation, camera))
            { Stop(); return false; }
            Inventory = slot.AddComponent<FoodInventory2D>(); FoodChoice = slot.AddComponent<FoodChoiceController2D>();
            Honmoon = slot.AddComponent<HonmoonController2D>(); if (!Honmoon.TryConfigure(honmoonProfile)) { Stop(); return false; }
            Special = slot.AddComponent<HuntrXSpecialController2D>(); slot.AddComponent<SpecialSequenceEffects2D>();
            Performance = slot.AddComponent<CombatPerformanceController2D>();
            Fans = slot.AddComponent<FanEncounterController2D>(); Aerial = slot.AddComponent<AerialStageEncounter2D>(); BossArena = slot.AddComponent<BossArenaController2D>();
            if (!Fans.TryStart(fanProfile, new Vector3(65f, .8f, 0f))) { Stop(); return false; }
            FoodDelivery = slot.AddComponent<ScoreMilestoneFoodDelivery2D>();
            FoodDelivery.Configure(Manager, Inventory, ramen, Fans);
            Manager.ActiveCharacterChanged += HandleActor;
            CheckpointFlow.RespawnRequested += HandleRespawnRequested;
            IsRunning = true; IsComplete = false; BeatIndex = -1;
            if (!Advance()) { Stop(); return false; }
            return isActiveAndEnabled && IsRunning && BeatIndex == 0;
        }
        public bool TryLoadStage(StageBlockoutDefinition definition)
        {
            if (!IsRunning || definition == null || !definition.IsValid(out _)) return false;
            GameObject holder = new GameObject("Slice stage owner");
            var candidate = holder.AddComponent<StageBlockoutBuilder2D>();
            if (!candidate.TryBuild(definition)) { Destroy(holder); return false; }
            bool accepted = Manager.ActiveCharacter == null
                ? Manager.Configure(playerPrefabs[0], CheckpointFlow, candidate.StartPoint, definition.StageId+":start") && Manager.TrySpawn()
                : Manager.TryEnterStage(candidate.StartPoint, definition.StageId+":start");
            if (!accepted || !isActiveAndEnabled || !IsRunning) { candidate.TryClear(); Destroy(holder); return false; }
            StageBlockoutBuilder2D previous = Stage; Stage = candidate;
            if (previous != null) { previous.TryClear(); Destroy(previous.gameObject); }
            Publish(StageLoaded, callback => callback(candidate));
            return isActiveAndEnabled && IsRunning;
        }
        public bool TrySpawnEnemy(GameObject prefab, Vector2 position, out EnemyAgent2D agent)
        {
            agent = null;
            if (!IsRunning || ownedEnemies.Count >= 32 || prefab == null || prefab.scene.IsValid() || prefab.GetComponent<EnemyAgent2D>() == null ||
                Manager.ActiveCharacter == null || !Manager.ActiveCharacter.IsAlive || !Finite(position.x) || !Finite(position.y)) return false;
            GameObject instance = Instantiate(prefab, position, Quaternion.identity); instance.SetActive(true);
            DamageReceiver2D target = Manager.ActiveCharacter;
            EnemyAgent2D next = instance.GetComponent<EnemyAgent2D>();
            if (!IsRunning || !isActiveAndEnabled || next == null || !next.IsConfigurationValid || !next.TrySetTarget(target) || !Current(target) ||
                !next.isActiveAndEnabled || !next.Self.IsAlive) { instance.SetActive(false); Destroy(instance); return false; }
            ownedEnemies.Add(next); agent = next; return true;
        }
        public int LivingEnemies
        {
            get { int count = 0; foreach (EnemyAgent2D agent in ownedEnemies) if (Live(agent)) count++; return count; }
        }
        private void HandleRespawnRequested(RespawnRequest request) => SuppressEnemyAggro(RespawnBreathingRoom);

        /// <summary>Makes every living enemy (ground, aerial, boss) stand down briefly; the player cannot be re-hit while returning.</summary>
        public void SuppressEnemyAggro(float seconds)
        {
            foreach (EnemyAgent2D agent in ownedEnemies) if (agent != null) agent.TrySuppressAggro(seconds);
            if (Aerial != null) for (int i = 0; i < Aerial.Count; i++) { var flyer = Aerial.GetAt(i); if (flyer != null) flyer.TrySuppressAggro(seconds); }
            if (BossArena != null && BossArena.Boss != null) BossArena.Boss.TrySuppressAggro(seconds);
        }
        public void ClearEnemies()
        {
            while (ownedEnemies.Count > 0) { int i = ownedEnemies.Count-1; var agent = ownedEnemies[i]; ownedEnemies.RemoveAt(i); if (agent != null) { agent.Stop(); agent.gameObject.SetActive(false); Destroy(agent.gameObject); } }
            if (Aerial != null) Aerial.Clear(); if (BossArena != null) BossArena.Clear(); externalTargets = Array.Empty<DamageReceiver2D>();
        }
        public bool TrySetExternalSpecialTargets(DamageReceiver2D[] targets)
        { if (targets == null || targets.Length > 64) return false; externalTargets = (DamageReceiver2D[])targets.Clone(); return true; }
        private void Update()
        {
            if (!IsRunning || transitioning || Manager == null || Manager.ActiveCharacter == null || !Manager.ActiveCharacter.IsAlive) return;
            DamageReceiver2D currentActor = Manager.ActiveCharacter;
            // A switch from a Special callback cannot configure until its dispatch/resolution unwinds.
            if (Special != null && Special.Owner != currentActor) RebindSpecial(currentActor);
            for (int i = ownedEnemies.Count-1; i >= 0; i--)
            { if (i >= ownedEnemies.Count) continue; var agent = ownedEnemies[i]; if (Live(agent)) agent.TrySetTarget(currentActor); if (!Current(currentActor)) return; }
            if (Stage != null && !Falling)
            {
                DamageReceiver2D actor = Manager.ActiveCharacter;
                foreach (CheckpointAnchor2D checkpoint in Stage.CheckpointAnchors)
                {
                    if (checkpoint != null && checkpoint.TryGetCheckpoint(out string id, out _, out _) && id != CheckpointFlow.ActiveCheckpointId &&
                        Vector2.Distance(actor.transform.position, checkpoint.transform.position) <= 1.2f) CheckpointFlow.ActivateCheckpoint(checkpoint);
                    if (!Current(actor)) return;
                }
                if (Manager.ActiveCharacter.transform.position.y < Stage.Definition.StartPosition.y - 20f)
                {
                    var body = Manager.ActiveCharacter.GetComponent<Rigidbody2D>(); body.position = CheckpointFlow.ActiveCheckpointPosition; body.linearVelocity = Vector2.zero; SuppressEnemyAggro(RespawnBreathingRoom);
                }
            }
            targetRefresh -= Time.deltaTime;
            if (targetRefresh <= 0f) { targetRefresh = .25f; RefreshSpecial(); }
            if (!Current(currentActor)) return;
            beats[BeatIndex].Tick(Time.deltaTime);
            if (Current(currentActor) && beats[BeatIndex].IsComplete && Manager.CanEnterStage) Advance();
        }
        private bool Advance()
        {
            if (transitioning || stopping || !IsRunning) return false;
            transitioning = true;
            try
            {
                Special?.Cancel();
                if (!isActiveAndEnabled || !IsRunning || stopping) return false;
                if (BeatIndex >= 0) beats[BeatIndex].Exit();
                if (!isActiveAndEnabled || !IsRunning || stopping) return false;
                ClearEnemies();
                if (!isActiveAndEnabled || !IsRunning || stopping) return false;
                int next = BeatIndex+1;
                // Retain the session's results/presentation while retiring its gameplay input.
                if (next >= beats.Length) { IsRunning = false; IsComplete = true; if (Input != null) Input.enabled = false; Performance?.TryFinishRun(out _); return true; }
                if (!beats[next].TryBegin(this)) { beats[next].Exit(); IsRunning = false; if (Input != null) Input.enabled = false; Debug.LogError("Slice beat failed: "+beats[next].Title, this); return false; }
                if (!isActiveAndEnabled || !IsRunning) { beats[next].Exit(); return false; }
                BeatIndex = next; Publish(BeatChanged, callback => callback(next));
                return isActiveAndEnabled && IsRunning && BeatIndex == next;
            }
            finally { transitioning = false; }
        }
        private void HandleActor(DamageReceiver2D previous, DamageReceiver2D current)
        {
            FoodChoice?.TryBindSelf(current);
            RebindSpecial(current);
        }
        private void RebindSpecial(DamageReceiver2D current)
        {
            if (current == null || Special == null) return;
            Special.Cancel();
            Special.TryConfigure(specialProfile, current, Array.Empty<DamageReceiver2D>(), out _);
        }
        private void HandleInput(SliceInputFrame frame)
        {
            if (!IsRunning || transitioning || BeatIndex < 0) return;
            DamageReceiver2D actor = Manager.ActiveCharacter;
            if (frame.FoodSelf) FoodChoice.TryUseOnSelf(ramen);
            if (!Current(actor)) return;
            if (frame.FoodFan) { FanActor2D fan = NearestFan(); if (fan != null) FoodChoice.TryUseOnFan(ramen, fan); }
            if (!Current(actor)) return;
            if (frame.Special) Special.TryActivate();
            if (!Current(actor)) return;
            if (!IsRunning || !isActiveAndEnabled) return;
            beats[BeatIndex].OnInput(frame);
            if (Current(actor)) Publish(InputObserved, callback => callback(frame));
        }
        public FanActor2D NearestFan()
        {
            if (Fans == null || Fans.Registry == null || Manager.ActiveCharacter == null) return null;
            FanActor2D selected = null; float distance = float.PositiveInfinity;
            for (int i = 0; i < Fans.Registry.Count; i++) { FanActor2D fan = Fans.Registry.GetAt(i); if (fan == null || !fan.isActiveAndEnabled) continue; float d = Vector2.Distance(fan.transform.position, Manager.ActiveCharacter.transform.position); if (d < distance) { distance = d; selected = fan; } }
            return selected;
        }
        private void RefreshSpecial()
        {
            if (Special == null || Special.IsActive || Manager.ActiveCharacter == null) return;
            var targets = new List<DamageReceiver2D>(64);
            foreach (EnemyAgent2D agent in ownedEnemies) if (Live(agent) && targets.Count < 64) targets.Add(agent.Self);
            if (Aerial != null) for (int i = 0; i < Aerial.Count && targets.Count < 64; i++) if (Live(Aerial.GetAt(i))) targets.Add(Aerial.GetAt(i).Self);
            if (BossArena != null && Live(BossArena.Boss) && targets.Count < 64) targets.Add(BossArena.Boss.Self);
            foreach (DamageReceiver2D target in externalTargets) if (target != null && target.isActiveAndEnabled && target.IsAlive && !targets.Contains(target) && targets.Count < 64) targets.Add(target);
            Special.TrySetTargets(targets.ToArray(), out _);
            DamageReceiver2D nearest = null; float distance = float.PositiveInfinity;
            foreach (DamageReceiver2D target in targets) { float d = Vector2.Distance(target.transform.position, Manager.ActiveCharacter.transform.position); if (d < distance) { nearest = target; distance = d; } }
            Input.SetAimTarget(nearest);
        }
        public bool TryBeginPerformance()
        {
            if (Performance.IsRunning) return true;
            var rescues = new List<FanRescueController2D>(); if (Fans.Registry != null) for (int i = 0; i < Fans.RescueSourceCount; i++) if (Fans.GetRescueAt(i) != null) rescues.Add(Fans.GetRescueAt(i));
            return Performance.Configure(performanceProfile, Manager, rescues.ToArray()) && Performance.TryBeginRun();
        }
        public void SetExternalCharacterArt(bool enabled) { if (slot != null) slot.GetComponent<SlicePlayerPresentation2D>()?.SetCharacterVisible(!enabled); }
        public void Stop()
        {
            if (stopping) return; stopping = true; IsRunning = false;
            try { foreach (SliceBeat2D beat in beats) if (beat != null) beat.Exit(); ClearEnemies(); if (Stage != null) { Stage.TryClear(); Destroy(Stage.gameObject); Stage = null; }
                if (Input != null) Input.ActionsRequested -= HandleInput; if (Manager != null) Manager.ActiveCharacterChanged -= HandleActor; if (CheckpointFlow != null) CheckpointFlow.RespawnRequested -= HandleRespawnRequested;
                if (slot != null) { slot.SetActive(false); Destroy(slot); slot = null; }
                if (ownedCamera != null)
                {
                    // Destroy is deferred: a same-frame restart must not reuse the retiring camera.
                    if (camera != null && camera.gameObject == ownedCamera) camera = null;
                    ownedCamera.SetActive(false); Destroy(ownedCamera); ownedCamera = null;
                }
            } finally { stopping = false; }
        }
        private void Publish<T>(T handlers, Action<T> invoke) where T : Delegate { if (handlers != null) foreach (Delegate handler in handlers.GetInvocationList()) try { invoke((T)handler); } catch (Exception exception) { Debug.LogException(exception, this); } }
        private bool Current(DamageReceiver2D actor) => isActiveAndEnabled && IsRunning && !stopping && Manager != null && Manager.isActiveAndEnabled && actor != null && actor.isActiveAndEnabled && actor.IsAlive && Manager.ActiveCharacter == actor;
        private static bool Live(EnemyAgent2D agent) => agent != null && agent.isActiveAndEnabled && agent.Self != null && agent.Self.isActiveAndEnabled && agent.Self.IsAlive;
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable() => Stop(); private void OnDestroy() => Stop();
    }
}
