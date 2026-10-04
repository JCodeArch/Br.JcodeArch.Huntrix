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
        private float timeScaleBeforeTest;
        private float fixedDeltaTimeBeforeTest;

        [SetUp]
        public void SetUp()
        {
            timeScaleBeforeTest = Time.timeScale;
            fixedDeltaTimeBeforeTest = Time.fixedDeltaTime;
        }

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
            HitStopService[] services = Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include);
            foreach (HitStopService service in services)
            {
                if (service != null)
                {
                    Object.Destroy(service.gameObject);
                }
            }

            yield return null;
            Time.timeScale = timeScaleBeforeTest;
            Time.fixedDeltaTime = fixedDeltaTimeBeforeTest;
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
        public IEnumerator GroundedStartSelectsGroundedSequence()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition groundedFirst = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition aerialFirst = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            SetPrivateField(groundedFirst, "hitboxSize", new Vector2(1.1f, 0.7f));
            SetPrivateField(aerialFirst, "hitboxSize", new Vector2(1.9f, 0.7f));
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(groundedFirst), FinalStep(groundedFirst) },
                new[] { LinkStep(aerialFirst), FinalStep(aerialFirst) });

            Assert.That(attacker.Controller.TryStartAttack(1f, true), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(1.1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator AirborneStartSelectsAerialSequence()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition groundedFirst = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition aerialFirst = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            SetPrivateField(groundedFirst, "hitboxSize", new Vector2(1.1f, 0.7f));
            SetPrivateField(aerialFirst, "hitboxSize", new Vector2(1.9f, 0.7f));
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(groundedFirst), FinalStep(groundedFirst) },
                new[] { LinkStep(aerialFirst), FinalStep(aerialFirst) });

            Assert.That(attacker.Controller.TryStartAttack(1f, false), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(1.9f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator OnePressAdvancesOneStepAtATimeAndFinalStepCannotChain()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition first = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition second = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            AttackDefinition third = CreateAttackDefinition(0f, 0.2f, 0f, 7f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(first), LinkStep(second), FinalStep(third) },
                new[] { LinkStep(first), LinkStep(second), FinalStep(third) });

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(0));
            Assert.That(attacker.Controller.TryStartAttack(-1f, false), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
            Assert.That(attacker.Controller.TryStartAttack(1f, true), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(2));
            Assert.That(attacker.Controller.TryStartAttack(1f, true), Is.False);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(2));

            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(third.HitboxSize.x).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator WhiffCanStillAdvanceToNextComboStep()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(attacker.Definition), FinalStep(attacker.Definition) },
                new[] { LinkStep(attacker.Definition), FinalStep(attacker.Definition) });

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<Hurtbox2D>(FindObjectsSortMode.None), Is.Empty);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LinkWindowIncludesStartBoundaryAndExcludesEndBoundary()
        {
            AttackFixture atStart = CreateAttacker(0f, 0.2f, 0f);
            ComboStep[] steps = BoundaryTestSteps(atStart.Definition);
            SetComboSequences(atStart.Combo, steps, steps);
            Assert.That(atStart.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(atStart.Controller.TryStartAttack(-1f), Is.True,
                "A press at the sampled start boundary is included.");
            Assert.That(atStart.Controller.CurrentComboStepIndex, Is.EqualTo(1));

            AttackFixture atEnd = CreateAttacker(0f, 0.2f, 0f);
            steps = BoundaryTestSteps(atEnd.Definition);
            SetComboSequences(atEnd.Combo, steps, steps);
            Assert.That(atEnd.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            Assert.That(atEnd.Controller.TryStartAttack(-1f), Is.False,
                "A press at the sampled end boundary is excluded.");
            Assert.That(atEnd.Controller.CurrentComboStepIndex, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator PressBeforeLinkWindowIsDiscardedWithoutQueueing()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetComboSequences(attacker.Combo, BoundaryTestSteps(attacker.Definition),
                BoundaryTestSteps(attacker.Definition));

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(0),
                "An early press must not be buffered for the later window.");
        }

        [UnityTest]
        public IEnumerator BusyGroundAirContextDoesNotSwitchAndFacingIsRecapturedForNextStep()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition groundedFirst = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition groundedSecond = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            AttackDefinition aerialFirst = CreateAttackDefinition(0f, 0.2f, 0f, 7f);
            AttackDefinition aerialSecond = CreateAttackDefinition(0f, 0.2f, 0f, 8f);
            SetPrivateField(groundedSecond, "hitboxSize", new Vector2(2.2f, 0.7f));
            SetPrivateField(groundedSecond, "hitboxOffset", new Vector2(0.8f, 0.1f));
            SetPrivateField(aerialSecond, "hitboxSize", new Vector2(3.2f, 0.7f));
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(groundedFirst), new ComboStep(groundedSecond, float.NaN, float.PositiveInfinity, 0.13f) },
                new[] { LinkStep(aerialFirst), FinalStep(aerialSecond) });

            Assert.That(attacker.Controller.TryStartAttack(1f, true), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(-1f, false), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(2.2f).Within(0.001f),
                "The ground chain stays selected even though the caller now reports airborne.");
            Assert.That(attacker.HitboxCollider.offset.x, Is.EqualTo(-0.8f).Within(0.001f),
                "The accepted link press supplies the new facing direction.");
            Assert.That(attacker.Hitbox.ComboStepIndex, Is.EqualTo(1));
            Assert.That(attacker.Hitbox.HitStopDuration, Is.EqualTo(0.13f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator AcceptedLinkClosesActiveHitboxAndBeginsNextStepWithItsContext()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition first = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition second = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            SetPrivateField(first, "hitboxSize", new Vector2(1.2f, 0.7f));
            SetPrivateField(second, "hitboxSize", new Vector2(2.2f, 0.7f));
            SetComboSequences(attacker.Combo,
                new[] { new ComboStep(first, 0f, 0.18f, 0.07f), FinalStep(second) },
                new[] { new ComboStep(first, 0f, 0.18f, 0.07f), FinalStep(second) });

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            Assert.That(attacker.Hitbox.ComboStepIndex, Is.EqualTo(0));
            Assert.That(attacker.Hitbox.HitStopDuration, Is.EqualTo(0.07f).Within(0.001f));

            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            Assert.That(attacker.HitboxCollider.enabled, Is.False,
                "Transition closes the previous active hitbox immediately.");
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(2.2f).Within(0.001f));
            Assert.That(attacker.Hitbox.ComboStepIndex, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FreshChainSamplesGroundedContextAgainAfterCompletion()
        {
            AttackFixture attacker = CreateAttacker(0f, Time.fixedDeltaTime, 0f);
            AttackDefinition groundedFirst = CreateAttackDefinition(0f, Time.fixedDeltaTime, 0f, 5f);
            AttackDefinition groundedSecond = CreateAttackDefinition(0f, Time.fixedDeltaTime, 0f, 6f);
            AttackDefinition aerialFirst = CreateAttackDefinition(0f, 0.12f, 0f, 7f);
            AttackDefinition aerialSecond = CreateAttackDefinition(0f, 0.12f, 0f, 8f);
            SetPrivateField(groundedFirst, "hitboxSize", new Vector2(1.1f, 0.7f));
            SetPrivateField(aerialFirst, "hitboxSize", new Vector2(2.1f, 0.7f));
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(groundedFirst), FinalStep(groundedSecond) },
                new[] { LinkStep(aerialFirst), FinalStep(aerialSecond) });

            Assert.That(attacker.Controller.TryStartAttack(1f, true), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);

            Assert.That(attacker.Controller.TryStartAttack(1f, false), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(0));
            Assert.That(attacker.HitboxCollider.size.x, Is.EqualTo(2.1f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator HitStopFreezesComboClockButStillAcceptsAnOpenLinkPress()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(attacker.Definition, 0f, 0.08f), FinalStep(attacker.Definition) },
                new[] { LinkStep(attacker.Definition, 0f, 0.08f), FinalStep(attacker.Definition) });
            HitStopService service = HitStopService.EnsureInstance();
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            service.RequestHitStop(0.2f);
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            Assert.That(attacker.Controller.TryStartAttack(-1f, false), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(Time.timeScale, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator HitStopFreezesComboClockAndRejectsPressOutsideLinkWindow()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(attacker.Definition, 0.06f, 0.14f), FinalStep(attacker.Definition) },
                new[] { LinkStep(attacker.Definition, 0.06f, 0.14f), FinalStep(attacker.Definition) });
            HitStopService service = HitStopService.EnsureInstance();
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            service.RequestHitStop(0.2f);
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            Assert.That(attacker.Controller.TryStartAttack(-1f, false), Is.False);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(0));
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.That(Time.timeScale, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator AcceptedLinkedHitPublishesTypedPayloadAfterHitStop()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition first = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition linked = CreateAttackDefinition(0f, 0.2f, 0f, 7f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(first, 0f, 0.2f), new ComboStep(linked, float.NaN, float.PositiveInfinity, 0.2f) },
                new[] { LinkStep(first, 0f, 0.2f), new ComboStep(linked, float.NaN, float.PositiveInfinity, 0.2f) });
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            HitStopService service = HitStopService.EnsureInstance();
            var impacts = new List<CombatImpactEvent>();
            float timeScaleWhenPublished = -1f;
            attacker.Controller.ImpactOccurred += impact =>
            {
                impacts.Add(impact);
                timeScaleWhenPublished = Time.timeScale;
            };
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 5 && impacts.Count == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(impacts, Has.Count.EqualTo(1));
            Assert.That(impacts[0].Attacker, Is.SameAs(attacker.Receiver));
            Assert.That(impacts[0].Receiver, Is.SameAs(target));
            Assert.That(impacts[0].AttackDefinition, Is.SameAs(linked));
            Assert.That(impacts[0].ComboStepIndex, Is.EqualTo(1));
            Assert.That(timeScaleWhenPublished, Is.Zero,
                "Hit stop must be requested before the public impact notification is dispatched.");
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - linked.Damage).Within(0.001f));
            Assert.That(service, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DuplicateHurtboxesPublishOneImpactAndApplyDamageOnce()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            AddExtraHurtboxCollider(target.transform);
            var impacts = new List<CombatImpactEvent>();
            attacker.Controller.ImpactOccurred += impacts.Add;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 6 && attacker.Controller.State != AttackState2D.Idle; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(impacts, Has.Count.EqualTo(1));
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
            Assert.That(Time.timeScale, Is.GreaterThan(0f), "A zero-duration hit stop must not pause time.");
        }

        [UnityTest]
        public IEnumerator SameFactionContactDoesNotPublishImpactOrRequestHitStop()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0.25f);
            DamageReceiver2D ally = CreateTarget(new Vector2(0.8f, 0f), CombatFaction2D.HuntrX);
            yield return AssertRejectedContactHasNoImpactOrHitStop(attacker, ally);
        }

        [UnityTest]
        public IEnumerator SelfContactDoesNotPublishImpactOrRequestHitStop()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0.25f);
            CreateSelfHurtbox(attacker);
            yield return AssertRejectedContactHasNoImpactOrHitStop(attacker, attacker.Receiver);
        }

        [UnityTest]
        public IEnumerator DeadReceiverContactDoesNotPublishImpactOrRequestHitStop()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0.25f);
            DamageReceiver2D dead = CreateTarget(new Vector2(0.8f, 0f));
            AttackDefinition lethal = CreateAttackDefinition(0f, 0.1f, 0f, dead.MaximumHealth);
            Assert.That(dead.TryReceiveHit(lethal, attacker.Receiver, 1f), Is.True);
            dead.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            yield return AssertRejectedContactHasNoImpactOrHitStop(attacker, dead);
        }

        [UnityTest]
        public IEnumerator DashInvulnerableContactDoesNotPublishImpactOrRequestHitStop()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0.25f);
            DamageReceiver2D dashingTarget = CreateTarget(new Vector2(0.8f, 0f), CombatFaction2D.Demon, true,
                dashDuration: 1f);
            DashController2D dash = dashingTarget.GetComponent<DashController2D>();
            SetPrivateField(dash, "dashSpeed", 0.1f);
            dash.SetGrounded(true);
            Assert.That(dash.TryStartDash(1f), Is.True);
            Assert.That(dash.IsInvulnerable, Is.True);

            yield return AssertRejectedContactHasNoImpactOrHitStop(attacker, dashingTarget);
        }

        [UnityTest]
        public IEnumerator ControllerDisableCancelsHitboxAndReenableDoesNotDuplicateInternalSubscription()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            var impacts = new List<CombatImpactEvent>();
            attacker.Controller.ImpactOccurred += impacts.Add;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            attacker.Controller.enabled = false;
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            yield return new WaitForFixedUpdate();
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth));
            Assert.That(impacts, Is.Empty);

            attacker.Controller.enabled = true;
            attacker.Controller.enabled = false;
            attacker.Controller.enabled = true;
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 5 && impacts.Count == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            Assert.That(impacts, Has.Count.EqualTo(1));
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ImpactConsumerCanUnsubscribeWhileDisabledAndResubscribeWhenEnabled()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            int consumerCalls = 0;
            System.Action<CombatImpactEvent> consumer = _ => consumerCalls++;

            // Consumers own their own enable/disable subscription lifecycle.
            attacker.Controller.ImpactOccurred += consumer;
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);
            Assert.That(consumerCalls, Is.EqualTo(1));

            attacker.Controller.ImpactOccurred -= consumer;
            ResetTargetAtHitbox(target);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);
            Assert.That(consumerCalls, Is.EqualTo(1));

            attacker.Controller.ImpactOccurred += consumer;
            ResetTargetAtHitbox(target);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);
            Assert.That(consumerCalls, Is.EqualTo(2));
            Assert.That(target.CurrentHealth,
                Is.EqualTo(target.MaximumHealth - (3f * attacker.Definition.Damage)).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ImpactListenerReentryCannotDuplicateDamageOrImpact()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            BoxCollider2D targetCollider = target.GetComponent<Collider2D>() as BoxCollider2D;
            var impacts = new List<CombatImpactEvent>();
            bool reentered = false;
            attacker.Controller.ImpactOccurred += impact =>
            {
                impacts.Add(impact);
                if (!reentered)
                {
                    reentered = true;
                    attacker.Hitbox.SendMessage("OnTriggerStay2D", targetCollider,
                        SendMessageOptions.DontRequireReceiver);
                }
            };
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 5 && impacts.Count == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(reentered, Is.True);
            Assert.That(impacts, Has.Count.EqualTo(1));
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ThrowingImpactListenerDoesNotSuppressDamageHitStopOrLaterListeners()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            SetFirstStepHitStop(attacker, 0.25f);
            DamageReceiver2D target = CreateTarget(new Vector2(0.8f, 0f));
            BoxCollider2D targetCollider = target.GetComponent<Collider2D>() as BoxCollider2D;
            var exceptionMessage = new System.Text.RegularExpressions.Regex("first impact listener failed");
            var service = HitStopService.EnsureInstance();
            int firstListenerCalls = 0;
            int laterListenerCalls = 0;
            float scaleSeenByLaterListener = -1f;
            attacker.Controller.ImpactOccurred += _ =>
            {
                firstListenerCalls++;
                attacker.Hitbox.SendMessage("OnTriggerStay2D", targetCollider,
                    SendMessageOptions.DontRequireReceiver);
                throw new System.InvalidOperationException("first impact listener failed");
            };
            attacker.Controller.ImpactOccurred += _ =>
            {
                laterListenerCalls++;
                scaleSeenByLaterListener = Time.timeScale;
            };
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, exceptionMessage);
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 5 && laterListenerCalls == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(firstListenerCalls, Is.EqualTo(1));
            Assert.That(laterListenerCalls, Is.EqualTo(1));
            Assert.That(scaleSeenByLaterListener, Is.Zero);
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth - attacker.Definition.Damage).Within(0.001f));
            Assert.That(service, Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator DisablingControllerCancelsActiveComboAndReenableStartsAtFirstStep()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            AttackDefinition first = CreateAttackDefinition(0f, 0.2f, 0f, 5f);
            AttackDefinition second = CreateAttackDefinition(0f, 0.2f, 0f, 6f);
            SetComboSequences(attacker.Combo,
                new[] { LinkStep(first), FinalStep(second) }, new[] { LinkStep(first), FinalStep(second) });
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.enabled, Is.True);

            attacker.Controller.enabled = false;
            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            Assert.That(attacker.HitboxCollider.enabled, Is.False);
            attacker.Controller.enabled = true;
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(0));
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
        public IEnumerator RequestsOutsideConfiguredBusyWindowAreDiscardedWithoutQueueing()
        {
            AttackFixture attacker = CreateAttacker(0.06f, 0.06f, 0.06f);
            ComboStep[] steps = { LinkStep(attacker.Definition, 0.1f, 0.12f), FinalStep(attacker.Definition) };
            SetComboSequences(attacker.Combo, steps, steps);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);

            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            yield return WaitForState(attacker.Controller, AttackState2D.Active);
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False);
            yield return WaitForState(attacker.Controller, AttackState2D.Recovery);
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.False,
                "The end boundary is exclusive, so the press is still outside the configured window.");
            yield return WaitForState(attacker.Controller, AttackState2D.Idle);

            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Idle));
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
        }

        [UnityTest]
        public IEnumerator OpenLinkWindowCanAcceptPressDuringActivePhase()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.2f, 0f);
            ComboStep[] steps = { LinkStep(attacker.Definition, 0.02f, 0.12f), FinalStep(attacker.Definition) };
            SetComboSequences(attacker.Combo, steps, steps);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(attacker.Controller.State, Is.EqualTo(AttackState2D.Active));
            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OpenLinkWindowCanAcceptPressDuringRecoveryPhase()
        {
            AttackFixture attacker = CreateAttacker(0f, 0.04f, 0.2f);
            ComboStep[] steps = { LinkStep(attacker.Definition, 0.04f, 0.2f), FinalStep(attacker.Definition) };
            SetComboSequences(attacker.Combo, steps, steps);
            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            yield return WaitForState(attacker.Controller, AttackState2D.Recovery);

            Assert.That(attacker.Controller.TryStartAttack(-1f), Is.True);
            Assert.That(attacker.Controller.CurrentComboStepIndex, Is.EqualTo(1));
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
            LogAssert.Expect(LogType.Error, "AttackController2D requires a valid CombatComboDefinition.");

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
            yield return new WaitForFixedUpdate();
            Assert.That(attacker.HitboxCollider.enabled, Is.True);
            float healthAfterAcceptedHit = target.CurrentHealth;
            Assert.That(healthAfterAcceptedHit, Is.LessThan(maximumHealth));

            Object.Destroy(attacker.Root);
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(target.CurrentHealth, Is.EqualTo(healthAfterAcceptedHit));
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

            CombatComboDefinition combo = CreateDefaultComboDefinition(definition);
            AttackController2D controller = root.AddComponent<AttackController2D>();
            SetPrivateField(controller, "comboDefinition", combo);
            root.SetActive(true);
            Physics2D.SyncTransforms();
            return new AttackFixture(root, receiver, definition, combo, controller, hitbox, hitboxCollider);
        }

        private CombatComboDefinition CreateDefaultComboDefinition(AttackDefinition definition)
        {
            var combo = ScriptableObject.CreateInstance<CombatComboDefinition>();
            createdObjects.Add(combo);
            float totalDuration = definition.StartupDuration + definition.ActiveDuration + definition.RecoveryDuration;
            var link = new ComboStep(definition, totalDuration * 0.75f, totalDuration, 0.05f);
            var final = FinalStep(definition);
            SetComboSequences(combo, new[] { link, final }, new[] { link, final });
            return combo;
        }

        private static void SetFirstStepHitStop(AttackFixture attacker, float duration)
        {
            float totalDuration = attacker.Definition.StartupDuration + attacker.Definition.ActiveDuration +
                attacker.Definition.RecoveryDuration;
            ComboStep[] steps =
            {
                new ComboStep(attacker.Definition, 0f, totalDuration, duration),
                FinalStep(attacker.Definition)
            };
            SetComboSequences(attacker.Combo, steps, steps);
        }

        private IEnumerator AssertRejectedContactHasNoImpactOrHitStop(AttackFixture attacker,
            DamageReceiver2D target)
        {
            var impacts = new List<CombatImpactEvent>();
            attacker.Controller.ImpactOccurred += impacts.Add;
            float healthBefore = target.CurrentHealth;
            float scaleBefore = Time.timeScale;
            Physics2D.SyncTransforms();

            Assert.That(attacker.Controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(impacts, Is.Empty);
            Assert.That(target.CurrentHealth, Is.EqualTo(healthBefore).Within(0.001f));
            Assert.That(Time.timeScale, Is.EqualTo(scaleBefore).Within(0.001f));
        }

        private void CreateSelfHurtbox(AttackFixture attacker)
        {
            var child = new GameObject("self hurtbox");
            child.transform.SetParent(attacker.Root.transform, false);
            child.transform.localPosition = new Vector3(0.8f, 0f, 0f);
            Rigidbody2D body = child.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            BoxCollider2D collider = child.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.7f, 0.7f);
            child.AddComponent<Hurtbox2D>();
            createdObjects.Add(child);
            Physics2D.SyncTransforms();
        }

        private static void ResetTargetAtHitbox(DamageReceiver2D target)
        {
            target.transform.position = new Vector2(0.8f, 0f);
            target.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }

        private static ComboStep LinkStep(AttackDefinition attack, float windowStart = 0f, float windowEnd = -1f)
        {
            if (windowEnd < 0f)
            {
                windowEnd = attack.StartupDuration + attack.ActiveDuration + attack.RecoveryDuration;
            }
            return new ComboStep(attack, windowStart, windowEnd, 0.05f);
        }

        private static ComboStep FinalStep(AttackDefinition attack) =>
            new ComboStep(attack, float.NaN, float.PositiveInfinity, 0.05f);

        private static ComboStep[] BoundaryTestSteps(AttackDefinition attack)
        {
            float step = Time.fixedDeltaTime;
            return new[] { LinkStep(attack, 2f * step, 4f * step), FinalStep(attack) };
        }

        private static void SetComboSequences(CombatComboDefinition combo, ComboStep[] grounded, ComboStep[] aerial)
        {
            SetPrivateField(combo, "groundedSteps", grounded);
            SetPrivateField(combo, "aerialSteps", aerial);
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
                CombatComboDefinition combo, AttackController2D controller, AttackHitbox2D hitbox,
                BoxCollider2D hitboxCollider)
            {
                Root = root;
                Receiver = receiver;
                Definition = definition;
                Combo = combo;
                Controller = controller;
                Hitbox = hitbox;
                HitboxCollider = hitboxCollider;
            }

            public GameObject Root { get; }
            public DamageReceiver2D Receiver { get; }
            public AttackDefinition Definition { get; set; }
            public CombatComboDefinition Combo { get; }
            public AttackController2D Controller { get; }
            public AttackHitbox2D Hitbox { get; }
            public BoxCollider2D HitboxCollider { get; }
        }
    }
}
