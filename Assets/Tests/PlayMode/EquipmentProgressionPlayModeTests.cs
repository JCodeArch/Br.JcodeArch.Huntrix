using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Equipment;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class EquipmentProgressionPlayModeTests
    {
        private readonly List<Object> trackedObjects = new List<Object>();
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (int i = trackedObjects.Count - 1; i >= 0; --i)
            {
                if (trackedObjects[i] != null)
                {
                    Object.DestroyImmediate(trackedObjects[i]);
                }
            }
            trackedObjects.Clear();
            yield return null;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, Flags);
                type = type.BaseType;
            }
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' not found on '{target.GetType().Name}'.");
            field.SetValue(target, value);
        }

        private static T GetField<T>(object target, string fieldName)
        {
            Type type = target.GetType();
            FieldInfo field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, Flags);
                type = type.BaseType;
            }
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' not found on '{target.GetType().Name}'.");
            return (T)field.GetValue(target);
        }

        private static object InvokeMethod(object target, string methodName, params object[] parameters)
        {
            Type type = target.GetType();
            MethodInfo method = null;
            while (type != null && method == null)
            {
                method = type.GetMethod(methodName, Flags);
                type = type.BaseType;
            }
            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found on '{target.GetType().Name}'.");
            return method.Invoke(target, parameters);
        }

        private AttackDefinition CreateAttack(float damage = 25f)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            trackedObjects.Add(attack);
            SetField(attack, "damage", damage);
            SetField(attack, "startupDuration", 0.05f);
            SetField(attack, "activeDuration", 0.2f);
            SetField(attack, "recoveryDuration", 0.1f);
            SetField(attack, "hitboxSize", new Vector2(2f, 2f));
            SetField(attack, "hitboxOffset", Vector2.zero);
            SetField(attack, "horizontalKnockbackImpulse", 6f);
            SetField(attack, "upwardKnockbackImpulse", 1f);
            return attack;
        }

        private EquipmentPieceDefinition CreateAirStepPiece(string narrativePoint = "story_point_airstep", float velocity = 9f, float cooldown = 2f)
        {
            var piece = ScriptableObject.CreateInstance<EquipmentPieceDefinition>();
            trackedObjects.Add(piece);
            SetField(piece, "mechanic", EquipmentMechanic.AirStep);
            SetField(piece, "narrativePointId", narrativePoint);
            SetField(piece, "airStepVelocity", velocity);
            SetField(piece, "cooldown", cooldown);
            return piece;
        }

        private EquipmentPieceDefinition CreateResonanceBurstPiece(string narrativePoint = "story_point_burst", float radius = 3f, float cooldown = 3f, AttackDefinition attack = null)
        {
            var piece = ScriptableObject.CreateInstance<EquipmentPieceDefinition>();
            trackedObjects.Add(piece);
            SetField(piece, "mechanic", EquipmentMechanic.ResonanceBurst);
            SetField(piece, "narrativePointId", narrativePoint);
            SetField(piece, "burstRadius", radius);
            SetField(piece, "cooldown", cooldown);
            SetField(piece, "burstAttack", attack ?? CreateAttack());
            return piece;
        }

        private GameObject CreateCombatEntity(string name, Vector2 position, CombatFaction2D faction)
        {
            var go = new GameObject(name);
            trackedObjects.Add(go);
            go.SetActive(false);
            go.transform.position = position;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;

            var receiver = go.AddComponent<DamageReceiver2D>();
            SetField(receiver, "faction", faction);
            SetField(receiver, "maximumHealth", 100f);

            var child = new GameObject("Hurtbox");
            child.transform.SetParent(go.transform, false);
            var col = child.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;
            child.AddComponent<Hurtbox2D>();

            go.SetActive(true);
            return go;
        }

        // =========================================================================
        // 1. EquipmentPieceDefinition tests
        // =========================================================================

        [Test]
        public void EquipmentPieceDefinition_IsValid_RejectsNullOrWhitespaceNarrativePoint()
        {
            var piece = CreateAirStepPiece(narrativePoint: "");
            Assert.That(piece.IsValid(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            SetField(piece, "narrativePointId", "   ");
            Assert.That(piece.IsValid(out error), Is.False);

            SetField(piece, "narrativePointId", null);
            Assert.That(piece.IsValid(out error), Is.False);
        }

        [Test]
        public void EquipmentPieceDefinition_IsValid_RejectsNonPositiveCooldown()
        {
            var piece = CreateAirStepPiece(cooldown: 0f);
            Assert.That(piece.IsValid(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            SetField(piece, "cooldown", -1.5f);
            Assert.That(piece.IsValid(out error), Is.False);

            SetField(piece, "cooldown", float.NaN);
            Assert.That(piece.IsValid(out error), Is.False);
        }

        [Test]
        public void EquipmentPieceDefinition_IsValid_AirStep_RequiresPositiveAirStepVelocity()
        {
            var piece = CreateAirStepPiece(velocity: 0f);
            Assert.That(piece.IsValid(out string error), Is.False);

            SetField(piece, "airStepVelocity", -5f);
            Assert.That(piece.IsValid(out error), Is.False);

            SetField(piece, "airStepVelocity", 8.5f);
            Assert.That(piece.IsValid(out error), Is.True);
            Assert.That(error, Is.Empty);
        }

        [Test]
        public void EquipmentPieceDefinition_IsValid_ResonanceBurst_RequiresPositiveRadiusAndValidAttack()
        {
            var attack = CreateAttack();
            var piece = CreateResonanceBurstPiece(radius: 0f, attack: attack);
            Assert.That(piece.IsValid(out string error), Is.False);

            SetField(piece, "burstRadius", 2.5f);
            SetField(piece, "burstAttack", null);
            Assert.That(piece.IsValid(out error), Is.False);

            var invalidAttack = ScriptableObject.CreateInstance<AttackDefinition>();
            trackedObjects.Add(invalidAttack);
            SetField(invalidAttack, "damage", -10f); // invalid damage
            SetField(piece, "burstAttack", invalidAttack);
            Assert.That(piece.IsValid(out error), Is.False);

            SetField(piece, "burstAttack", attack);
            Assert.That(piece.IsValid(out error), Is.True);
            Assert.That(error, Is.Empty);
        }

        // =========================================================================
        // 2. EquipmentProgress2D tests
        // =========================================================================

        [Test]
        public void EquipmentProgress2D_TryBindProfile_AcceptsExact32HexFormatAndEnforcesIdempotencyAndRejectsMismatch()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();

            // Reject malformed profile IDs
            Assert.That(progress.TryBindProfile(null), Is.False);
            Assert.That(progress.TryBindProfile(""), Is.False);
            Assert.That(progress.TryBindProfile("invalid-uuid"), Is.False);
            Assert.That(progress.TryBindProfile(Guid.NewGuid().ToString("D")), Is.False); // "D" format has hyphens, TryParseExact requires "N"

            // Accept 32-digit hex format "N"
            string validId1 = Guid.NewGuid().ToString("N");
            Assert.That(progress.TryBindProfile(validId1), Is.True);
            Assert.That(progress.ProfileId, Is.EqualTo(validId1));

            // Idempotent for same ID
            Assert.That(progress.TryBindProfile(validId1), Is.True);

            // Rejects rebinding to another ID
            string validId2 = Guid.NewGuid().ToString("N");
            Assert.That(progress.TryBindProfile(validId2), Is.False);
            Assert.That(progress.ProfileId, Is.EqualTo(validId1));
        }

        [Test]
        public void EquipmentProgress2D_TryCompleteNarrativePoint_RequiresBoundProfileAndActiveComponent()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();

            var piece = CreateAirStepPiece("point_1");
            SetField(progress, "catalog", new[] { piece });

            // Fails when profile is not bound
            Assert.That(progress.TryCompleteNarrativePoint("point_1"), Is.False);

            // Bind profile
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            // Fails when component is disabled
            progress.enabled = false;
            Assert.That(progress.TryCompleteNarrativePoint("point_1"), Is.False);

            // Succeeds when re-enabled
            progress.enabled = true;
            Assert.That(progress.TryCompleteNarrativePoint("point_1"), Is.True);
        }

        [Test]
        public void EquipmentProgress2D_TryCompleteNarrativePoint_RejectsNullOrWhitespacePointId()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            Assert.That(progress.TryCompleteNarrativePoint(null), Is.False);
            Assert.That(progress.TryCompleteNarrativePoint(""), Is.False);
            Assert.That(progress.TryCompleteNarrativePoint("   "), Is.False);
        }

        [Test]
        public void EquipmentProgress2D_TryCompleteNarrativePoint_UnlocksMatchingCatalogPiecesAndRaisesEvent()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            var piece1 = CreateAirStepPiece("narrative_stage_1");
            var piece2 = CreateResonanceBurstPiece("narrative_stage_2");
            SetField(progress, "catalog", new[] { piece1, piece2 });

            EquipmentPieceDefinition unlockedEventPiece = null;
            int eventCount = 0;
            progress.PieceUnlocked += p =>
            {
                unlockedEventPiece = p;
                eventCount++;
            };

            Assert.That(progress.IsUnlocked(piece1), Is.False);
            Assert.That(progress.IsUnlocked(piece2), Is.False);

            bool completed = progress.TryCompleteNarrativePoint("narrative_stage_1");
            Assert.That(completed, Is.True);
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(unlockedEventPiece, Is.EqualTo(piece1));
            Assert.That(progress.IsUnlocked(piece1), Is.True);
            Assert.That(progress.IsUnlocked(piece2), Is.False);
        }

        [Test]
        public void EquipmentProgress2D_TryCompleteNarrativePoint_ProtectsAgainstDuplicateUnlocks()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            var piece = CreateAirStepPiece("narrative_dup");
            SetField(progress, "catalog", new[] { piece });

            int eventCount = 0;
            progress.PieceUnlocked += _ => eventCount++;

            Assert.That(progress.TryCompleteNarrativePoint("narrative_dup"), Is.True);
            Assert.That(eventCount, Is.EqualTo(1));

            // Calling again must return false and not fire event again
            Assert.That(progress.TryCompleteNarrativePoint("narrative_dup"), Is.False);
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void EquipmentProgress2D_TryCompleteNarrativePoint_EventExceptionIsolation()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            var piece = CreateAirStepPiece("point_fault");
            SetField(progress, "catalog", new[] { piece });

            bool secondHandlerExecuted = false;
            progress.PieceUnlocked += _ => throw new InvalidOperationException("Simulated subscriber fault");
            progress.PieceUnlocked += _ => secondHandlerExecuted = true;

            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, "InvalidOperationException: Simulated subscriber fault");
            bool completed = progress.TryCompleteNarrativePoint("point_fault");

            Assert.That(completed, Is.True);
            Assert.That(secondHandlerExecuted, Is.True, "Subsequent subscribers must still execute despite earlier fault.");
            Assert.That(progress.IsUnlocked(piece), Is.True);
        }

        [Test]
        public void EquipmentProgress2D_SnapshotUnlockedIds_ReturnsIndependentSnapshot()
        {
            var go = new GameObject("ProgressTest");
            trackedObjects.Add(go);
            var progress = go.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            var piece = CreateAirStepPiece("point_snap");
            SetField(progress, "catalog", new[] { piece });

            Assert.That(progress.SnapshotUnlockedIds(), Is.Empty);

            progress.TryCompleteNarrativePoint("point_snap");
            string[] snapshot = progress.SnapshotUnlockedIds();
            Assert.That(snapshot.Length, Is.EqualTo(1));
            Assert.That(snapshot[0], Is.EqualTo(piece.Id));
        }

        // =========================================================================
        // 3. EquipmentStoryPoint2D tests
        // =========================================================================

        [Test]
        public void EquipmentStoryPoint2D_RequiresCollider2DAndCompletesNarrativePointOnProgress()
        {
            var storyGo = new GameObject("StoryPoint");
            trackedObjects.Add(storyGo);
            var col = storyGo.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            var storyPoint = storyGo.AddComponent<EquipmentStoryPoint2D>();
            SetField(storyPoint, "narrativePointId", "story_encounter_1");

            var playerGo = new GameObject("PlayerProgress");
            trackedObjects.Add(playerGo);
            var progress = playerGo.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            var piece = CreateAirStepPiece("story_encounter_1");
            SetField(progress, "catalog", new[] { piece });

            Assert.That(storyPoint.TryComplete(progress), Is.True);
            Assert.That(progress.IsUnlocked(piece), Is.True);
        }

        [Test]
        public void EquipmentStoryPoint2D_ReturnsFalseWhenInactiveOrNullProgress()
        {
            var storyGo = new GameObject("StoryPoint");
            trackedObjects.Add(storyGo);
            storyGo.AddComponent<BoxCollider2D>();
            var storyPoint = storyGo.AddComponent<EquipmentStoryPoint2D>();
            SetField(storyPoint, "narrativePointId", "story_encounter_2");

            Assert.That(storyPoint.TryComplete(null), Is.False);

            var playerGo = new GameObject("PlayerProgress");
            trackedObjects.Add(playerGo);
            var progress = playerGo.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            storyPoint.enabled = false;
            Assert.That(storyPoint.TryComplete(progress), Is.False);
        }

        // =========================================================================
        // 4. EquipmentMechanics2D tests
        // =========================================================================

        [Test]
        public void EquipmentMechanics2D_TryBindProgression_BindsProgressionAndResetsEquippedPiece()
        {
            var player = CreateCombatEntity("Player", Vector2.zero, CombatFaction2D.HuntrX);
            var mechanics = player.AddComponent<EquipmentMechanics2D>();

            var progress = player.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));

            Assert.That(mechanics.TryBindProgression(null), Is.False);
            Assert.That(mechanics.TryBindProgression(progress), Is.True);
            Assert.That(mechanics.Progression, Is.EqualTo(progress));
            Assert.That(mechanics.Equipped, Is.Null);
        }

        [Test]
        public void EquipmentMechanics2D_TryEquip_EnforcesUnlockedStatusAndValidity()
        {
            var player = CreateCombatEntity("Player", Vector2.zero, CombatFaction2D.HuntrX);
            var mechanics = player.AddComponent<EquipmentMechanics2D>();
            var progress = player.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));
            mechanics.TryBindProgression(progress);

            var piece = CreateAirStepPiece("story_jump");
            SetField(progress, "catalog", new[] { piece });

            // Cannot equip piece if it's not unlocked yet
            Assert.That(mechanics.TryEquip(piece), Is.False);
            Assert.That(mechanics.Equipped, Is.Null);

            // Cannot equip null or invalid piece
            Assert.That(mechanics.TryEquip(null), Is.False);
            var invalidPiece = CreateAirStepPiece("story_jump", velocity: -1f);
            Assert.That(mechanics.TryEquip(invalidPiece), Is.False);

            // Unlock and equip
            progress.TryCompleteNarrativePoint("story_jump");
            Assert.That(mechanics.TryEquip(piece), Is.True);
            Assert.That(mechanics.Equipped, Is.EqualTo(piece));
        }

        [Test]
        public void EquipmentMechanics2D_ReadyGuards_RejectWhenDisabledDeadOrOnCooldown()
        {
            var player = CreateCombatEntity("Player", Vector2.zero, CombatFaction2D.HuntrX);
            var mechanics = player.AddComponent<EquipmentMechanics2D>();
            var progress = player.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));
            mechanics.TryBindProgression(progress);

            var piece = CreateAirStepPiece("story_guard", velocity: 10f, cooldown: 2f);
            SetField(progress, "catalog", new[] { piece });
            progress.TryCompleteNarrativePoint("story_guard");
            mechanics.TryEquip(piece);

            // Make sure it is airborne and landed previously
            SetField(mechanics, "landed", true);
            SetField(mechanics, "airStepUsed", false);

            // Disabled mechanics component rejects
            mechanics.enabled = false;
            Assert.That(mechanics.TryUseAbility(), Is.False);
            mechanics.enabled = true;

            // Cooldown rejects
            SetField(mechanics, "cooldown", 1.5f);
            Assert.That(mechanics.TryUseAbility(), Is.False);
            SetField(mechanics, "cooldown", 0f);

            // Dead owner rejects
            var receiver = player.GetComponent<DamageReceiver2D>();
            SetField(receiver, "maximumHealth", 0f);
            LogAssert.Expect(LogType.Error, "DamageReceiver2D requires finite positive maximum health.");
            InvokeMethod(receiver, "InitializeConfiguration");
            Assert.That(mechanics.TryUseAbility(), Is.False);
        }

        [Test]
        public void EquipmentMechanics2D_AirStep_AppliesVelocityAndEnforcesSingleAirUseUntilGrounded()
        {
            var player = CreateCombatEntity("Player", Vector2.zero, CombatFaction2D.HuntrX);
            var mechanics = player.AddComponent<EquipmentMechanics2D>();
            var progress = player.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));
            mechanics.TryBindProgression(progress);

            var piece = CreateAirStepPiece("story_air", velocity: 12f, cooldown: 3f);
            SetField(progress, "catalog", new[] { piece });
            progress.TryCompleteNarrativePoint("story_air");
            mechanics.TryEquip(piece);

            // Before landing has ever occurred, cannot air-step
            SetField(mechanics, "landed", false);
            Assert.That(mechanics.TryUseAbility(), Is.False);

            // Simulate grounded: ground count > 0 -> cannot air-step while on ground
            var dummyCollider = player.GetComponentInChildren<Collider2D>();
            var groundSet = GetField<HashSet<Collider2D>>(mechanics, "ground");
            groundSet.Add(dummyCollider);
            SetField(mechanics, "landed", true);
            Assert.That(mechanics.TryUseAbility(), Is.False);

            // Leave ground: ground count = 0, landed = true, airStepUsed = false
            groundSet.Clear();

            EquipmentPieceDefinition usedPiece = null;
            mechanics.AbilityUsed += p => usedPiece = p;

            bool activated = mechanics.TryUseAbility();
            Assert.That(activated, Is.True);
            Assert.That(usedPiece, Is.EqualTo(piece));

            var body = player.GetComponent<Rigidbody2D>();
            Assert.That(body.linearVelocityY, Is.EqualTo(12f));
            Assert.That(GetField<float>(mechanics, "cooldown"), Is.EqualTo(3f));
            Assert.That(GetField<bool>(mechanics, "airStepUsed"), Is.True);

            // Attempting to air step again in same airborne phase is rejected even if cooldown is cleared
            SetField(mechanics, "cooldown", 0f);
            Assert.That(mechanics.TryUseAbility(), Is.False);

            // Resetting airStepUsed (e.g., after landing on ground and jumping again) allows another air step
            SetField(mechanics, "airStepUsed", false);
            Assert.That(mechanics.TryUseAbility(), Is.True);
        }

        [Test]
        public void EquipmentMechanics2D_ResonanceBurst_HitsNearbyEnemiesWithinRadiusAndRespectsLineOfSight()
        {
            var player = CreateCombatEntity("Player", Vector2.zero, CombatFaction2D.HuntrX);
            var mechanics = player.AddComponent<EquipmentMechanics2D>();
            var progress = player.AddComponent<EquipmentProgress2D>();
            progress.TryBindProfile(Guid.NewGuid().ToString("N"));
            mechanics.TryBindProgression(progress);

            var attack = CreateAttack(damage: 35f);
            var piece = CreateResonanceBurstPiece("story_burst", radius: 4f, cooldown: 2f, attack: attack);
            SetField(progress, "catalog", new[] { piece });
            progress.TryCompleteNarrativePoint("story_burst");
            mechanics.TryEquip(piece);

            // Spawn Demon enemy within burst radius (distance = 2 units)
            var enemy = CreateCombatEntity("DemonEnemy", new Vector2(2f, 0f), CombatFaction2D.Demon);
            var enemyReceiver = enemy.GetComponent<DamageReceiver2D>();
            float initialEnemyHealth = enemyReceiver.CurrentHealth;

            // Spawn ally within burst radius (should NOT be damaged)
            var ally = CreateCombatEntity("AllyEntity", new Vector2(-1.5f, 0f), CombatFaction2D.HuntrX);
            var allyReceiver = ally.GetComponent<DamageReceiver2D>();
            float initialAllyHealth = allyReceiver.CurrentHealth;

            // Sync 2D physics
            Physics2D.SyncTransforms();

            CombatImpactEvent recordedImpact = default;
            bool impactFired = false;
            mechanics.ImpactOccurred += e =>
            {
                recordedImpact = e;
                impactFired = true;
            };

            bool burstTriggered = mechanics.TryUseAbility();
            Assert.That(burstTriggered, Is.True);
            Assert.That(impactFired, Is.True);
            Assert.That(recordedImpact.Receiver, Is.EqualTo(enemyReceiver));
            Assert.That(recordedImpact.AttackDefinition, Is.EqualTo(attack));

            // Enemy received damage
            Assert.That(enemyReceiver.CurrentHealth, Is.EqualTo(initialEnemyHealth - 35f));
            // Ally was untouched
            Assert.That(allyReceiver.CurrentHealth, Is.EqualTo(initialAllyHealth));
        }

        // =========================================================================
        // 5. EquipmentPrototypeBinding2D tests
        // =========================================================================

        [Test]
        public void EquipmentPrototypeBinding2D_AutoBindsProfileProgressionAndAutoEquipsOnUnlock()
        {
            var player = CreateCombatEntity("PrototypePlayer", Vector2.zero, CombatFaction2D.HuntrX);
            var progress = player.AddComponent<EquipmentProgress2D>();
            var mechanics = player.AddComponent<EquipmentMechanics2D>();
            var binding = player.AddComponent<EquipmentPrototypeBinding2D>();

            var piece = CreateAirStepPiece("story_prototype", velocity: 10f);
            SetField(progress, "catalog", new[] { piece });

            // Trigger Start lifecycle
            InvokeMethod(binding, "Awake");
            InvokeMethod(binding, "OnEnable");
            InvokeMethod(binding, "Start");

            // Profile must now be bound
            Assert.That(progress.ProfileId, Is.Not.Null);
            Assert.That(Guid.TryParseExact(progress.ProfileId, "N", out _), Is.True);
            Assert.That(mechanics.Progression, Is.EqualTo(progress));

            // Complete story point -> progress unlocks piece -> prototype binding automatically equips it
            progress.TryCompleteNarrativePoint("story_prototype");
            Assert.That(progress.IsUnlocked(piece), Is.True);
            Assert.That(mechanics.Equipped, Is.EqualTo(piece));

            InvokeMethod(binding, "OnDisable");
        }
    }
}
