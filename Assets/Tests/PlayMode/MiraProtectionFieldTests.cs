using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class MiraProtectionFieldTests
    {
        private readonly List<Object> objects = new List<Object>();
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private float previousScale;
        [SetUp] public void SetUp() { previousScale = Time.timeScale; Time.timeScale = 1f; }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Time.timeScale = previousScale;
            for (int i = objects.Count - 1; i >= 0; --i) if (objects[i] != null) Object.Destroy(objects[i]);
            objects.Clear();
            yield return null;
        }
        private Type RuntimeType(string name)
        {
            Type type = typeof(DamageReceiver2D).Assembly.GetType("HuntrX.Gameplay.Protection." + name);
            Assert.That(type, Is.Not.Null, name + " runtime contract is required");
            return type;
        }
        private void Set(object target, string field, object value) => target.GetType().GetField(field, Flags).SetValue(target, value);
        private T Get<T>(object target, string property) => (T)target.GetType().GetProperty(property).GetValue(target);
        private object Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, Flags).Invoke(target, args);
        private GameObject Root(string name, Vector2 position, bool optIn = true)
        {
            var root = new GameObject(name); objects.Add(root); root.SetActive(false); root.transform.position = position;
            var receiver = root.AddComponent<DamageReceiver2D>(); Set(receiver, "maximumHealth", 80f);
            var body = root.GetComponent<Rigidbody2D>(); body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeAll;
            if (optIn) root.AddComponent(RuntimeType("DamageProtection2D"));
            AddCollider(root, Vector2.zero);
            root.SetActive(true); return root;
        }
        private BoxCollider2D AddCollider(GameObject root, Vector2 local)
        {
            var child = new GameObject("hurtbox"); child.transform.SetParent(root.transform); child.transform.localPosition = local;
            var collider = child.AddComponent<BoxCollider2D>(); collider.size = Vector2.one * .2f; collider.isTrigger = true;
            child.AddComponent<Hurtbox2D>(); return collider;
        }
        private Component Protection(GameObject root) => root.GetComponent(RuntimeType("DamageProtection2D"));
        private bool Protected(GameObject root) => Get<bool>(Protection(root), "IsProtected");
        private Component Field(float duration = 1f, Vector2? position = null)
        {
            var root = Root("Mira field", position ?? Vector2.zero);
            root.SetActive(false);
            var child = new GameObject("field trigger"); child.transform.SetParent(root.transform, false);
            var circle = child.AddComponent<CircleCollider2D>(); circle.isTrigger = true;
            var field = root.AddComponent(RuntimeType("MiraProtectionField2D"));
            var definition = ScriptableObject.CreateInstance<MiraProtectionDefinition>(); objects.Add(definition);
            Set(definition, "durationSeconds", duration); Set(definition, "radius", 2f);
            Set(field, "definition", definition); Set(field, "fieldCollider", circle);
            root.SetActive(true); return field;
        }
        [TestCase("definition")]
        [TestCase("invalidDefinition")]
        [TestCase("fieldCollider")]
        [TestCase("nonTrigger")]
        [TestCase("rootCollider")]
        [TestCase("disabled")]
        [TestCase("offset")]
        [TestCase("owner")]
        [TestCase("ownerDisabled")]
        [TestCase("inactiveCollider")]
        public void ActivationRejectsInvalidDependenciesWithoutEnablingField(string dependency)
        {
            var field = Field(); var circle = field.GetComponentInChildren<CircleCollider2D>(true);
            if (dependency == "definition" || dependency == "fieldCollider") Set(field, dependency, null);
            if (dependency == "invalidDefinition") Set(Get<MiraProtectionDefinition>(field, "Definition"), "radius", 0f);
            if (dependency == "nonTrigger") circle.isTrigger = false;
            if (dependency == "rootCollider") Set(field, "fieldCollider", field.gameObject.AddComponent<CircleCollider2D>());
            if (dependency == "disabled") ((Behaviour)field).enabled = false;
            if (dependency == "offset") circle.offset = Vector2.one;
            if (dependency == "ownerDisabled") field.GetComponent<DamageReceiver2D>().enabled = false;
            if (dependency == "inactiveCollider") circle.gameObject.SetActive(false);
            if (dependency == "owner") Object.DestroyImmediate(field.GetComponent<DamageReceiver2D>());
            Assert.That(Call(field, "TryActivate"), Is.False);
            Assert.That(Get<bool>(field, "IsActive"), Is.False);
            Assert.That(circle.enabled, Is.False);
        }
        [UnityTest] public IEnumerator SeededTargetMovedBeforeFirstSimulationCannotRetainProtection()
        {
            var field = Field(); var target = Root("early moving ally", Vector2.right); var enemy = Enemy(); var attack = Attack();
            Call(field, "TryActivate"); Assert.That(Protected(target), Is.True);
            target.transform.position = Vector2.right * 4f;
            // No trigger pair existed before this movement, so no exit callback is guaranteed.
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False);
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Damaged"));
        }
        [UnityTest] public IEnumerator SeededFieldMovedBeforeFirstSimulationCannotProtectOutsideTarget()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); var attack = Attack();
            Call(field, "TryActivate"); Assert.That(Protected(target), Is.True);
            field.transform.position = Vector2.left * 4f;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False);
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Damaged"));
        }
        [UnityTest] public IEnumerator ProtectedTargetRemainsReservedAcrossExpiryThenNewActivationDamages()
        {
            var field = Field(Time.fixedDeltaTime * 3f); var target = Root("ally", Vector2.right); var enemy = Enemy(); int accepted = 0, parried = 0;
            var hitbox = Hitbox(enemy, (_, __) => accepted++, _ => parried++); var attack = Attack(); var collider = target.GetComponentInChildren<BoxCollider2D>();
            Call(field, "TryActivate"); hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f); Call(hitbox, "ProcessContact", collider);
            for (int tick = 0; tick < 4; ++tick) yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False);
            Call(hitbox, "ProcessContact", collider);
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.EqualTo(80f), "Protected target remains reserved for this swing");
            Assert.That(accepted, Is.Zero); Assert.That(parried, Is.Zero);
            target.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f); Call(hitbox, "ProcessContact", collider);
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.Zero);
            Assert.That(target.GetComponent<Rigidbody2D>().linearVelocity.x, Is.GreaterThan(0f));
            Assert.That(accepted, Is.EqualTo(1)); Assert.That(parried, Is.Zero);
        }
        [UnityTest] public IEnumerator ProtectedTargetRemainsReservedAcrossExitAndLaterActivationRechecks()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); int accepted = 0, parried = 0;
            var hitbox = Hitbox(enemy, (_, __) => accepted++, _ => parried++); var attack = Attack(); var collider = target.GetComponentInChildren<BoxCollider2D>();
            Call(field, "TryActivate"); hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f); Call(hitbox, "ProcessContact", collider);
            // Establish the trigger pair before moving so this tests a real exit callback.
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            target.transform.position = Vector2.right * 4f;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False); Call(hitbox, "ProcessContact", collider);
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.EqualTo(80f)); Assert.That(accepted, Is.Zero);
            hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f); Call(hitbox, "ProcessContact", collider);
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.Zero); Assert.That(accepted, Is.EqualTo(1)); Assert.That(parried, Is.Zero);
        }
        [UnityTest] public IEnumerator ReceiverEnteringAfterExpiryIsNotProtected()
        {
            var field = Field(Time.fixedDeltaTime * 2f); var target = Root("late ally", Vector2.right * 4f); var enemy = Enemy();
            Call(field, "TryActivate"); for (int tick = 0; tick < 3; ++tick) yield return new WaitForFixedUpdate();
            target.transform.position = Vector2.right; yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False); Assert.That(Resolve(target, enemy, Attack()), Is.EqualTo("Damaged"));
        }
        [Test] public void LaterActivationRechecksActiveProtectionWithoutPublishingEvents()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); int accepted = 0, parried = 0;
            var hitbox = Hitbox(enemy, (_, __) => accepted++, _ => parried++); var attack = Attack(); var collider = target.GetComponentInChildren<BoxCollider2D>();
            Call(field, "TryActivate");
            for (int activation = 0; activation < 2; ++activation)
            { hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f); Call(hitbox, "ProcessContact", collider); }
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.EqualTo(80f)); Assert.That(accepted, Is.Zero); Assert.That(parried, Is.Zero);
        }
        private AttackDefinition Attack()
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>(); objects.Add(attack);
            Set(attack, "damage", 100f); Set(attack, "activeDuration", 1f); Set(attack, "hitboxSize", Vector2.one);
            Set(attack, "horizontalKnockbackImpulse", 5f); Set(attack, "upwardKnockbackImpulse", 2f);
            Assert.That(attack.IsValid(out _), Is.True); return attack;
        }
        private GameObject Enemy()
        {
            var root = Root("enemy", Vector2.left * 4f, false); Set(root.GetComponent<DamageReceiver2D>(), "faction", CombatFaction2D.Demon); return root;
        }
        private string Resolve(GameObject target, GameObject enemy, AttackDefinition attack, float direction = 1f)
        {
            object[] args = { attack, enemy.GetComponent<DamageReceiver2D>(), direction, 0, null };
            return Call(target.GetComponent<DamageReceiver2D>(), "ResolveHit", args).ToString();
        }
        private AttackHitbox2D Hitbox(GameObject enemy, Action<CombatImpactEvent, float> accepted, Action<CombatParryEvent> parried)
        {
            enemy.SetActive(false);
            var child = new GameObject("attack collider"); child.transform.SetParent(enemy.transform, false);
            child.AddComponent<BoxCollider2D>().isTrigger = true;
            var hitbox = enemy.AddComponent<AttackHitbox2D>(); enemy.SetActive(true);
            typeof(AttackHitbox2D).GetEvent("AcceptedHit", Flags).GetAddMethod(true).Invoke(hitbox, new object[] { accepted });
            typeof(AttackHitbox2D).GetEvent("ParriedHit", Flags).GetAddMethod(true).Invoke(hitbox, new object[] { parried });
            return hitbox;
        }
        [Test] public void ProtectedContactReturnsDistinctResultWithoutHealthDeathOrKnockback()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); var attack = Attack();
            var receiver = target.GetComponent<DamageReceiver2D>(); var body = target.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.None; body.linearVelocity = new Vector2(2f, 3f);
            int deaths = 0; receiver.Died += _ => deaths++;
            Call(field, "TryActivate");
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Protected"));
            Assert.That(receiver.CurrentHealth, Is.EqualTo(80f)); Assert.That(body.linearVelocity, Is.EqualTo(new Vector2(2f, 3f))); Assert.That(deaths, Is.Zero);
            Assert.That(receiver.TryReceiveHit(attack, enemy.GetComponent<DamageReceiver2D>(), 1f), Is.False);
            ((Behaviour)field).enabled = false;
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Damaged"));
            Assert.That(receiver.CurrentHealth, Is.Zero); Assert.That(body.linearVelocity.x, Is.GreaterThan(2f)); Assert.That(deaths, Is.EqualTo(1));
        }
        [Test] public void ProtectedHitboxEmitsNeitherAcceptedNorParriedEvent()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); int accepted = 0, parried = 0;
            var hitbox = Hitbox(enemy, (_, __) => accepted++, _ => parried++); var attack = Attack(); Call(field, "TryActivate");
            hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f);
            Call(hitbox, "ProcessContact", target.GetComponentInChildren<BoxCollider2D>());
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.EqualTo(80f));
            Assert.That(accepted, Is.Zero); Assert.That(parried, Is.Zero);
        }
        [Test] public void ParryResolvesAndPublishesBeforeProtection()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); int accepted = 0, parried = 0;
            var hitbox = Hitbox(enemy, (_, __) => accepted++, _ => parried++); var attack = Attack();
            var definition = ScriptableObject.CreateInstance<ParryDefinition>(); objects.Add(definition); Set(definition, "windowDuration", .2f);
            var controller = target.AddComponent<ParryController2D>(); Set(controller, "definition", definition); Assert.That(controller.TryStartParry(), Is.True);
            Call(field, "TryActivate"); hitbox.BeginActivation(attack, enemy.GetComponent<DamageReceiver2D>(), 1f);
            Call(hitbox, "ProcessContact", target.GetComponentInChildren<BoxCollider2D>());
            Assert.That(parried, Is.EqualTo(1)); Assert.That(accepted, Is.Zero); Assert.That(controller.IsWindowActive, Is.False);
            Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.EqualTo(80f));
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Protected"));
        }
        [Test] public void DashInvulnerabilityRejectsBeforeParryAndProtection()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); var attack = Attack();
            var dash = target.AddComponent<HuntrX.Gameplay.Dash.DashController2D>(); Set(dash, "dashSpeed", 8f); Set(dash, "dashDuration", .2f);
            Set(target.GetComponent<DamageReceiver2D>(), "dashController", dash);
            dash.SetGrounded(true); Assert.That(dash.TryStartDash(1f), Is.True);
            var definition = ScriptableObject.CreateInstance<ParryDefinition>(); objects.Add(definition); Set(definition, "windowDuration", .2f);
            var parry = target.AddComponent<ParryController2D>(); Set(parry, "definition", definition);
            Assert.That(parry.TryStartParry(), Is.False);
            Call(field, "TryActivate"); Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Rejected"));
            Call(dash, "EndDash"); Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Protected"));
        }
        [Test] public void ExistingValidityFactionAndSelfRulesPrecedeProtection()
        {
            var field = Field(); var target = Root("ally", Vector2.right); var enemy = Enemy(); var attack = Attack(); Call(field, "TryActivate");
            Assert.That(Resolve(target, field.gameObject, attack), Is.EqualTo("Rejected"));
            Assert.That(Resolve(target, target, attack), Is.EqualTo("Rejected"));
            Assert.That(Resolve(target, enemy, null), Is.EqualTo("Rejected"));
            Assert.That(Resolve(target, enemy, attack, 0f), Is.EqualTo("Rejected"));
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Protected"));
        }
        [Test] public void UnoptedReceiverInsideActiveFieldStillTakesDamage()
        {
            var field = Field(); var target = Root("unopted", Vector2.right, false); var enemy = Enemy(); var attack = Attack(); Call(field, "TryActivate");
            Assert.That(Resolve(target, enemy, attack), Is.EqualTo("Damaged")); Assert.That(target.GetComponent<DamageReceiver2D>().CurrentHealth, Is.Zero);
        }
        [UnityTest] public IEnumerator ExpiryUsesScaledFixedTimeAndPausesAtZeroScale()
        {
            var field = Field(Time.fixedDeltaTime * 4f); var target = Root("ally", Vector2.right);
            Call(field, "TryActivate"); Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(.12f);
            Assert.That(Get<bool>(field, "IsActive"), Is.True); Assert.That(Protected(target), Is.True);
            Time.timeScale = .5f;
            for (int tick = 0; tick < 3; ++tick) yield return new WaitForFixedUpdate();
            Assert.That(Get<bool>(field, "IsActive"), Is.True, "Has not reached four scaled fixed ticks");
            for (int tick = 0; tick < 2; ++tick) yield return new WaitForFixedUpdate();
            Assert.That(Get<bool>(field, "IsActive"), Is.False);
            Assert.That(Protected(target), Is.False);
            Assert.That(field.GetComponentInChildren<CircleCollider2D>(true).enabled, Is.False);
        }
        [UnityTest] public IEnumerator RepeatedActivationDoesNotRefreshOriginalDuration()
        {
            var field = Field(Time.fixedDeltaTime * 6f); var target = Root("ally", Vector2.right);
            Call(field, "TryActivate");
            for (int tick = 0; tick < 4; ++tick) yield return new WaitForFixedUpdate();
            Assert.That(Call(field, "TryActivate"), Is.False);
            for (int tick = 0; tick < 3; ++tick) yield return new WaitForFixedUpdate();
            Assert.That(Get<bool>(field, "IsActive"), Is.False, "Original six-tick deadline survives attempted activation");
            Assert.That(Protected(target), Is.False);
            Assert.That(Call(field, "TryActivate"), Is.True, "Can activate again after expiry");
        }
        [UnityTest] public IEnumerator TriggerTransitionsFollowReceiverMovement()
        {
            var field = Field(); var target = Root("moving ally", Vector2.right * 4f);
            Call(field, "TryActivate"); Assert.That(Protected(target), Is.False);
            target.transform.position = Vector2.right;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.True);
            target.transform.position = Vector2.right * 4f;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False);
        }
        [UnityTest] public IEnumerator MovingFieldUpdatesProtectionAndMultipleColliderExits()
        {
            var field = Field(); var target = Root("multi moving ally", Vector2.right); var second = AddCollider(target, Vector2.up * .2f);
            Call(field, "TryActivate");
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            second.transform.localPosition = Vector2.right * 4f;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.True);
            field.transform.position = Vector2.left * 4f;
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(Protected(target), Is.False);
        }
        [Test] public void MultipleCollidersRemainProtectedUntilFinalExit()
        {
            var field = Field(); var target = Root("multi collider", Vector2.right); var second = AddCollider(target, Vector2.up * .2f);
            Call(field, "TryActivate"); var protection = Protection(target); var first = target.GetComponentInChildren<BoxCollider2D>();
            Call(protection, "Unregister", field, first);
            Assert.That(Protected(target), Is.True, "The second collider still overlaps");
            Call(protection, "Unregister", field, second);
            Assert.That(Protected(target), Is.False);
        }
        [TestCase(false)] [TestCase(true)] public void EndingOneSourcePreservesOtherSource(bool destroy)
        {
            var first = Field(); var second = Field(); var target = Root("ally", Vector2.right);
            Call(first, "TryActivate"); Call(second, "TryActivate");
            if (destroy) Object.DestroyImmediate(first); else ((Behaviour)first).enabled = false;
            Assert.That(Protected(target), Is.True);
            ((Behaviour)second).enabled = false;
            Assert.That(Protected(target), Is.False);
            Assert.That(second.GetComponentInChildren<CircleCollider2D>(true).enabled, Is.False);
        }
        [TestCase(false)] [TestCase(true)] public void ReceiverTeardownRemovesBothSides(bool destroy)
        {
            var field = Field(); var target = Root("ally", Vector2.right); var marker = Protection(target);
            Call(field, "TryActivate");
            if (destroy) Object.DestroyImmediate(marker); else ((Behaviour)marker).enabled = false;
            if (!destroy) { Assert.That(Get<bool>(marker, "IsProtected"), Is.False); ((Behaviour)marker).enabled = true; Assert.That(Protected(target), Is.False); }
            var members = (System.Collections.ICollection)field.GetType().GetField("members", Flags)?.GetValue(field);
            Assert.That(members, Is.Not.Null, "Field tracks receivers for bidirectional cleanup");
            Assert.That(members.Count, Is.EqualTo(1), "Only the field owner's marker remains registered");
            ((Behaviour)field).enabled = false;
        }
        [Test] public void ActivationSeedsExistingReceiversSynchronously()
        {
            var field = Field(); var target = Root("ally", Vector2.right);
            Assert.That(Call(field, "TryActivate"), Is.True);
            Assert.That(Protected(target), Is.True);
            Assert.That(Protected(field.gameObject), Is.True);
            Assert.That(field.GetComponentInChildren<CircleCollider2D>().radius, Is.EqualTo(2f));
        }
        [Test] public void OutsideAndUnoptedReceiversRemainUnprotected()
        {
            var field = Field(); var outside = Root("outside", Vector2.right * 4f); var unopted = Root("unopted", Vector2.right, false);
            Assert.That(Call(field, "TryActivate"), Is.True);
            Assert.That(Protected(outside), Is.False);
            Assert.That(unopted.GetComponent(RuntimeType("DamageProtection2D")), Is.Null);
        }
    }
}
