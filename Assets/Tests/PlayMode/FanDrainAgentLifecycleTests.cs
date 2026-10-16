using System.Collections;
using System.Collections.Generic;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FanDrainAgentLifecycleTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();
        private float previousScale;

        [SetUp] public void SetUp() { previousScale = Time.timeScale; Time.timeScale = 1f; }
        [TearDown] public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
            Time.timeScale = previousScale;
        }

        private GameObject Spawn(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var instance = Object.Instantiate(prefab);
            owned.Add(instance);
            instance.transform.position = Vector3.zero;
            var body = instance.GetComponent<Rigidbody2D>();
            if (body != null) { body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeAll; }
            return instance;
        }

        private FanSoulDrainController2D Arrange(out EnemyAgent2D agent, out FanActor2D fan)
        {
            var registryObject = new GameObject("drain lifecycle registry");
            owned.Add(registryObject);
            var registry = registryObject.AddComponent<FanRegistry2D>();
            fan = Spawn("Fans/RescuableFan_Prototype").GetComponent<FanActor2D>();
            Assert.That(registry.Register(fan), Is.True);
            var demon = Spawn("Fans/FanSoulDrainer_Prototype");
            agent = demon.GetComponent<EnemyAgent2D>();
            var drain = demon.GetComponent<FanSoulDrainController2D>();
            Assert.That(agent.IsConfigurationValid, Is.True);
            Assert.That(drain.TryBindRegistry(registry), Is.True);
            return drain;
        }

        [UnityTest] public IEnumerator DisabledAgentCannotAcquireFan()
        {
            var drain = Arrange(out var agent, out var fan);
            float soul = fan.Soul;
            int pulses = 0;
            drain.FanDrainAccepted += (target, amount) => pulses++;
            agent.enabled = false;
            yield return null;
            yield return null;
            Assert.That(drain.IsTelegraphing, Is.False);
            Assert.That(fan.Soul, Is.EqualTo(soul));
            Assert.That(pulses, Is.Zero);
        }

        [UnityTest] public IEnumerator DisablingAgentCancelsPendingTelegraph()
        {
            var drain = Arrange(out var agent, out var fan);
            float soul = fan.Soul;
            int pulses = 0;
            drain.FanDrainAccepted += (target, amount) => pulses++;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (!drain.IsTelegraphing && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(drain.IsTelegraphing, Is.True, "Active agent did not acquire the fan.");
            agent.enabled = false;
            yield return null;
            Assert.That(drain.IsTelegraphing, Is.False);
            Assert.That(fan.Soul, Is.EqualTo(soul));
            Assert.That(pulses, Is.Zero);
        }

        [UnityTest] public IEnumerator EnabledAgentStillCommitsFanDrain()
        {
            var drain = Arrange(out var agent, out var fan);
            float soul = fan.Soul;
            int pulses = 0;
            float lost = 0f;
            FanActor2D accepted = null;
            drain.FanDrainAccepted += (target, amount) => { accepted = target; pulses++; lost += amount; };
            float deadline = Time.realtimeSinceStartup + 5f;
            while (pulses == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(agent.enabled, Is.True);
            Assert.That(pulses, Is.GreaterThan(0), "Active agent did not commit a pulse.");
            Assert.That(accepted, Is.SameAs(fan));
            Assert.That(lost, Is.GreaterThan(0f));
            Assert.That(fan.Soul, Is.EqualTo(soul - lost).Within(.001f));
        }
    }
}
