using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AttackController2DPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<Object> createdObjects = new List<Object>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.Destroy(createdObjects[i]);
                }
            }
            createdObjects.Clear();
            yield return null;
        }

        [Test]
        public void AttackStartsWithoutRequiringGroundedStateInput()
        {
            AttackFixture attacker = CreateAttacker(0.04f, 0.06f, 0.04f);

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Startup));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator ControllerActivatesOnlyItsOwnRootHitbox()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.1f, 0f);
            AttackFixture otherCharacter = CreateAttacker(0f, 0.1f, 0f);

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            Assert.That(otherCharacter.HitboxCollider.enabled, Is.False);
        }

        [TestCase(0f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidFacingDoesNotStart(float facing)
        {
            AttackFixture attacker = CreateAttacker(0f, 0.1f, 0f);

            Assert.That(attacker.Controller.TryStartAttack(facing), Is.False);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator RequestsInEveryBusyPhaseAreDiscardedWithoutQueueing()
        {
            AttackFixture attacker = CreateAttacker(0.06f, 0.06f, 0.06f);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);

            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            yield return WaitForState(attacker.Controller, AttackState2D.Active);
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            yield return WaitForState(attacker.Controller, AttackState2D.Recovery);
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);

            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
        }

        [UnityTest]
        public IEnumerator ZeroStartupAndRecoveryDoNotInsertExtraFixedTicks()
        {
            AttackFixture attacker = CreateAttacker(0f, Time.fixedDeltaTime, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            float maximum = target.MaximumHealth;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Active));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);

            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            yield return null;
            Assert.That(target.CurrentHealth, Is.EqualTo(maximum - attacker.Definition.Damage).Within(0.001f));
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator HitboxMirrorsOffsetWhenFacingRight()
        {
            yield return CheckHitboxOffset(1f, 0.8f);
        }

        [UnityTest]
        public IEnumerator HitboxMirrorsOffsetWhenFacingLeft()
        {
            yield return CheckHitboxOffset(-1f, -0.8f);
        }

        private IEnumerator CheckHitboxOffset(float facing, float expectedX)
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);

            Assert.That(attacker.Controller.TryStartAttack(facing), Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.size, Is.EqualTo(new Vector2(1.4f, 0.9f)));
            Assert.That(attacker.HitboxCollider.offset, Is.EqualTo(new Vector2(expectedX, 0.1f)));
        }

        [UnityTest]
        public IEnumerator HitboxOnlyDamagesDuringActiveWindowAndCanHitWithoutDashController()
        {
            AttackFixture attacker = CreateAttacker(0.06f, 0.1f, 0.04f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            float maximum = target.MaximumHealth;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(maximum));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);

            yield return WaitForState(attacker.Controller, AttackState2D.Active);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.LessThan(maximum));
            Assert.That(target.GetComponent<Rigidbody2D>().linearVelocityX,
                Is.EqualTo(attacker.Definition.HorizontalKnockbackImpulse).Within(0.01f));
            Assert.That(target.GetComponent<Rigidbody2D>().linearVelocityY,
                Is.EqualTo(attacker.Definition.UpwardKnockbackImpulse).Within(0.01f));

            float afterHit = target.CurrentHealth;
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(afterHit));
        }

        [UnityTest]
        public IEnumerator PersistentOverlapAndMultipleHurtboxesHitEachReceiverOnlyOnce()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            AddExtraHurtboxCollider(target.transform);
            float expected = target.MaximumHealth - attacker.Definition.Damage;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(target.CurrentHealth, Is.EqualTo(expected).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator OneActivationCanHitSeveralOpposingReceivers()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D first = CreateTarget(new Vector2(0.8f, 0f));
            DamageReceiver2D second = CreateTarget(new Vector2(1.15f, 0.5f));
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(first.CurrentHealth, Is.EqualTo(first.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
            Assert.That(second.CurrentHealth, Is.EqualTo(second.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
        }

        [Test]
        public void SelfAndSameFactionAreIgnoredAndDamageClampsAtZero()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D ally = CreateTarget(new Vector2(0.8f, 0f), CombatFaction2D.HuntrX);
            DamageReceiver2D enemy = CreateTarget(new Vector2(5f, 0f));

            Assert.That(attacker.Receiver.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.False);
            Assert.That(ally.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.False);
            Assert.That(ally.CurrentHealth, Is.EqualTo(ally.MaximumHealth));

            AttackDefinition lethalDefinition = CreateAttackDefinition(0f, 0.12f, 0f, 1000f);
            Assert.That(enemy.TryReceiveHit(lethalDefinition, attacker.Receiver, 1f), Is.True);
            Assert.That(enemy.CurrentHealth, Is.EqualTo(0f));
            Assert.That(enemy.IsAlive, Is.False);
            Assert.That(enemy.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.False);
        }

        [UnityTest]
        public IEnumerator DashRejectsHitButPersistentOverlapIsAcceptedAfterDashEnds()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.3f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f), CombatFaction2D.Demon, true,
                dashDuration: Time.fixedDeltaTime * 3f);
            DashController2D dash = target.GetComponent<DashController2D>();
            float maximum = target.MaximumHealth;
            dash.SetGrounded(true);
            Assert.That(dash.TryStartDash(1f), Is.True);
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(maximum));

            for (int i = 0; i < 10 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
                if (dash.IsDashing)
                {
                    Assert.That(target.CurrentHealth, Is.EqualTo(maximum));
                }
            }
            Assert.That(dash.IsInvulnerable, Is.False);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(maximum - attacker.Definition.Damage).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator AcceptedKnockbackPointsAwayAndUpward()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D rightTarget = CreateTarget(new Vector2(4f, 0f));
            DamageReceiver2D leftTarget = CreateTarget(new Vector2(-4f, 0f));

            Assert.That(rightTarget.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.True);
            Assert.That(leftTarget.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(rightTarget.GetComponent<Rigidbody2D>().linearVelocityX, Is.GreaterThan(0f));
            Assert.That(leftTarget.GetComponent<Rigidbody2D>().linearVelocityX, Is.LessThan(0f));
            Assert.That(rightTarget.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f));
            Assert.That(leftTarget.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator ExactHorizontalOverlapUsesFacingForKnockbackDirection()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D target = CreateTarget(Vector2.zero);
            attacker.HitboxCollider.enabled = false;
            target.GetComponent<Collider2D>().enabled = false;

            Assert.That(target.TryReceiveHit(attacker.Definition, attacker.Receiver, -1f), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(target.GetComponent<Rigidbody2D>().linearVelocityX, Is.LessThan(0f));
        }

        [UnityTest]
        public IEnumerator ExistingMovementMotorDeceleratesAcceptedKnockback()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(4f, 0f), CombatFaction2D.Demon, false, true);

            Assert.That(target.TryReceiveHit(attacker.Definition, attacker.Receiver, 1f), Is.True);
            yield return new WaitForFixedUpdate();
            float initialKnockbackSpeed = target.GetComponent<Rigidbody2D>().linearVelocityX;
            yield return new WaitForFixedUpdate();
            float deceleratedSpeed = target.GetComponent<Rigidbody2D>().linearVelocityX;

            Assert.That(initialKnockbackSpeed, Is.GreaterThan(0f));
            Assert.That(deceleratedSpeed, Is.GreaterThan(0f));
            Assert.That(deceleratedSpeed, Is.LessThan(initialKnockbackSpeed));
        }

        [UnityTest]
        public IEnumerator DashInvulnerabilityRejectsImpulseAndPreservesMovementOverride()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.12f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(4f, 0f), CombatFaction2D.Demon, true);
            DashController2D dash = target.GetComponent<DashController2D>();
            dash.SetGrounded(true);
            Assert.That(dash.TryStartDash(1f), Is.True);
            yield return new WaitForFixedUpdate();
            float dashSpeed = target.GetComponent<Rigidbody2D>().linearVelocityX;
            float health = target.CurrentHealth;

            Assert.That(target.TryReceiveHit(attacker.Definition, attacker.Receiver, -1f), Is.False);
            yield return new WaitForFixedUpdate();

            Assert.That(target.CurrentHealth, Is.EqualTo(health));
            Assert.That(target.GetComponent<Rigidbody2D>().linearVelocityX, Is.EqualTo(dashSpeed).Within(0.001f));
            Assert.That(dash.IsInvulnerable, Is.True);
        }

        [Test]
        public void InvalidDefinitionIsRejectedWithOneDiagnosticAndNoHitbox()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.1f, 0f);
            SetPrivateField(attacker.Definition, "damage", 0f);
            LogAssert.Expect(LogType.Error, "AttackController2D requires a valid AttackDefinition.");

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.False);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.False);
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void InvalidHitboxSetupRejectsAttackWithOneDiagnostic(bool includeCollider, bool isTrigger)
        {
            AttackFixture attacker = CreateAttacker(0f, 0.1f, 0f, includeCollider, isTrigger);
            LogAssert.Expect(LogType.Error, "AttackController2D requires a child BoxCollider2D trigger.");

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.False);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.False);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
        }

        [UnityTest]
        public IEnumerator DisablingControllerDuringOverlapPreventsDelayedHitAfterReenable()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0.1f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            float maximum = target.MaximumHealth;
            Physics2D.SyncTransforms();
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            float afterHit = target.CurrentHealth;
            Assert.That(afterHit, Is.LessThan(maximum));

            attacker.Controller.enabled = false;
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            attacker.Controller.enabled = true;
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(afterHit));
        }

        [UnityTest]
        public IEnumerator DisablingHitboxDuringActivationDoesNotResumeOldHit()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            Physics2D.SyncTransforms();
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            float afterHit = target.CurrentHealth;
            Assert.That(afterHit, Is.LessThan(target.MaximumHealth));

            attacker.Hitbox.enabled = false;
            yield return new WaitForFixedUpdate();
            attacker.Hitbox.enabled = true;
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            Assert.That(target.CurrentHealth, Is.EqualTo(afterHit));
        }

        [UnityTest]
        public IEnumerator DespawningAttackerDuringOverlapDoesNotCauseDelayedHit()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.3f, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            float maximumHealth = target.MaximumHealth;
            Physics2D.SyncTransforms();
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.HitboxCollider.enabled, Is.False);

            Object.Destroy(attacker.Root);
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(target.CurrentHealth, Is.EqualTo(maximumHealth));
        }

        private AttackFixture CreateAttacker(float startup, float active, float recovery,
            bool includeHitboxCollider = true, bool hitboxIsTrigger = true)
        {
            AttackDefinition definition = CreateAttackDefinition(startup, active, recovery, 12f);
            var root = new GameObject("attacker");
            root.SetActive(false);
            createdObjects.Add(root);
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            DamageReceiver2D receiver = root.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 100f);
            SetPrivateField(receiver, "faction", CombatFaction2D.HuntrX);

            AttackHitbox2D hitbox = root.AddComponent<AttackHitbox2D>();
            BoxCollider2D hitboxCollider = null;
            if (includeHitboxCollider)
            {
                GameObject colliderObject = new GameObject("attack hitbox");
                colliderObject.transform.SetParent(root.transform, false);
                hitboxCollider = colliderObject.AddComponent<BoxCollider2D>();
                hitboxCollider.isTrigger = hitboxIsTrigger;
                hitboxCollider.enabled = false;
            }
            SetPrivateField(hitbox, "hitboxCollider", hitboxCollider);

            AttackController2D controller = root.AddComponent<AttackController2D>();
            SetPrivateField(controller, "attackDefinition", definition);
            root.SetActive(true);
            createdObjects.Add(definition);
            Physics2D.SyncTransforms();
            return new AttackFixture(root, receiver, definition, controller, hitbox, hitboxCollider);
        }

        private DamageReceiver2D CreateTarget(Vector2 position, CombatFaction2D faction = CombatFaction2D.Demon,
            bool withDash = false, bool withMovement = false, float dashDuration = 2f)
        {
            var root = new GameObject("target");
            root.SetActive(false);
            root.transform.position = position;
            createdObjects.Add(root);
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            DamageReceiver2D receiver = root.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 100f);
            SetPrivateField(receiver, "faction", faction);
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 0.7f);
            root.AddComponent<Hurtbox2D>();
            if (withDash)
            {
                AddDashController(root, dashDuration);
            }
            else if (withMovement)
            {
                AddMovement(root);
            }
            root.SetActive(true);
            Physics2D.SyncTransforms();
            return receiver;
        }

        private static DashController2D AddDashController(GameObject target, float dashDuration)
        {
            HorizontalMovement2D movement = AddMovement(target);
            DashController2D dash = target.AddComponent<DashController2D>();
            SetPrivateField(dash, "dashSpeed", 10f);
            SetPrivateField(dash, "dashDuration", dashDuration);
            return dash;
        }

        private static HorizontalMovement2D AddMovement(GameObject target)
        {
            HorizontalMovement2D movement = target.AddComponent<HorizontalMovement2D>();
            SetPrivateField(movement, "maxHorizontalSpeed", 8f);
            SetPrivateField(movement, "acceleration", 20f);
            SetPrivateField(movement, "deceleration", 30f);
            return movement;
        }

        private AttackDefinition CreateAttackDefinition(float startup, float active, float recovery, float damage)
        {
            var definition = ScriptableObject.CreateInstance<AttackDefinition>();
            createdObjects.Add(definition);
            SetPrivateField(definition, "damage", damage);
            SetPrivateField(definition, "startupDuration", startup);
            SetPrivateField(definition, "activeDuration", active);
            SetPrivateField(definition, "recoveryDuration", recovery);
            SetPrivateField(definition, "hitboxSize", new Vector2(1.4f, 0.9f));
            SetPrivateField(definition, "hitboxOffset", new Vector2(0.8f, 0.1f));
            SetPrivateField(definition, "horizontalKnockbackImpulse", 5f);
            SetPrivateField(definition, "upwardKnockbackImpulse", 2f);
            return definition;
        }

        private static void AddExtraHurtboxCollider(Transform target)
        {
            GameObject child = new GameObject("second hurtbox");
            child.transform.SetParent(target, false);
            BoxCollider2D collider = child.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.3f, 0.3f);
            child.AddComponent<Hurtbox2D>();
        }

        private static IEnumerator WaitForState(AttackController2D controller, AttackState2D state)
        {
            for (int i = 0; i < 20 && controller.State != state; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(controller.State, Is.EqualTo(state));
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected serialized field '{fieldName}'.");
            field.SetValue(instance, value);
        }

        private sealed class AttackFixture
        {
            public AttackFixture(GameObject root, DamageReceiver2D receiver, AttackDefinition definition,
                AttackController2D controller, AttackHitbox2D hitbox, BoxCollider2D hitboxCollider)
            {
                Root = root;
                Receiver = receiver;
                Definition = definition;
                Controller = controller;
                Hitbox = hitbox;
                HitboxCollider = hitboxCollider;
            }

            public GameObject Root { get; }
            public DamageReceiver2D Receiver { get; }
            public AttackDefinition Definition { get; set; }
            public AttackController2D Controller { get; }
            public AttackHitbox2D Hitbox { get; }
            public BoxCollider2D HitboxCollider { get; }
        }
    }
}
