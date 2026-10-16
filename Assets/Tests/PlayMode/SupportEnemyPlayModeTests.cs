using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Protection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SupportEnemyPlayModeTests
    {
        private static readonly BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<GameObject> instances = new List<GameObject>();

        private GameObject Spawn(string path)
        {
            var prefab = UnityEngine.Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instances.Add(instance);
            return instance;
        }

        private static T Field<T>(object instance, string name) =>
            (T)instance.GetType().GetField(name, NonPublicInstance).GetValue(instance);

        private static void SetProperty(object instance, string propName, object value)
        {
            var prop = instance.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(prop, Is.Not.Null, "Property not found: " + propName);
            prop.SetValue(instance, value);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in instances)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            instances.Clear();
        }

        [Test]
        public void AllThreeSupportEnemiesLoadAndHaveValidDefinitions()
        {
            string[] paths =
            {
                "Enemies/HealerDemon_Prototype",
                "Enemies/ProtectorDemon_Prototype",
                "Enemies/SoulDrainerDemon_Prototype"
            };

            SupportEnemyRole[] expectedRoles =
            {
                SupportEnemyRole.Healer,
                SupportEnemyRole.Protector,
                SupportEnemyRole.SoulDrainer
            };

            for (int i = 0; i < paths.Length; i++)
            {
                var go = Spawn(paths[i]);
                var support = go.GetComponent<SupportEnemyController2D>();
                var agent = go.GetComponent<EnemyAgent2D>();
                var receiver = go.GetComponent<DamageReceiver2D>();

                Assert.That(support, Is.Not.Null, paths[i]);
                Assert.That(agent, Is.Not.Null, paths[i]);
                Assert.That(receiver, Is.Not.Null, paths[i]);

                var def = Field<SupportEnemyDefinition>(support, "definition");
                Assert.That(def, Is.Not.Null, paths[i] + " definition");
                Assert.That(def.IsValid(out string error), Is.True, error);
                Assert.That(def.Role, Is.EqualTo(expectedRoles[i]));
                Assert.That(def.Radius, Is.GreaterThan(0f));
                Assert.That(def.Interval, Is.GreaterThan(0f));
            }
        }

        [UnityTest]
        public IEnumerator HealerDemonRestoresHealthToInjuredAllyWithinRadius()
        {
            var healerGo = Spawn("Enemies/HealerDemon_Prototype");
            var healer = healerGo.GetComponent<SupportEnemyController2D>();
            var def = Field<SupportEnemyDefinition>(healer, "definition");

            var allyGo = Spawn("Enemies/GroundDemon_Prototype");
            var allyReceiver = allyGo.GetComponent<DamageReceiver2D>();
            allyGo.transform.position = healerGo.transform.position + new Vector3(1f, 0f, 0f);

            yield return null;

            // Damage ally
            SetProperty(allyReceiver, "CurrentHealth", allyReceiver.MaximumHealth * 0.5f);
            float damagedHealth = allyReceiver.CurrentHealth;

            bool pulsed = healer.TryPulse();
            Assert.That(pulsed, Is.True);
            Assert.That(allyReceiver.CurrentHealth, Is.GreaterThan(damagedHealth));
            Assert.That(allyReceiver.CurrentHealth, Is.EqualTo(Mathf.Min(allyReceiver.MaximumHealth, damagedHealth + def.HealingAmount)));
        }

        [UnityTest]
        public IEnumerator ProtectorDemonGrantsExternalProtectionSourceToAllies()
        {
            var protectorGo = Spawn("Enemies/ProtectorDemon_Prototype");
            var protector = protectorGo.GetComponent<SupportEnemyController2D>();

            var allyGo = Spawn("Enemies/GroundDemon_Prototype");
            var allyReceiver = allyGo.GetComponent<DamageReceiver2D>();
            var allyProtection = allyGo.GetComponent<DamageProtection2D>();
            if (allyProtection == null)
            {
                allyProtection = allyGo.AddComponent<DamageProtection2D>();
            }
            allyGo.transform.position = protectorGo.transform.position + new Vector3(1f, 0f, 0f);

            yield return null;

            Assert.That(protector.Covers(allyReceiver), Is.False);

            bool pulsed = protector.TryPulse();
            Assert.That(pulsed, Is.True);
            Assert.That(protector.Covers(allyReceiver), Is.True);
            Assert.That(allyProtection.IsProtected, Is.True);
        }

        [UnityTest]
        public IEnumerator ProtectorDemonProtectionClearsOnDisable()
        {
            var protectorGo = Spawn("Enemies/ProtectorDemon_Prototype");
            var protector = protectorGo.GetComponent<SupportEnemyController2D>();

            var allyGo = Spawn("Enemies/GroundDemon_Prototype");
            var allyReceiver = allyGo.GetComponent<DamageReceiver2D>();
            var allyProtection = allyGo.GetComponent<DamageProtection2D>();
            if (allyProtection == null)
            {
                allyProtection = allyGo.AddComponent<DamageProtection2D>();
            }
            allyGo.transform.position = protectorGo.transform.position + new Vector3(1f, 0f, 0f);

            yield return null;

            Assert.That(protector.TryPulse(), Is.True);
            Assert.That(protector.Covers(allyReceiver), Is.True);
            Assert.That(allyProtection.IsProtected, Is.True);

            // Disabling protector clears all protection
            protectorGo.SetActive(false);
            Assert.That(protector.Covers(allyReceiver), Is.False);
            Assert.That(allyProtection.IsProtected, Is.False);
        }

        [UnityTest]
        public IEnumerator SoulDrainerDemonAppliesHitAndFiresDrainAcceptedEvent()
        {
            var drainerGo = Spawn("Enemies/SoulDrainerDemon_Prototype");
            var drainer = drainerGo.GetComponent<SupportEnemyController2D>();
            var agent = drainerGo.GetComponent<EnemyAgent2D>();

            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var rumiReceiver = rumiGo.GetComponent<DamageReceiver2D>();
            rumiGo.transform.position = drainerGo.transform.position + new Vector3(1f, 0f, 0f);

            yield return null;

            Assert.That(agent.TrySetTarget(rumiReceiver), Is.True);

            int drainEvents = 0;
            DamageReceiver2D drainedTarget = null;
            float actualLost = 0f;

            drainer.DrainAccepted += (target, lost) =>
            {
                drainEvents++;
                drainedTarget = target;
                actualLost = lost;
            };

            float healthBefore = rumiReceiver.CurrentHealth;
            bool pulsed = drainer.TryPulse();
            Assert.That(pulsed, Is.True);

            Assert.That(drainEvents, Is.EqualTo(1));
            Assert.That(drainedTarget, Is.SameAs(rumiReceiver));
            Assert.That(actualLost, Is.GreaterThan(0f));
            Assert.That(rumiReceiver.CurrentHealth, Is.EqualTo(healthBefore - actualLost));
        }
    }
}
