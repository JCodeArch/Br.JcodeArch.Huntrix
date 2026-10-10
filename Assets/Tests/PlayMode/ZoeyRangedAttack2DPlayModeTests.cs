using System.Collections;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class ZoeyRangedAttack2DPlayModeTests
    {
        private GameObject actor, target;
        private ZoeyRangedDefinition definition;
        private AttackDefinition attack;
        private ZoeyRangedAttack2D ranged;
        [UnitySetUp] public IEnumerator SetUp()
        {
            actor = Object.Instantiate(Resources.Load<GameObject>("Characters/Rumi_Prototype"), new Vector3(0f, 20f), Quaternion.identity);
            target = Object.Instantiate(Resources.Load<GameObject>("Targets/DemonTarget"), new Vector3(4f, 20f), Quaternion.identity);
            actor.GetComponent<Rigidbody2D>().gravityScale = 0f; target.GetComponent<Rigidbody2D>().gravityScale = 0f;
            actor.SetActive(false);
            ranged = actor.AddComponent<ZoeyRangedAttack2D>();
            attack = ScriptableObject.CreateInstance<AttackDefinition>();
            Set(attack, "damage", 10f); Set(attack, "activeDuration", .1f); Set(attack, "hitboxSize", Vector2.one); Set(attack, "horizontalKnockbackImpulse", 1f);
            definition = ScriptableObject.CreateInstance<ZoeyRangedDefinition>();
            Set(definition, "attack", attack); Set(definition, "range", 8f); Set(definition, "cooldown", .25f); Set(definition, "muzzleOffset", Vector2.zero);
            Assert.That(ranged.TrySetDefinition(definition, out var error), Is.True, error);
            actor.SetActive(true); yield return null; Physics2D.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Object.Destroy(actor); Object.Destroy(target); Object.Destroy(definition); Object.Destroy(attack); yield return null;
        }
        [UnityTest] public IEnumerator FiresOnceAndRejectsRepeatedShotDuringCooldown()
        {
            var health = target.GetComponent<DamageReceiver2D>(); float before = health.CurrentHealth;
            int impacts = 0; ranged.HitConfirmed += _ => impacts++;
            Assert.That(ranged.TryFire(Vector2.right), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(before - attack.Damage));
            Assert.That(impacts, Is.EqualTo(1)); Assert.That(ranged.TryFire(Vector2.right), Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(before - attack.Damage)); yield return null;
        }
        [UnityTest] public IEnumerator ExplicitDiagonalAimHitsAirborneTarget()
        {
            target.transform.position = actor.transform.position + new Vector3(3f, 3f);
            Physics2D.SyncTransforms(); var health = target.GetComponent<DamageReceiver2D>(); float before = health.CurrentHealth;
            Assert.That(ranged.TryFire(Vector2.one), Is.True); Assert.That(health.CurrentHealth, Is.EqualTo(before - attack.Damage)); yield return null;
        }
        [UnityTest] public IEnumerator InvalidAimAndDisabledControllerCannotFire()
        {
            Assert.That(ranged.TryFire(Vector2.zero), Is.False);
            Assert.That(ranged.TryFire(new Vector2(float.NaN, 1f)), Is.False);
            Assert.That(ranged.TryFire(new Vector2(1f, float.PositiveInfinity)), Is.False);
            ranged.enabled = false; Assert.That(ranged.TryFire(Vector2.right), Is.False); yield return null;
        }
        [UnityTest] public IEnumerator SolidBeforeTargetPreventsDamage()
        {
            var wall = new GameObject("ranged blocker"); wall.transform.position = actor.transform.position + Vector3.right * 2f;
            wall.AddComponent<BoxCollider2D>(); Physics2D.SyncTransforms();
            try
            {
                var health = target.GetComponent<DamageReceiver2D>(); float before = health.CurrentHealth;
                Assert.That(ranged.TryFire(Vector2.right), Is.True); Assert.That(health.CurrentHealth, Is.EqualTo(before)); yield return null;
            }
            finally { Object.Destroy(wall); }
        }
        [UnityTest] public IEnumerator FirstTargetStopsRayBeforeSecondTarget()
        {
            var behind = Object.Instantiate(Resources.Load<GameObject>("Targets/DemonTarget"), new Vector3(6f, 20f), Quaternion.identity);
            behind.GetComponent<Rigidbody2D>().gravityScale = 0f;
            try
            {
                yield return null; Physics2D.SyncTransforms();
                var first = target.GetComponent<DamageReceiver2D>(); var second = behind.GetComponent<DamageReceiver2D>();
                float before = second.CurrentHealth;
                Assert.That(ranged.TryFire(Vector2.right), Is.True);
                Assert.That(first.CurrentHealth, Is.LessThan(first.MaximumHealth)); Assert.That(second.CurrentHealth, Is.EqualTo(before));
            }
            finally { Object.Destroy(behind); }
        }
        [UnityTest] public IEnumerator SameFactionBlocksRayWithoutFriendlyDamage()
        {
            var ally = Object.Instantiate(Resources.Load<GameObject>("Characters/Rumi_Prototype"), new Vector3(2f, 20f), Quaternion.identity);
            ally.GetComponent<Rigidbody2D>().gravityScale = 0f;
            try
            {
                yield return null; Physics2D.SyncTransforms();
                var allyHealth = ally.GetComponent<DamageReceiver2D>(); var enemyHealth = target.GetComponent<DamageReceiver2D>();
                float beforeAlly = allyHealth.CurrentHealth, beforeEnemy = enemyHealth.CurrentHealth;
                Assert.That(ranged.TryFire(Vector2.right), Is.True);
                Assert.That(allyHealth.CurrentHealth, Is.EqualTo(beforeAlly)); Assert.That(enemyHealth.CurrentHealth, Is.EqualTo(beforeEnemy));
            }
            finally { Object.Destroy(ally); }
        }
        [UnityTest] public IEnumerator ParryConsumesWindowWithoutDamageOrImpact()
        {
            Object.Destroy(target);
            target = Object.Instantiate(Resources.Load<GameObject>("Characters/Rumi_Prototype"), new Vector3(4f, 20f), Quaternion.identity);
            target.GetComponent<Rigidbody2D>().gravityScale = 0f;
            Set(actor.GetComponent<DamageReceiver2D>(), "faction", CombatFaction2D.Demon);
            yield return null; Physics2D.SyncTransforms();
            var receiver = target.GetComponent<DamageReceiver2D>(); float before = receiver.CurrentHealth;
            var parry = target.GetComponent<ParryController2D>(); int parries = 0, impacts = 0;
            ranged.HitParried += _ => parries++; ranged.HitConfirmed += _ => impacts++;
            Assert.That(parry.TryStartParry(), Is.True); Assert.That(ranged.TryFire(Vector2.right), Is.True);
            Assert.That(receiver.CurrentHealth, Is.EqualTo(before)); Assert.That(parries, Is.EqualTo(1));
            Assert.That(impacts, Is.Zero); Assert.That(parry.IsWindowActive, Is.False);
        }
        [UnityTest] public IEnumerator MiraProtectionPreventsDamageWithoutImpact()
        {
            Object.Destroy(target);
            target = Object.Instantiate(Resources.Load<GameObject>("Characters/Mira_Prototype"), new Vector3(4f, 20f), Quaternion.identity);
            target.GetComponent<Rigidbody2D>().gravityScale = 0f;
            Set(actor.GetComponent<DamageReceiver2D>(), "faction", CombatFaction2D.Demon);
            yield return null; Physics2D.SyncTransforms();
            var receiver = target.GetComponent<DamageReceiver2D>(); float before = receiver.CurrentHealth; int impacts = 0;
            ranged.HitConfirmed += _ => impacts++;
            Assert.That(target.GetComponent<HuntrX.Gameplay.Protection.MiraProtectionField2D>().TryActivate(), Is.True);
            Assert.That(ranged.TryFire(Vector2.right), Is.True); Assert.That(receiver.CurrentHealth, Is.EqualTo(before)); Assert.That(impacts, Is.Zero);
        }
        [UnityTest] public IEnumerator ReentrantImpactCannotFireAnotherShot()
        {
            bool reentered = true; int impacts = 0;
            ranged.HitConfirmed += _ => { impacts++; reentered = ranged.TryFire(Vector2.right); };
            Assert.That(ranged.TryFire(Vector2.right), Is.True); Assert.That(reentered, Is.False); Assert.That(impacts, Is.EqualTo(1)); yield return null;
        }
        [UnityTest] public IEnumerator CooldownExpiresAndDisableResetsIt()
        {
            Assert.That(ranged.TryFire(Vector2.left), Is.True); Assert.That(ranged.CooldownRemaining, Is.GreaterThan(0f));
            yield return new WaitForSeconds(definition.Cooldown + .05f);
            Assert.That(ranged.TryFire(Vector2.left), Is.True); ranged.enabled = false;
            Assert.That(ranged.CooldownRemaining, Is.Zero); Assert.That(ranged.TryFire(Vector2.left), Is.False);
            ranged.enabled = true; Assert.That(ranged.TryFire(Vector2.left), Is.True);
        }
        [UnityTest] public IEnumerator DeadActorCannotFire()
        {
            Set(actor.GetComponent<DamageReceiver2D>(), "maximumHealth", 1f);
            Set(attack, "damage", 100000f);
            Assert.That(actor.GetComponent<DamageReceiver2D>().TryReceiveHit(attack, target.GetComponent<DamageReceiver2D>(), -1f), Is.True);
            Assert.That(actor.GetComponent<DamageReceiver2D>().IsAlive, Is.False); Assert.That(ranged.TryFire(Vector2.right), Is.False); yield return null;
        }
        [UnityTest] public IEnumerator AuthoredZoeyPrefabHasConfiguredRangedKit()
        {
            var prefab = Resources.Load<GameObject>("Characters/Zoey_Prototype");
            Assert.That(prefab, Is.Not.Null); var zoey = Object.Instantiate(prefab, new Vector3(0f, 30f), Quaternion.identity);
            try
            {
                yield return null; var controller = zoey.GetComponent<ZoeyRangedAttack2D>();
                Assert.That(controller, Is.Not.Null); Assert.That(controller.IsConfigurationValid, Is.True);
                Assert.That(controller.Definition.IsValid(out var error), Is.True, error);
                Assert.That(zoey.GetComponent<HuntrX.Gameplay.CharacterDefinitionApplier2D>().Definition.DisplayName, Is.EqualTo("Zoey"));
                Assert.That(controller.TryFire(Vector2.right), Is.True);
            }
            finally { Object.Destroy(zoey); }
        }
        [UnityTest] public IEnumerator UnrelatedTriggerDoesNotBlockTarget()
        {
            var trigger = new GameObject("unrelated trigger"); trigger.transform.position = new Vector3(2f, 20f);
            trigger.AddComponent<BoxCollider2D>().isTrigger = true;
            try
            {
                Physics2D.SyncTransforms(); var receiver = target.GetComponent<DamageReceiver2D>(); float before = receiver.CurrentHealth;
                Assert.That(ranged.TryFire(Vector2.right), Is.True); Assert.That(receiver.CurrentHealth, Is.EqualTo(before - attack.Damage)); yield return null;
            }
            finally { Object.Destroy(trigger); }
        }
        [UnityTest] public IEnumerator MultipleTargetCollidersStillApplyOneHit()
        {
            var extra = new GameObject("extra hurtbox collider"); extra.transform.SetParent(target.transform, false);
            extra.AddComponent<BoxCollider2D>().isTrigger = true;
            Physics2D.SyncTransforms(); var receiver = target.GetComponent<DamageReceiver2D>(); float before = receiver.CurrentHealth; int impacts = 0;
            ranged.HitConfirmed += _ => impacts++;
            Assert.That(ranged.TryFire(Vector2.right), Is.True); Assert.That(receiver.CurrentHealth, Is.EqualTo(before - attack.Damage));
            Assert.That(impacts, Is.EqualTo(1)); yield return null;
        }
        [UnityTest] public IEnumerator SaturatedRayRejectsWithoutDamageOrCooldown()
        {
            var triggers = new GameObject[64];
            try
            {
                for (int i = 0; i < triggers.Length; i++)
                {
                    triggers[i] = new GameObject("saturation trigger " + i);
                    triggers[i].transform.position = new Vector3(1f + i * .03f, 20f);
                    var collider = triggers[i].AddComponent<BoxCollider2D>(); collider.isTrigger = true; collider.size = Vector2.one * .02f;
                }
                Physics2D.SyncTransforms(); var receiver = target.GetComponent<DamageReceiver2D>(); float before = receiver.CurrentHealth;
                int shots = 0; ranged.ShotFired += (_, __) => shots++;
                Assert.That(ranged.TryFire(Vector2.right), Is.False); Assert.That(receiver.CurrentHealth, Is.EqualTo(before));
                Assert.That(ranged.CooldownRemaining, Is.Zero); Assert.That(shots, Is.Zero); yield return null;
            }
            finally { foreach (var trigger in triggers) if (trigger != null) Object.Destroy(trigger); }
        }
        private static void Set(object value, string field, object data) => value.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(value, data);
    }
}
