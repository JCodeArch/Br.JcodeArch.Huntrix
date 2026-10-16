using System.Collections;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Fans;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FanStatePlayModeTests
    {
        private sealed class TestFanProtectionSource : MonoBehaviour, IFanProtectionSource2D
        {
            public bool ShouldCover = true;
            public bool Covers(FanActor2D fan) => ShouldCover;
        }

        private readonly List<UnityEngine.Object> disposables = new List<UnityEngine.Object>();

        private GameObject Spawn(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, "Resource not found at: " + path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            disposables.Add(instance);
            return instance;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(disposables[i]);
                }
            }
            disposables.Clear();
        }

        [Test]
        public void RescuableFanPrototype_LoadsWithValidDefinitionAndInitializesNormalState()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();

            Assert.That(fan, Is.Not.Null);
            Assert.That(fan.IsConfigurationValid, Is.True);
            Assert.That(fan.Definition.MaximumSoul, Is.GreaterThan(0f));
            Assert.That(fan.Definition.CriticalFraction, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(fan.Soul, Is.EqualTo(fan.Definition.MaximumSoul));
            Assert.That(fan.State, Is.EqualTo(FanState.Normal));
            Assert.That(fan.IsProtected, Is.False);
        }

        [Test]
        public void TryDrain_TransitionsFromNormalToDrainedToCriticalToCorrupted()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            float max = fan.Definition.MaximumSoul;
            float critThreshold = max * fan.Definition.CriticalFraction;

            FanState recordedPreviousState = FanState.Normal;
            FanState recordedNextState = FanState.Normal;
            int stateChangeCount = 0;
            fan.StateChanged += (actor, prev, next) =>
            {
                recordedPreviousState = prev;
                recordedNextState = next;
                stateChangeCount++;
            };

            bool drain1 = fan.TryDrain(max * 0.2f);
            Assert.That(drain1, Is.True);
            Assert.That(fan.State, Is.EqualTo(FanState.Drained));
            Assert.That(recordedPreviousState, Is.EqualTo(FanState.Normal));
            Assert.That(recordedNextState, Is.EqualTo(FanState.Drained));
            Assert.That(stateChangeCount, Is.EqualTo(1));

            float toCritical = (fan.Soul - critThreshold) + 1f;
            bool drain2 = fan.TryDrain(toCritical);
            Assert.That(drain2, Is.True);
            Assert.That(fan.State, Is.EqualTo(FanState.Critical));
            Assert.That(recordedNextState, Is.EqualTo(FanState.Critical));

            bool drain3 = fan.TryDrain(fan.Soul + 50f);
            Assert.That(drain3, Is.True);
            Assert.That(fan.Soul, Is.EqualTo(0f));
            Assert.That(fan.State, Is.EqualTo(FanState.Corrupted));
            Assert.That(recordedNextState, Is.EqualTo(FanState.Corrupted));

            bool drainEmpty = fan.TryDrain(10f);
            Assert.That(drainEmpty, Is.False);
            Assert.That(fan.Soul, Is.EqualTo(0f));
        }

        [Test]
        public void TryRecover_RestoresSoulAndTransitionsBackToNormal()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            float max = fan.Definition.MaximumSoul;

            fan.TryDrain(max);
            Assert.That(fan.State, Is.EqualTo(FanState.Corrupted));

            bool rec1 = fan.TryRecover(max * 0.2f);
            Assert.That(rec1, Is.True);
            Assert.That(fan.State, Is.EqualTo(FanState.Critical));

            bool rec2 = fan.TryRecover(max * 0.5f);
            Assert.That(rec2, Is.True);
            Assert.That(fan.State, Is.EqualTo(FanState.Drained));

            bool rec3 = fan.TryRecover(max);
            Assert.That(rec3, Is.True);
            Assert.That(fan.Soul, Is.EqualTo(max));
            Assert.That(fan.State, Is.EqualTo(FanState.Normal));

            bool recOver = fan.TryRecover(10f);
            Assert.That(recOver, Is.False);
            Assert.That(fan.Soul, Is.EqualTo(max));
        }

        [Test]
        public void ProtectionSource_PreventsDrainWhenRegistered()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            float initialSoul = fan.Soul;

            var protectorGo = new GameObject("ProtectorSource");
            var protector = protectorGo.AddComponent<TestFanProtectionSource>();
            disposables.Add(protectorGo);

            bool registered = fan.RegisterProtection(protector);
            Assert.That(registered, Is.True);
            Assert.That(fan.IsProtected, Is.True);

            bool drainBlocked = fan.TryDrain(30f);
            Assert.That(drainBlocked, Is.False);
            Assert.That(fan.Soul, Is.EqualTo(initialSoul));

            fan.UnregisterProtection(protector);
            Assert.That(fan.IsProtected, Is.False);

            bool drainAllowed = fan.TryDrain(30f);
            Assert.That(drainAllowed, Is.True);
            Assert.That(fan.Soul, Is.LessThan(initialSoul));
        }

        [Test]
        public void ContentIntensity_AdaptsPresentationAndPublishesEvent()
        {
            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();

            FanContentIntensity receivedIntensity = FanContentIntensity.Gentle;
            int eventCount = 0;
            fan.PresentationChanged += intensity =>
            {
                receivedIntensity = intensity;
                eventCount++;
            };

            Assert.That(fan.ContentIntensity, Is.EqualTo(FanContentIntensity.Gentle));

            bool setStandard = fan.TrySetContentIntensity(FanContentIntensity.Standard);
            Assert.That(setStandard, Is.True);
            Assert.That(fan.ContentIntensity, Is.EqualTo(FanContentIntensity.Standard));
            Assert.That(receivedIntensity, Is.EqualTo(FanContentIntensity.Standard));
            Assert.That(eventCount, Is.EqualTo(1));

            bool setSame = fan.TrySetContentIntensity(FanContentIntensity.Standard);
            Assert.That(setSame, Is.False);
            Assert.That(eventCount, Is.EqualTo(1));

            bool setIntense = fan.TrySetContentIntensity(FanContentIntensity.Intense);
            Assert.That(setIntense, Is.True);
            Assert.That(fan.ContentIntensity, Is.EqualTo(FanContentIntensity.Intense));
            Assert.That(receivedIntensity, Is.EqualTo(FanContentIntensity.Intense));
            Assert.That(eventCount, Is.EqualTo(2));
        }
    }
}
