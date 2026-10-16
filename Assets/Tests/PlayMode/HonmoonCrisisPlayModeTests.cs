using System;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Honmoon;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class HonmoonCrisisPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<Object> disposables = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                {
                    Object.DestroyImmediate(disposables[i]);
                }
            }
            disposables.Clear();
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Field '{name}' must exist on '{target.GetType().Name}'.");
            field.SetValue(target, value);
        }

        private HonmoonCrisisDefinition CreateCrisisDefinition(
            float spawnMultiplier = 0.5f,
            int extraEnemies = 4,
            float recovery = 30f,
            float duration = 2.5f)
        {
            var def = ScriptableObject.CreateInstance<HonmoonCrisisDefinition>();
            Set(def, "spawnIntervalMultiplier", spawnMultiplier);
            Set(def, "extraConcurrentEnemies", extraEnemies);
            Set(def, "unionRecoveryValue", recovery);
            Set(def, "unionProtectionDuration", duration);
            disposables.Add(def);
            return def;
        }

        private HonmoonDefinition CreateHonmoonDefinition(float initial = 100f, float enter = 20f, float exit = 40f)
        {
            var def = ScriptableObject.CreateInstance<HonmoonDefinition>();
            Set(def, "initialValue", initial);
            Set(def, "crisisEnterValue", enter);
            Set(def, "crisisExitValue", exit);
            disposables.Add(def);
            return def;
        }

        private CharacterManager2D CreatePlayerSlot(out GameObject slotGo)
        {
            var slotPrefab = Resources.Load<GameObject>("Characters/HuntrXPlayerSlot_Prototype");
            Assert.That(slotPrefab, Is.Not.Null, "HuntrXPlayerSlot_Prototype prefab must exist.");
            slotGo = Object.Instantiate(slotPrefab);
            disposables.Add(slotGo);

            var manager = slotGo.GetComponent<CharacterManager2D>();
            var flow = slotGo.GetComponent<CheckpointAttemptFlow2D>();
            var rumiPrefab = Resources.Load<GameObject>("Characters/Rumi_Prototype");
            Assert.That(manager.Configure(rumiPrefab, flow, slotGo.transform.Find("StageStart"), "start"), Is.True);
            Assert.That(manager.TrySpawn(), Is.True);
            return manager;
        }

        [Test]
        public void HonmoonCrisisDefinition_ValidatesBoundaries()
        {
            HonmoonCrisisDefinition valid = CreateCrisisDefinition();
            Assert.That(valid.IsValid(out string error), Is.True, error);

            HonmoonCrisisDefinition invalidMultiplier = CreateCrisisDefinition(spawnMultiplier: 0f);
            Assert.That(invalidMultiplier.IsValid(out _), Is.False);

            HonmoonCrisisDefinition excessiveMultiplier = CreateCrisisDefinition(spawnMultiplier: 1.5f);
            Assert.That(excessiveMultiplier.IsValid(out _), Is.False);

            HonmoonCrisisDefinition excessiveEnemies = CreateCrisisDefinition(extraEnemies: 100);
            Assert.That(excessiveEnemies.IsValid(out _), Is.False);

            HonmoonCrisisDefinition zeroRecovery = CreateCrisisDefinition(recovery: 0f);
            Assert.That(zeroRecovery.IsValid(out _), Is.False);

            HonmoonCrisisDefinition zeroDuration = CreateCrisisDefinition(duration: 0f);
            Assert.That(zeroDuration.IsValid(out _), Is.False);
        }

        [Test]
        public void HonmoonCrisisController_AppliesIntensityWhenEnteringCrisis_AndClearsOnExit()
        {
            GameObject host = new GameObject("CrisisHost");
            disposables.Add(host);

            var honmoon = host.AddComponent<HonmoonController2D>();
            Assert.That(honmoon.TryConfigure(CreateHonmoonDefinition()), Is.True);

            var horde = host.AddComponent<HordeController2D>();
            var union = host.AddComponent<CrisisTeamUnion2D>();
            var crisisController = host.AddComponent<HonmoonCrisisController2D>();

            HonmoonCrisisDefinition crisisDef = CreateCrisisDefinition(spawnMultiplier: 0.5f, extraEnemies: 6);
            CharacterManager2D player = CreatePlayerSlot(out _);
            Assert.That(union.TryConfigure(crisisDef, new[] { player }), Is.True);
            Assert.That(crisisController.TryConfigure(honmoon, crisisDef, horde, union), Is.True);

            Assert.That(crisisController.IsIntensifying, Is.False);

            var intensityEvents = new List<bool>();
            crisisController.IntensityChanged += intensityEvents.Add;

            // Enter crisis (below 20f)
            Assert.That(honmoon.TrySetValue(15f), Is.True);
            Assert.That(honmoon.IsInCrisis, Is.True);
            Assert.That(crisisController.IsIntensifying, Is.True);

            // Exit crisis (at or above 40f)
            Assert.That(honmoon.TrySetValue(45f), Is.True);
            Assert.That(honmoon.IsInCrisis, Is.False);
            Assert.That(crisisController.IsIntensifying, Is.False);

            CollectionAssert.AreEqual(new[] { true, false }, intensityEvents);
        }

        [Test]
        public void HonmoonCrisisController_TryUnite_RecoversResourceAndProtectsParticipants()
        {
            GameObject host = new GameObject("CrisisHost");
            disposables.Add(host);

            var honmoon = host.AddComponent<HonmoonController2D>();
            Assert.That(honmoon.TryConfigure(CreateHonmoonDefinition()), Is.True);

            var horde = host.AddComponent<HordeController2D>();
            var union = host.AddComponent<CrisisTeamUnion2D>();
            var crisisController = host.AddComponent<HonmoonCrisisController2D>();

            HonmoonCrisisDefinition crisisDef = CreateCrisisDefinition(recovery: 35f);
            CharacterManager2D player = CreatePlayerSlot(out _);
            Assert.That(union.TryConfigure(crisisDef, new[] { player }), Is.True);
            Assert.That(crisisController.TryConfigure(honmoon, crisisDef, horde, union), Is.True);

            // Cannot unite outside of crisis
            Assert.That(crisisController.CanUnite, Is.False);
            Assert.That(crisisController.TryUnite(), Is.False);

            // Drop into crisis
            Assert.That(honmoon.TrySetValue(15f), Is.True);
            Assert.That(crisisController.CanUnite, Is.True);

            bool teamUnitedFired = false;
            crisisController.TeamUnited += () => teamUnitedFired = true;

            bool united = crisisController.TryUnite();
            Assert.That(united, Is.True);
            Assert.That(teamUnitedFired, Is.True);
            Assert.That(union.IsActive, Is.True);
            Assert.That(union.Covers(player.ActiveCharacter), Is.True);

            // Resource recovered by 35f: 15f + 35f = 50f
            Assert.That(honmoon.Value, Is.EqualTo(50f));
            Assert.That(honmoon.IsInCrisis, Is.False);

            // Second union attempt in the same crisis cycle is rejected
            Assert.That(crisisController.TryUnite(), Is.False);
        }

        [Test]
        public void CrisisTeamUnion_ProtectsOnlyValidParticipants()
        {
            GameObject host = new GameObject("UnionHost");
            disposables.Add(host);

            var union = host.AddComponent<CrisisTeamUnion2D>();
            HonmoonCrisisDefinition crisisDef = CreateCrisisDefinition();
            CharacterManager2D player = CreatePlayerSlot(out _);
            Assert.That(union.TryConfigure(crisisDef, new[] { player }), Is.True);

            Assert.That(union.TryExecuteTeamUnion(), Is.True);
            Assert.That(union.IsActive, Is.True);
            Assert.That(union.Covers(player.ActiveCharacter), Is.True);

            // Foreign receiver without HuntrX participation is not covered
            GameObject foreignGo = new GameObject("ForeignActor");
            disposables.Add(foreignGo);
            var foreignReceiver = foreignGo.AddComponent<DamageReceiver2D>();
            foreignReceiver.Configure(100f, CombatFaction2D.Demon, 1f, null, 1f, 1f);
            Assert.That(union.Covers(foreignReceiver), Is.False);
        }
    }
}
