using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.FanEncounters;
using HuntrX.Gameplay.Fans;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FanEncounterPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject hostGo;
        private FanEncounterController2D controller;
        private GameObject validGroupPrefab;

        [SetUp]
        public void SetUp()
        {
            hostGo = new GameObject("FanEncounterHost");
            controller = hostGo.AddComponent<FanEncounterController2D>();
            validGroupPrefab = Resources.Load<GameObject>("Fans/RescuableFanGroup_Prototype");
        }

        [TearDown]
        public void TearDown()
        {
            if (hostGo != null)
            {
                Object.DestroyImmediate(hostGo);
            }
        }

        private static void Set(object target, string name, object value)
        {
            target.GetType().GetField(name, PrivateInstance).SetValue(target, value);
        }

        private static FanEncounterDefinition CreateProfile(GameObject groupPrefab, float initialSoulLoss = 15f)
        {
            var def = ScriptableObject.CreateInstance<FanEncounterDefinition>();
            Set(def, "groupPrefab", groupPrefab);
            Set(def, "initialSoulLoss", initialSoulLoss);
            return def;
        }

        [Test]
        public void DefinitionValidationRejectsNullOrScenePrefabOrNegativeLoss()
        {
            var defNull = CreateProfile(null, 15f);
            try
            {
                Assert.That(defNull.IsValid(out string error), Is.False);
                Assert.That(error, Is.Not.Empty);
            }
            finally
            {
                Object.DestroyImmediate(defNull);
            }

            var sceneGo = new GameObject("SceneFanGroup");
            var defScene = CreateProfile(sceneGo, 15f);
            try
            {
                Assert.That(defScene.IsValid(out string error), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(sceneGo);
                Object.DestroyImmediate(defScene);
            }

            var defNegative = CreateProfile(validGroupPrefab, -5f);
            try
            {
                Assert.That(defNegative.IsValid(out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(defNegative);
            }
        }

        [Test]
        public void DefinitionValidationAcceptsValidResourcesPrefab()
        {
            var def = CreateProfile(validGroupPrefab, 20f);
            try
            {
                Assert.That(def.IsValid(out string error), Is.True);
                Assert.That(error, Is.Empty);
                Assert.That(def.InitialSoulLoss, Is.EqualTo(20f));
                Assert.That(def.GroupPrefab, Is.SameAs(validGroupPrefab));
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void TryStartRejectsNullOrInvalidProfile()
        {
            Assert.That(controller.TryStart(null, Vector3.zero), Is.False);
            Assert.That(controller.RuntimeRoot, Is.Null);

            var invalid = CreateProfile(null);
            try
            {
                Assert.That(controller.TryStart(invalid, Vector3.zero), Is.False);
                Assert.That(controller.RuntimeRoot, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(invalid);
            }
        }

        [Test]
        public void TryStartRejectsNonFinitePosition()
        {
            var profile = CreateProfile(validGroupPrefab, 10f);
            try
            {
                Assert.That(controller.TryStart(profile, new Vector3(float.NaN, 0f, 0f)), Is.False);
                Assert.That(controller.TryStart(profile, new Vector3(0f, float.PositiveInfinity, 0f)), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void TryStartInstantiatesGroupAndInitializesDrainedFans()
        {
            var profile = CreateProfile(validGroupPrefab, 20f);
            try
            {
                bool started = controller.TryStart(profile, new Vector3(1f, 2f, 0f));
                Assert.That(started, Is.True);
                Assert.That(controller.RuntimeRoot, Is.Not.Null);
                Assert.That(controller.Registry, Is.Not.Null);
                Assert.That(controller.Registry.Count, Is.GreaterThan(0));
                Assert.That(controller.RescueSourceCount, Is.GreaterThan(0));
                Assert.That(controller.RescuedCount, Is.Zero);

                for (int i = 0; i < controller.Registry.Count; i++)
                {
                    var fan = controller.Registry.GetAt(i);
                    Assert.That(fan, Is.Not.Null);
                    Assert.That(fan.Soul, Is.LessThanOrEqualTo(fan.MaximumSoul - 20f + 0.01f));
                }
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void TryStartGuardsAgainstReentrantCallWithoutStopping()
        {
            var profile = CreateProfile(validGroupPrefab, 10f);
            try
            {
                Assert.That(controller.TryStart(profile, Vector3.zero), Is.True);
                Assert.That(controller.TryStart(profile, Vector3.one), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void StopAndOnDisableCleansUpRuntimeObjectsAndResetsRegistry()
        {
            var profile = CreateProfile(validGroupPrefab, 10f);
            try
            {
                controller.TryStart(profile, Vector3.zero);
                Assert.That(controller.RuntimeRoot, Is.Not.Null);

                controller.Stop();

                Assert.That(controller.RuntimeRoot, Is.Null);
                Assert.That(controller.Registry, Is.Null);
                Assert.That(controller.RescueSourceCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }
    }
}
