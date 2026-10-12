using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Honmoon;
using HuntrX.Gameplay.Resources;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FanHonmoonTransactionTests
    {
        private readonly List<Object> owned = new List<Object>();
        private float previousScale;
        [SetUp] public void SetUp() { previousScale = Time.timeScale; Time.timeScale = 1f; }
        [TearDown] public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear(); Time.timeScale = previousScale;
        }
        private GameObject NewObject(string name) { var go = new GameObject(name); owned.Add(go); return go; }
        private GameObject Spawn(string path)
        {
            GameObject prefab = UnityEngine.Resources.Load<GameObject>(path); Assert.That(prefab, Is.Not.Null, path);
            var go = Object.Instantiate(prefab); owned.Add(go);
            Rigidbody2D body = go.GetComponent<Rigidbody2D>();
            if (body != null) { body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeAll; }
            return go;
        }
        private static FieldInfo Field(object instance, string name) => instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private HonmoonController2D Resource()
        {
            var profile = ScriptableObject.CreateInstance<HonmoonDefinition>(); owned.Add(profile);
            var resource = NewObject("test Honmoon").AddComponent<HonmoonController2D>();
            Assert.That(resource.TryConfigure(profile), Is.True); return resource;
        }
        [Test] public void HonmoonCrisisUsesHysteresisAndDistinctGenerationTransitions()
        {
            var resource = Resource(); var transitions = new List<bool>(); resource.CrisisChanged += transitions.Add;
            Assert.That(resource.TrySetValue(20f), Is.True); Assert.That(resource.IsInCrisis, Is.True);
            Assert.That(resource.TrySetValue(30f), Is.True); Assert.That(resource.IsInCrisis, Is.True);
            Assert.That(resource.TrySetValue(40f), Is.True); Assert.That(resource.IsInCrisis, Is.False);
            Assert.That(resource.TrySetValue(25f), Is.True); Assert.That(resource.IsInCrisis, Is.False);
            Assert.That(resource.TrySetValue(20f), Is.True);
            CollectionAssert.AreEqual(new[] { true, false, true }, transitions);
            Assert.That(resource.CrisisGeneration, Is.EqualTo(3));
        }
        [Test] public void HonmoonCommitsBeforeEventsAndRejectsNestedMutation()
        {
            var resource = Resource(); var events = new List<string>();
            bool nestedAccepted = true, coherentSnapshot = false;
            resource.ValueChanged += (before, after) => {
                events.Add("value");
                coherentSnapshot = before == 100f && after == 20f && resource.Value == 20f && resource.Band == HonmoonBand.Critical && resource.IsInCrisis;
                nestedAccepted = resource.TrySetValue(90f);
            };
            resource.BandChanged += band => events.Add("band"); resource.CrisisChanged += crisis => events.Add("crisis");
            Assert.That(resource.TrySetValue(20f), Is.True);
            CollectionAssert.AreEqual(new[] { "value", "band", "crisis" }, events);
            Assert.That(coherentSnapshot, Is.True); Assert.That(nestedAccepted, Is.False); Assert.That(resource.Value, Is.EqualTo(20f));
            Assert.That(resource.IsPublishing, Is.False);
        }
        [Test] public void HonmoonBoundsAndNonfiniteRequestsProduceNoUnacceptedEvents()
        {
            var resource = Resource(); int changes = 0; resource.ValueChanged += (before, after) => changes++;
            Assert.That(resource.TrySetValue(float.NaN), Is.False); Assert.That(resource.TryChange(float.PositiveInfinity), Is.False);
            Assert.That(resource.TryChange(float.NegativeInfinity), Is.False); Assert.That(changes, Is.Zero);
            Assert.That(resource.TrySetValue(-1000f), Is.True); Assert.That(resource.Value, Is.Zero);
            Assert.That(resource.TrySetValue(-1f), Is.False); Assert.That(changes, Is.EqualTo(1));
            Assert.That(resource.TrySetValue(1000f), Is.True); Assert.That(resource.Value, Is.EqualTo(100f)); Assert.That(changes, Is.EqualTo(2));
        }
        [Test] public void FanSoulTransactionsClampAndRejectNestedRecovery()
        {
            var fan = Spawn("Fans/RescuableFan_Prototype").GetComponent<FanActor2D>();
            var order = new List<string>(); bool nestedAccepted = true;
            fan.SoulChanged += (actor, before, after) => { order.Add("soul"); nestedAccepted = fan.TryRecover(1f); };
            fan.StateChanged += (actor, before, after) => order.Add("state");
            Assert.That(fan.TryDrain(fan.Definition.MaximumSoul * 2f), Is.True);
            Assert.That(fan.Soul, Is.Zero); Assert.That(fan.State, Is.EqualTo(FanState.Corrupted)); Assert.That(nestedAccepted, Is.False);
            CollectionAssert.AreEqual(new[] { "soul", "state" }, order);
            Assert.That(fan.TryDrain(1f), Is.False);
            Assert.That(fan.TryRecover(fan.Definition.MaximumSoul * 2f), Is.True);
            Assert.That(fan.Soul, Is.EqualTo(fan.Definition.MaximumSoul)); Assert.That(fan.State, Is.EqualTo(FanState.Normal));
        }
        [UnityTest] public IEnumerator CompletedRescueCreditsHonmoonOnceDespiteDuplicateEnrollmentAndNestedRestart()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var go = Spawn("Fans/RescuableFan_Prototype"); var fan = go.GetComponent<FanActor2D>(); var rescue = go.GetComponent<FanRescueController2D>();
            var definition = Object.Instantiate((FanRescueDefinition)Field(rescue, "definition").GetValue(rescue)); owned.Add(definition);
            Field(definition, "recoveryDuration").SetValue(definition, .02f); Field(rescue, "definition").SetValue(rescue, definition);
            var resource = Resource(); Assert.That(resource.TrySetValue(50f), Is.True);
            var adapterObject = NewObject("rescue credit adapter"); adapterObject.SetActive(false);
            var adapter = adapterObject.AddComponent<HonmoonFanAdapter2D>();
            Field(adapter, "honmoon").SetValue(adapter, resource); Field(adapter, "rescue").SetValue(adapter, rescue);
            Field(adapter, "additionalRescues").SetValue(adapter, new[] { rescue, rescue }); adapterObject.SetActive(true);
            int rewards = 0, rewardSum = 0; bool nestedRestart = true;
            rescue.RescueCompleted += (actor, amount) => { rewards++; rewardSum += amount; actor.TryDrain(1f); nestedRestart = rescue.TryBeginRescue(owner); };
            Assert.That(fan.TryDrain(fan.Definition.MaximumSoul * .5f), Is.True); Assert.That(rescue.TryBeginRescue(owner), Is.True);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (rescue.IsRecovering && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(rescue.IsRecovering, Is.False, "Rescue did not complete within runtime deadline.");
            Assert.That(rewards, Is.EqualTo(1)); Assert.That(rewardSum, Is.EqualTo(definition.RewardAmount));
            Assert.That(rescue.CompletedRescueCount, Is.EqualTo(1)); Assert.That(nestedRestart, Is.False); Assert.That(fan.IsProtected, Is.False);
            Assert.That(resource.Value, Is.EqualTo(60f)); yield return null;
            Assert.That(rewards, Is.EqualTo(1)); Assert.That(resource.Value, Is.EqualTo(60f));
        }
        [Test] public void FoodBenefitExceptionRestoresReservationAndPermitsNextConsumption()
        {
            var inventory = NewObject("exception rollback inventory").AddComponent<FoodInventory2D>();
            var pickup = Spawn("Food/Ramen_Pickup").GetComponent<FoodPickup2D>(); var food = (FoodDefinition)Field(pickup, "definition").GetValue(pickup);
            Assert.That(inventory.TryAdd(food), Is.True); int publications = 0; inventory.QuantityChanged += (item, count) => publications++;
            MethodInfo consume = typeof(FoodInventory2D).GetMethod("TryConsume", BindingFlags.Instance | BindingFlags.NonPublic);
            Func<bool> throwing = () => throw new InvalidOperationException("deliberate failed benefit");
            var error = Assert.Throws<TargetInvocationException>(() => consume.Invoke(inventory, new object[] { food, throwing }));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(inventory.Count(food), Is.EqualTo(1)); Assert.That(inventory.TotalCount, Is.EqualTo(1)); Assert.That(inventory.IsBusy, Is.False); Assert.That(publications, Is.Zero);
            Assert.That((bool)consume.Invoke(inventory, new object[] { food, new Func<bool>(() => true) }), Is.True);
            Assert.That(inventory.TotalCount, Is.Zero); Assert.That(publications, Is.EqualTo(1));
        }
    }
}
