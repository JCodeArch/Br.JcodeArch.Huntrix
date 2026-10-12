using System;
using System.Collections.Generic;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Special;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AcceptedCombatNotificationTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();
        [TearDown] public void TearDown()
        { for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]); owned.Clear(); }
        private DamageReceiver2D Actor(string path, float x)
        {
            var prefab = UnityEngine.Resources.Load<GameObject>(path); Assert.That(prefab, Is.Not.Null, path);
            var go = Object.Instantiate(prefab, new Vector3(x, 0f, 0f), Quaternion.identity); owned.Add(go);
            var body = go.GetComponent<Rigidbody2D>(); body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeAll;
            var receiver = go.GetComponent<DamageReceiver2D>(); Assert.That(receiver.IsConfigurationValid, Is.True); return receiver;
        }
        private static AttackDefinition LethalAttack()
        {
            var prefab = UnityEngine.Resources.Load<GameObject>("Special/HuntrXSpecial_Prototype"); Assert.That(prefab, Is.Not.Null);
            return prefab.GetComponent<HuntrXSpecialController2D>().Definition.CombinedAttack;
        }
        [Test] public void LethalContactPublishesCommittedDamageBeforeDeathUsingCapturedReceiverSubscriptions()
        {
            var source = Actor("Characters/Rumi_Prototype", 0f); var target = Actor("Enemies/GroundDemon_Prototype", 1f);
            var hurtbox = target.GetComponentInChildren<Hurtbox2D>(); var attack = LethalAttack();
            var order = new List<string>(); bool coherentHealth = false, identity = false;
            Action<CombatImpactEvent> received = hit => { order.Add("received"); identity = hit.Attacker == source && hit.Receiver == target && hit.AttackDefinition == attack; };
            target.DamageReceived += received;
            source.DamageDealt += hit => { order.Add("dealt"); coherentHealth = target.CurrentHealth == 0f && !target.IsAlive; target.DamageReceived -= received; };
            target.Died += actor => order.Add("died");
            Assert.That(hurtbox.TryReceiveHit(attack, source, 1f, out var receiver), Is.True);
            Assert.That(receiver, Is.SameAs(target)); Assert.That(coherentHealth, Is.True); Assert.That(identity, Is.True);
            CollectionAssert.AreEqual(new[] { "dealt", "received", "died" }, order);
            Assert.That(hurtbox.TryReceiveHit(attack, source, 1f, out _), Is.False);
            Assert.That(order.Count, Is.EqualTo(3), "Dead receivers must not republish accepted damage or death.");
        }
        [Test] public void RejectedFriendlyAndInvalidDirectionContactsPublishNoAcceptedDamage()
        {
            var source = Actor("Characters/Rumi_Prototype", 0f); var ally = Actor("Characters/Mira_Prototype", 1f);
            var enemy = Actor("Enemies/GroundDemon_Prototype", 2f); int dealt = 0, received = 0;
            source.DamageDealt += hit => dealt++; ally.DamageReceived += hit => received++; enemy.DamageReceived += hit => received++;
            Assert.That(ally.GetComponentInChildren<Hurtbox2D>().TryReceiveHit(LethalAttack(), source, 1f, out _), Is.False);
            Assert.That(enemy.GetComponentInChildren<Hurtbox2D>().TryReceiveHit(LethalAttack(), source, 0f, out _), Is.False);
            Assert.That(dealt, Is.Zero); Assert.That(received, Is.Zero);
            Assert.That(ally.CurrentHealth, Is.EqualTo(ally.MaximumHealth)); Assert.That(enemy.CurrentHealth, Is.EqualTo(enemy.MaximumHealth));
        }
    }
}
