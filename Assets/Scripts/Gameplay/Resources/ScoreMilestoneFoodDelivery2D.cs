using System;
using HuntrX.Data;
using HuntrX.Gameplay.Audio;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.FanEncounters;
using HuntrX.Gameplay.Fans;
using UnityEngine;

namespace HuntrX.Gameplay.Resources
{
    /// <summary>
    /// Implements the KPop Demon Hunters "Carb-Loading Before the Show" rule:
    /// Delivers Korean food dishes at score milestones (1,000 / 2,500 / 5,000 / 10,000 pts)
    /// that refill varying amounts of the Energy Tank, charging Mega Man X style Sub-Tanks on overflow,
    /// and applies film-signature martial arts combo bonuses (Rumi Kick, Zoey Leg Chokehold, Mira Coffee Pot Smash).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScoreMilestoneFoodDelivery2D : MonoBehaviour
    {
        private CharacterManager2D manager;
        private FoodInventory2D inventory;
        private FoodDefinition ramenDefinition;
        private FanEncounterController2D fanEncounter;
        private DamageReceiver2D boundActor;
        private AttackController2D boundAttack;

        private int nextDrinkMilestone = 1000;
        private int nextOnigiriMilestone = 2500;
        private int nextGyozaMilestone = 5000;
        private int nextRamyeonMilestone = 10000;
        private int consecutiveHits;
        private float comboDecayTimer;

        public int ArcadeScore { get; private set; }
        public float SubTankReservePercent { get; private set; }
        public string LastDeliveredFoodLabel { get; private set; } = "Café Gelado (+15% Tank @ 1.000 pts)";
        public string LastSignatureMoveLabel { get; private set; } = "Z-Saber / Saingeom Ready";
        public float DeliveryBannerTimer { get; private set; }
        public float SignatureMoveBannerTimer { get; private set; }

        public int NextMilestoneTarget
        {
            get
            {
                int a = Mathf.Min(nextDrinkMilestone, nextOnigiriMilestone);
                int b = Mathf.Min(nextGyozaMilestone, nextRamyeonMilestone);
                return Mathf.Min(a, b);
            }
        }

        public bool Configure(CharacterManager2D slot, FoodInventory2D foodInventory, FoodDefinition ramen, FanEncounterController2D fans)
        {
            if (slot == null || foodInventory == null) return false;
            Unbind();
            manager = slot;
            inventory = foodInventory;
            ramenDefinition = ramen;
            fanEncounter = fans;
            ResetMilestones();
            if (isActiveAndEnabled) Bind();
            return true;
        }

        public void ResetMilestones()
        {
            ArcadeScore = 0;
            SubTankReservePercent = 0f;
            nextDrinkMilestone = 1000;
            nextOnigiriMilestone = 2500;
            nextGyozaMilestone = 5000;
            nextRamyeonMilestone = 10000;
            consecutiveHits = 0;
            comboDecayTimer = 0f;
            DeliveryBannerTimer = 0f;
            SignatureMoveBannerTimer = 0f;
            LastDeliveredFoodLabel = "Próx: Café Gelado @ 1.000 pts";
        }

        private void OnEnable()
        {
            if (manager != null) Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (DeliveryBannerTimer > 0f) DeliveryBannerTimer = Mathf.Max(0f, DeliveryBannerTimer - dt);
            if (SignatureMoveBannerTimer > 0f) SignatureMoveBannerTimer = Mathf.Max(0f, SignatureMoveBannerTimer - dt);
            if (comboDecayTimer > 0f)
            {
                comboDecayTimer -= Time.deltaTime;
                if (comboDecayTimer <= 0f) consecutiveHits = 0;
            }
        }

        private void Bind()
        {
            manager.ActiveCharacterChanged -= HandleActorChanged;
            manager.ActiveCharacterChanged += HandleActorChanged;
            if (fanEncounter != null)
            {
                fanEncounter.RescueCompleted -= HandleFanRescued;
                fanEncounter.RescueCompleted += HandleFanRescued;
            }
            HandleActorChanged(null, manager.ActiveCharacter);
        }

        private void Unbind()
        {
            if (manager != null) manager.ActiveCharacterChanged -= HandleActorChanged;
            if (fanEncounter != null) fanEncounter.RescueCompleted -= HandleFanRescued;
            DetachActor();
        }

        private void DetachActor()
        {
            if (boundActor != null) boundActor.DamageDealt -= HandleDamageDealt;
            boundActor = null;
            boundAttack = null;
        }

        private void HandleActorChanged(DamageReceiver2D previous, DamageReceiver2D current)
        {
            DetachActor();
            boundActor = current;
            if (boundActor != null)
            {
                boundAttack = boundActor.GetComponent<AttackController2D>();
                if (boundActor.GetComponent<TemporaryPropWeapon2D>() == null)
                {
                    boundActor.gameObject.AddComponent<TemporaryPropWeapon2D>();
                }
                boundActor.DamageDealt += HandleDamageDealt;
            }
        }

        private void HandleFanRescued(FanActor2D fan, int reward)
        {
            AddScoreAndCheckFoodMilestones(1200);
        }

        private void HandleDamageDealt(CombatImpactEvent impact)
        {
            if (boundActor == null || !boundActor.IsAlive) return;
            consecutiveHits = Mathf.Min(99, consecutiveHits + 1);
            comboDecayTimer = 3.0f;

            int step = boundAttack != null ? boundAttack.CurrentComboStepIndex : (consecutiveHits - 1) % 3;
            string heroId = manager != null && manager.ActivePrefab != null ? manager.ActivePrefab.name : boundActor.name;
            Rigidbody2D actorBody = boundActor.GetComponent<Rigidbody2D>();
            bool isAirborne = actorBody != null && Mathf.Abs(actorBody.linearVelocity.y) > 0.25f;

            int baseScore = 350 + (consecutiveHits * 50);

            TemporaryPropWeapon2D propWeapon = boundActor.GetComponent<TemporaryPropWeapon2D>();
            if (propWeapon != null && propWeapon.HasEquippedProp && propWeapon.TryConsumeStrike(out StagePropType2D usedProp, out int remUses))
            {
                string propName = TemporaryPropWeapon2D.ResolveDisplayName(usedProp);
                LastSignatureMoveLabel = $"{propName} EQUIPADA (CLANG! {remUses}x RESTANTES)!";
                SignatureMoveBannerTimer = 2.2f;
                baseScore += 550;
                if (impact.Receiver != null && impact.Receiver.IsAlive)
                {
                    Rigidbody2D targetBody = impact.Receiver.GetComponent<Rigidbody2D>();
                    float facing = boundAttack != null ? boundAttack.LastFacingDirection : 1f;
                    if (targetBody != null) targetBody.linearVelocity = new Vector2(facing * 5.2f, 3.0f);
                }
            }
            else if (isAirborne && heroId.Contains("Rumi"))
            {
                float facing = boundAttack != null ? boundAttack.LastFacingDirection : 1f;
                if (actorBody != null) actorBody.linearVelocity = new Vector2(facing * 7.2f, -10.5f);
                LastSignatureMoveLabel = "RUMI: CHUTE AEREO SOLA VERMELHA (DIVE KICK)!";
                SignatureMoveBannerTimer = 2.4f;
                baseScore += 500;
            }
            else if (step >= 2)
            {
                ApplyFilmSignatureMartialArtsBonus(heroId, impact.Receiver);
                baseScore += 450;
            }
            else if (heroId.Contains("Mira") && step == 0)
            {
                LastSignatureMoveLabel = "MIRA: RAJADA DE SOCOS DE RUA (BRAWLER)!";
                SignatureMoveBannerTimer = 1.6f;
                baseScore += 200;
            }
            else if (heroId.Contains("Zoey") && step == 1)
            {
                LastSignatureMoveLabel = "ZOEY: ARREMESSO TRIPLO DE ADAGAS SHINKAL!";
                SignatureMoveBannerTimer = 1.4f;
            }
            else if (step == 1)
            {
                LastSignatureMoveLabel = "RUMI: CORTE DUPLO SAINGEOM!";
                SignatureMoveBannerTimer = 1.4f;
            }

            AddScoreAndCheckFoodMilestones(baseScore);
        }

        private void ApplyFilmSignatureMartialArtsBonus(string heroId, DamageReceiver2D target)
        {
            Rigidbody2D targetBody = target != null ? target.GetComponent<Rigidbody2D>() : null;
            Rigidbody2D actorBody = boundActor != null ? boundActor.GetComponent<Rigidbody2D>() : null;
            float facing = boundAttack != null ? boundAttack.LastFacingDirection : 1f;

            if (heroId.Contains("Mira"))
            {
                // Mira's Capcom Spinning-Blade Shoryuken Launcher!
                LastSignatureMoveLabel = "MIRA: SHORYUKEN LAMINA GIRATORIA GOKDO (UPPERCUT)!";
                SignatureMoveBannerTimer = 2.4f;
                if (actorBody != null)
                {
                    actorBody.linearVelocity = new Vector2(facing * 3.8f, 8.2f);
                }
                if (targetBody != null && target != null && target.IsAlive)
                {
                    targetBody.linearVelocity = new Vector2(facing * 3.5f, 8.8f);
                }
            }
            else if (heroId.Contains("Zoey"))
            {
                // Zoey's Iconic Flying Leg-Scissor Chokehold Takedown!
                LastSignatureMoveLabel = "ZOEY: MATA-LEÃO COM AS PERNAS (SLAM)!";
                SignatureMoveBannerTimer = 2.4f;
                if (targetBody != null && target != null && target.IsAlive)
                {
                    targetBody.linearVelocity = new Vector2(facing * 4.2f, -6.5f);
                }
            }
            else
            {
                // Rumi's Iconic High Roundhouse Taekwondo Kick!
                LastSignatureMoveLabel = "RUMI: CHUTE GIRATÓRIO TAEKWONDO (LAUNCH)!";
                SignatureMoveBannerTimer = 2.4f;
                if (targetBody != null && target != null && target.IsAlive)
                {
                    targetBody.linearVelocity = new Vector2(facing * 4.8f, 6.2f);
                }
            }
        }

        public void AddScoreAndCheckFoodMilestones(int deltaPoints)
        {
            if (deltaPoints <= 0) return;
            ArcadeScore = Mathf.Min(9999999, ArcadeScore + deltaPoints);

            // Check highest tier first so player gets full feedback
            while (ArcadeScore >= nextRamyeonMilestone)
            {
                nextRamyeonMilestone += 10000;
                DeliverKoreanFood("CUP RAMYEON PRÉ-SHOW (+100% TANQUE + MARMITA VIP!)", 100f, true);
                EquipBonusStageProp(StagePropType2D.VipGoldenMicStand, 5);
            }
            while (ArcadeScore >= nextGyozaMilestone)
            {
                nextGyozaMilestone += 5000;
                DeliverKoreanFood("MANDU / GYOZA GRELHADO (+50% TANQUE DE ENERGIA!)", 50f, false);
                EquipBonusStageProp(StagePropType2D.MyeongdongBoilingKettle, 5);
            }
            while (ArcadeScore >= nextOnigiriMilestone)
            {
                nextOnigiriMilestone += 2500;
                DeliverKoreanFood("KIMBAP / ONIGIRI DE BACKSTAGE (+30% TANQUE!)", 30f, false);
                EquipBonusStageProp(StagePropType2D.AirlineCoffeePot, 5);
            }
            while (ArcadeScore >= nextDrinkMilestone)
            {
                nextDrinkMilestone += 1500;
                DeliverKoreanFood("CAFÉ GELADO + GARRAFA DE CAFÉ EQUIPÁVEL (5x GOLPES)!", 15f, false);
                EquipBonusStageProp(StagePropType2D.AirlineCoffeePot, 5);
            }
        }

        private void EquipBonusStageProp(StagePropType2D propType, int uses)
        {
            DamageReceiver2D actor = manager != null ? manager.ActiveCharacter : boundActor;
            if (actor == null) return;
            TemporaryPropWeapon2D propWeapon = actor.GetComponent<TemporaryPropWeapon2D>();
            if (propWeapon == null) propWeapon = actor.gameObject.AddComponent<TemporaryPropWeapon2D>();
            propWeapon.EquipProp(propType, uses);
        }

        private void DeliverKoreanFood(string dishLabel, float healAmount, bool grantGuaranteedSubTank)
        {
            LastDeliveredFoodLabel = "DELIVERY: " + dishLabel;
            DeliveryBannerTimer = 3.2f;

            DamageReceiver2D actor = manager != null ? manager.ActiveCharacter : null;
            float overflow = healAmount;
            if (actor != null && actor.IsAlive)
            {
                float missing = Mathf.Max(0f, actor.MaximumHealth - actor.CurrentHealth);
                if (missing > 0.01f)
                {
                    actor.TryRestoreHealth(healAmount, actor);
                    overflow = Mathf.Max(0f, healAmount - missing);
                }
            }

            if (overflow > 0f)
            {
                SubTankReservePercent += overflow;
                while (SubTankReservePercent >= 100f)
                {
                    SubTankReservePercent -= 100f;
                    if (inventory != null && ramenDefinition != null)
                    {
                        inventory.TryAdd(ramenDefinition, 1);
                    }
                }
            }

            if (grantGuaranteedSubTank && inventory != null && ramenDefinition != null)
            {
                inventory.TryAdd(ramenDefinition, 1);
            }

            var audio = FindAnyObjectByType<GameAudioManager2D>();
            if (audio != null)
            {
                audio.TryPlay(grantGuaranteedSubTank ? AudioCue.SubTankFull : AudioCue.EnergyTankTick, transform.position);
            }
        }
    }
}
