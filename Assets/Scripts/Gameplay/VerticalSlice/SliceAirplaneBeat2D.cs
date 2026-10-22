using HuntrX.Data;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Narrative;
using HuntrX.Gameplay.Resources;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceAirplaneBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private GameObject groundEnemy;
        [SerializeField] private GameObject flyingEnemy;
        [SerializeField] private GameObject captainEnemy;
        /// <summary>Enemies of this beat notice the heroine only within this distance, so a safe start is not defeated by the 14-unit definition range.</summary>
        public const float EncounterAggroRange = 9f;
        private VerticalSliceSession2D session;
        private SliceNarrativeComposition2D narrative;
        private GameObject vipLoungeRoot;
        private SpriteRenderer rumiSeatRenderer;
        private SpriteRenderer zoeyMiraTableRenderer;
        private readonly System.Collections.Generic.List<Sprite> ownedIntroSprites = new System.Collections.Generic.List<Sprite>(4);
        private Sprite[] vipIntroFrames;
        private float beatClock;
        private bool heroinesStoodUpFromTable;
        private bool wasOpeningDialoguePresenting;

        public override string Title
        {
            get
            {
                if (!heroinesStoodUpFromTable && beatClock < 3.1f)
                {
                    return "Fundo do Jato VIP: Comendo Ramyeon e Kimbap antes do Show! (Ande ou Ataque [Z/J] para levantar!)";
                }
                if (session != null && session.Manager != null && session.Manager.ActiveCharacter != null &&
                    session.Manager.ActiveCharacter.transform.position.x >= 32f)
                {
                    return "Porta da Cabine de Comando (Cockpit): Derrote o BOSS Capitão Dokkaebi!";
                }
                return "Corredor do Avião: Avance até a Cabine derrotando a Tripulação Demoníaca!";
            }
        }

        public override bool TryBegin(VerticalSliceSession2D owner)
        {
            if (owner == null || groundEnemy == null || flyingEnemy == null || captainEnemy == null ||
                captainEnemy.GetComponent<MinibossPhaseController2D>() == null ||
                captainEnemy.GetComponent<EnemyPatternController2D>() == null || !owner.TryLoadStage(stage)) return false;
            session = owner;
            narrative = owner.GetComponent<SliceNarrativeComposition2D>();
            IsComplete = false;
            beatClock = 0f;
            heroinesStoodUpFromTable = false;
            wasOpeningDialoguePresenting = false;

            BuildVipRearLoungeEatingIntro(owner);

            // Spawn the authored Stage 1 enemies + additional corridor waves leading to the Cockpit Boss at x = 42f!
            string[] crewRoles = { "FlightCrew_MagentaStewardess", "FlightCrew_BlueCyclops", "FlightCrew_CaptainDokkaebi" };
            for (int i = 0; i < stage.EnemySpawnCount; i++)
            {
                if (!owner.TrySpawnEnemy(groundEnemy, stage.GetEnemySpawn(i), out EnemyAgent2D spawned)) return false;
                if (spawned != null)
                {
                    spawned.TrySetAggroRangeLimit(EncounterAggroRange);
                    spawned.gameObject.name = crewRoles[i % crewRoles.Length] + "_" + i;
                    var vis = spawned.GetComponent<EnemyPrototypeVisual2D>();
                    if (vis != null) vis.RefreshCasteAssignment();
                }
            }

            // Additional corridor guards + Front Cockpit Captain Dokkaebi Boss at the right end of the plane!
            // Spawns keep >= 5 units from every checkpoint (x=14, x=33) and outside the opening viewport.
            Vector2[] extraCabinSpawns =
            {
                new Vector2(26.0f, 0.8f),
                new Vector2(39.5f, 0.8f),
                new Vector2(42.5f, 0.8f) // Cockpit Boss Captain Dokkaebi right in front of the Cockpit Door!
            };
            string[] extraRoles =
            {
                "FlightCrew_BlueCyclops_Corridor1",
                "FlightCrew_MagentaStewardess_CockpitGuard",
                "BOSS_Cockpit_CaptainDokkaebi"
            };
            for (int i = 0; i < extraCabinSpawns.Length; i++)
            {
                GameObject prefab = i == extraCabinSpawns.Length - 1 ? captainEnemy : groundEnemy;
                if (!owner.TrySpawnEnemy(prefab, extraCabinSpawns[i], out EnemyAgent2D extra) || extra == null) return false;
                extra.gameObject.name = extraRoles[i];
                extra.TrySetAggroRangeLimit(EncounterAggroRange);
                var vis = extra.GetComponent<EnemyPrototypeVisual2D>();
                if (vis != null) vis.RefreshCasteAssignment();
            }

            if (owner.Manager != null && owner.Manager.ActiveCharacter != null)
            {
                var prop = owner.Manager.ActiveCharacter.GetComponent<TemporaryPropWeapon2D>();
                if (prop == null) prop = owner.Manager.ActiveCharacter.gameObject.AddComponent<TemporaryPropWeapon2D>();
                prop.EquipProp(StagePropType2D.AirlineCoffeePot, 5);
            }
            if (!owner.Aerial.Configure(owner.Stage, owner.Manager, flyingEnemy) || !owner.Aerial.TrySpawn()) return false;
            for (int i = 0; i < owner.Aerial.Count; i++) owner.Aerial.GetAt(i)?.TrySetAggroRangeLimit(EncounterAggroRange);
            return owner.TryBeginPerformance();
        }

        private void BuildVipRearLoungeEatingIntro(VerticalSliceSession2D owner)
        {
            CleanupVipLounge();
            if (owner.Stage == null || owner.Stage.RuntimeRoot == null) return;

            vipIntroFrames = new Sprite[4];
            int loadedCount = 0;
            for (int i = 0; i < 4; i++)
            {
                Sprite s = LoadIntroSprite("Art/Props/VipLoungeIntro/" + i);
                vipIntroFrames[i] = s;
                if (s != null) loadedCount++;
            }
            if (loadedCount < 4) return;

            vipLoungeRoot = new GameObject("VIP_RearLounge_EatingScene");
            vipLoungeRoot.transform.SetParent(owner.Stage.RuntimeRoot.transform, false);
            Vector2 startPos = stage != null ? stage.StartPosition : new Vector2(1f, 0.8f);
            vipLoungeRoot.transform.localPosition = new Vector3(startPos.x, startPos.y, 0f);

            // Rumi starting seated in her luxury airplane VIP chair slurping ramyeon (world X ~ 1.35, well inside the left camera margin)
            var rumiSeatGo = new GameObject("VIP_Seat_Rumi_EatingRamyeon");
            rumiSeatGo.transform.SetParent(vipLoungeRoot.transform, false);
            rumiSeatGo.transform.localPosition = new Vector3(0.35f, 0.15f, 0f);
            rumiSeatRenderer = rumiSeatGo.AddComponent<SpriteRenderer>();
            rumiSeatRenderer.sprite = vipIntroFrames[0];
            rumiSeatRenderer.sortingOrder = 16;
            float seatScale = vipIntroFrames[0].bounds.size.y > 0.01f ? (2.30f / vipIntroFrames[0].bounds.size.y) : 0.45f;
            rumiSeatGo.transform.localScale = new Vector3(seatScale, seatScale, 1f);

            // Zoey & Mira seated at the VIP booth table feasting on kimbap, mandu & coffee pot (world X ~ 3.65, framed in the center-left viewport)
            var tableGo = new GameObject("VIP_Table_ZoeyAndMira_Feasting");
            tableGo.transform.SetParent(vipLoungeRoot.transform, false);
            tableGo.transform.localPosition = new Vector3(2.65f, 0.18f, 0f);
            zoeyMiraTableRenderer = tableGo.AddComponent<SpriteRenderer>();
            zoeyMiraTableRenderer.sprite = vipIntroFrames[1];
            zoeyMiraTableRenderer.sortingOrder = 11;
            float tableScale = vipIntroFrames[1].bounds.size.y > 0.01f ? (2.35f / vipIntroFrames[1].bounds.size.y) : 0.45f;
            tableGo.transform.localScale = new Vector3(tableScale, tableScale, 1f);

            // While Rumi is seated in the VIP chair eating ramyeon at the very start, hide the standing gameplay sprite
            owner.SetExternalCharacterArt(true);
        }

        private Sprite LoadIntroSprite(string path)
        {
            Sprite s = UnityEngine.Resources.Load<Sprite>(path);
            if (s != null) return s;
            Texture2D tex = UnityEngine.Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                ownedIntroSprites.Add(s);
                return s;
            }
            return null;
        }

        private bool IsOpeningDialoguePresenting()
        {
            if (narrative == null && session != null)
                narrative = session.GetComponent<SliceNarrativeComposition2D>();
            var presenter = narrative != null ? narrative.Presenter : null;
            return presenter != null && presenter.isActiveAndEnabled && presenter.IsPresenting;
        }

        public override void OnInput(SliceInputFrame frame)
        {
            if (IsOpeningDialoguePresenting()) return;
            if (!heroinesStoodUpFromTable && (frame.Move.sqrMagnitude > 0.05f || frame.Attack || frame.JumpPressed || frame.Dash))
            {
                TriggerStandUpFromVipTable();
            }
        }

        private void TriggerStandUpFromVipTable()
        {
            if (heroinesStoodUpFromTable) return;
            heroinesStoodUpFromTable = true;
            if (session != null) session.SetExternalCharacterArt(false);
            // Both images contain seated heroines, rather than reusable empty scenery.
            // Release the whole tableau when gameplay takes ownership of the characters.
            if (rumiSeatRenderer != null) rumiSeatRenderer.enabled = false;
            if (zoeyMiraTableRenderer != null) zoeyMiraTableRenderer.enabled = false;
        }

        public override void Tick(float deltaTime)
        {
            if (session == null || session.Manager.ActiveCharacter == null) return;

            bool dialogueActive = IsOpeningDialoguePresenting();
            if (dialogueActive)
            {
                wasOpeningDialoguePresenting = true;
                beatClock = 0f;
                if (!heroinesStoodUpFromTable && rumiSeatRenderer != null && vipIntroFrames != null && vipIntroFrames.Length >= 4)
                {
                    rumiSeatRenderer.sprite = vipIntroFrames[0];
                    rumiSeatRenderer.transform.localPosition = new Vector3(0.35f, 0.15f, 0f);
                }
                return;
            }

            if (wasOpeningDialoguePresenting)
            {
                wasOpeningDialoguePresenting = false;
                beatClock = 0f;
            }

            beatClock += Mathf.Max(0f, deltaTime);

            Vector2 startPos = stage != null ? stage.StartPosition : new Vector2(1f, 0.8f);
            float distFromStart = Mathf.Abs(session.Manager.ActiveCharacter.transform.position.x - startPos.x);

            if (!heroinesStoodUpFromTable)
            {
                if (distFromStart > 0.35f || beatClock >= 3.1f)
                {
                    TriggerStandUpFromVipTable();
                }
                else if (rumiSeatRenderer != null && vipIntroFrames != null && vipIntroFrames.Length >= 4)
                {
                    // Frame 0 (0..1.35s): Slurping ramyeon in VIP seat
                    // Frame 2 (1.35..2.25s): Alert! Standing up & tossing cup while grabbing Saingeom hilt
                    // Frame 3 (2.25..3.10s): Leaping forward slashing cyan-gold Honmoon arc into combat stance!
                    if (beatClock < 1.35f)
                    {
                        rumiSeatRenderer.sprite = vipIntroFrames[0];
                        rumiSeatRenderer.transform.localPosition = new Vector3(0.35f, 0.15f, 0f);
                    }
                    else if (beatClock < 2.25f)
                    {
                        rumiSeatRenderer.sprite = vipIntroFrames[2];
                        rumiSeatRenderer.transform.localPosition = new Vector3(0.46f, 0.22f, 0f);
                    }
                    else
                    {
                        rumiSeatRenderer.sprite = vipIntroFrames[3];
                        rumiSeatRenderer.transform.localPosition = new Vector3(0.62f, 0.28f, 0f);
                    }
                }
            }

            if (session.LivingEnemies > 0) return;
            for (int i = 0; i < session.Aerial.Count; i++) if (session.Aerial.GetAt(i) != null && session.Aerial.GetAt(i).Self.IsAlive) return;
            IsComplete = Vector2.Distance(session.Manager.ActiveCharacter.transform.position, session.Stage.EndPoint.position) <= 2f;
        }

        private void CleanupVipLounge()
        {
            if (session != null) session.SetExternalCharacterArt(false);
            if (vipLoungeRoot != null) Destroy(vipLoungeRoot);
            vipLoungeRoot = null;
            rumiSeatRenderer = null;
            zoeyMiraTableRenderer = null;
            for (int i = 0; i < ownedIntroSprites.Count; i++)
            {
                if (ownedIntroSprites[i] != null) Destroy(ownedIntroSprites[i]);
            }
            ownedIntroSprites.Clear();
        }

        public override void Exit()
        {
            CleanupVipLounge();
            base.Exit();
            session = null;
            narrative = null;
            wasOpeningDialoguePresenting = false;
        }
    }
}
