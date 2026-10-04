using HuntrX.Gameplay;
using HuntrX.Gameplay.Protection;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using HuntrX.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class MiraCharacterPlayModeTests
    {
        [UnityTest]
        public IEnumerator MiraProfileConfiguresSharedMovementCombatAndParryControllers()
        {
            yield return LoadCombatLabHost();
            GameObject prefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            Assert.That(prefab, Is.Not.Null, "The Mira prototype prefab must be available at runtime.");
            GameObject actor = Object.Instantiate(prefab);

            try
            {
                yield return null;

                CharacterDefinitionApplier2D applier = actor.GetComponent<CharacterDefinitionApplier2D>();
                HorizontalMovement2D movement = actor.GetComponent<HorizontalMovement2D>();
                JumpController2D jump = actor.GetComponent<JumpController2D>();
                DashController2D dash = actor.GetComponent<DashController2D>();
                AttackController2D attack = actor.GetComponent<AttackController2D>();
                ParryController2D parry = actor.GetComponent<ParryController2D>();
                Assert.That(actor.GetComponent<Hurtbox2D>(), Is.Not.Null);

                Assert.That(applier.Definition, Is.Not.Null);
                Assert.That(applier.Definition.DisplayName, Is.EqualTo("Mira"));

                var profile = applier.Definition.Movement;
                AssertConfigured(movement, "maxHorizontalSpeed", profile.MaxHorizontalSpeed);
                AssertConfigured(movement, "acceleration", profile.Acceleration);
                AssertConfigured(movement, "deceleration", profile.Deceleration);
                AssertConfigured(jump, "jumpVelocity", profile.JumpVelocity);
                AssertConfigured(jump, "wallJumpHorizontalSpeed", profile.WallJumpHorizontalSpeed);
                AssertConfigured(jump, "gravityScale", profile.GravityScale);
                AssertConfigured(jump, "coyoteTime", profile.CoyoteTime);
                AssertConfigured(jump, "jumpBufferTime", profile.JumpBufferTime);
                AssertConfigured(jump, "jumpCutMultiplier", profile.JumpCutMultiplier);
                AssertConfigured(dash, "dashSpeed", profile.DashSpeed);
                AssertConfigured(dash, "dashDuration", profile.DashDuration);
                AssertConfigured(dash, "airDashSpeed", profile.AirDashSpeed);
                AssertConfigured(dash, "airDashDuration", profile.AirDashDuration);
                AssertConfigured(attack, "comboDefinition", applier.Definition.CombatCombo);
                AssertConfigured(parry, "definition", applier.Definition.Parry);
                movement.SetMovementInput(Vector2.right);
                yield return new WaitForFixedUpdate();
                Assert.That(actor.GetComponent<Rigidbody2D>().linearVelocityX, Is.GreaterThan(0f));

                jump.SetGrounded(true);
                jump.PressJump();
                yield return new WaitForFixedUpdate();
                Assert.That(actor.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f));

                Assert.That(parry.TryStartParry(), Is.True);
                Assert.That(parry.IsWindowActive, Is.True);
                yield return new WaitForSeconds(0.2f);

                dash.SetGrounded(true);
                Assert.That(dash.TryStartDash(1f), Is.True);
                Assert.That(dash.IsDashing, Is.True);
                yield return new WaitForSeconds(0.25f);
                Assert.That(dash.IsDashing, Is.False);

                Assert.That(attack.TryStartAttack(1f, true), Is.True);
                Assert.That(attack.ActiveComboStepCount, Is.EqualTo(3));
                yield return new WaitForSeconds(0.26f);
                Assert.That(attack.TryStartAttack(1f, true), Is.True);
                Assert.That(attack.CurrentComboStepIndex, Is.EqualTo(1));
                yield return new WaitForSeconds(0.65f);
                Assert.That(attack.State, Is.EqualTo(AttackState2D.Idle));

                Assert.That(attack.TryStartAttack(1f, false), Is.True);
                Assert.That(attack.ActiveComboStepCount, Is.EqualTo(2));
                yield return new WaitForSeconds(0.26f);
                Assert.That(attack.TryStartAttack(1f, false), Is.True);
                Assert.That(attack.CurrentComboStepIndex, Is.EqualTo(1));
            }
            finally
            {
                Object.Destroy(actor);
            }
        }

        [UnityTest]
        public IEnumerator GroundAndAerialSwordCombosApplyDirectDamage()
        {
            yield return AssertComboDamagesTarget(true, Vector3.zero);
            yield return AssertComboDamagesTarget(false, new Vector3(0f, 2f, 0f));
        }

        [UnityTest]
        public IEnumerator ParryRejectsIncomingDamageAndDisableEndsActiveActions()
        {
            yield return LoadCombatLabHost();
            GameObject actorPrefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            GameObject targetPrefab = Resources.Load<GameObject>("Targets/DemonTarget");
            Assert.That(actorPrefab, Is.Not.Null);
            Assert.That(targetPrefab, Is.Not.Null);
            GameObject actor = Object.Instantiate(actorPrefab);
            GameObject attacker = Object.Instantiate(targetPrefab, new Vector3(1.3f, 0f, 0f), Quaternion.identity);

            try
            {
                yield return null;
                DamageReceiver2D receiver = actor.GetComponent<DamageReceiver2D>();
                ParryController2D parry = actor.GetComponent<ParryController2D>();
                AttackController2D attack = actor.GetComponent<AttackController2D>();
                AttackController2D attackerAttack = attacker.GetComponent<AttackController2D>();
                AttackHitbox2D hitbox = actor.GetComponent<AttackHitbox2D>();
                BoxCollider2D hitboxCollider = actor.GetComponentInChildren<BoxCollider2D>();
                BoxCollider2D attackerHitboxCollider = attacker.GetComponentInChildren<BoxCollider2D>();
                Physics2D.IgnoreCollision(actor.GetComponent<CapsuleCollider2D>(), attacker.GetComponent<CapsuleCollider2D>());

                Assert.That(parry.TryStartParry(), Is.True);
                float healthBeforeParry = receiver.CurrentHealth;
                bool parryEventRaised = false;
                Action<CombatParryEvent> onParry = _ => parryEventRaised = true;
                attackerAttack.ParryOccurred += onParry;
                Assert.That(attackerAttack.TryStartAttack(-1f, true), Is.True);
                for (int i = 0; i < 12 && !attackerHitboxCollider.enabled; i++)
                    yield return new WaitForFixedUpdate();
                Assert.That(attackerHitboxCollider.enabled, Is.True);
                for (int i = 0; i < 12 && !parryEventRaised; i++)
                    yield return new WaitForFixedUpdate();
                Assert.That(parryEventRaised, Is.True);
                Assert.That(parry.IsWindowActive, Is.False);
                Assert.That(receiver.CurrentHealth, Is.EqualTo(healthBeforeParry));

                attackerAttack.ParryOccurred -= onParry;

                Assert.That(attack.TryStartAttack(1f, true), Is.True);
                for (int i = 0; i < 12 && !hitboxCollider.enabled; i++)
                    yield return new WaitForFixedUpdate();
                Assert.That(hitboxCollider.enabled, Is.True);
                Assert.That(parry.TryStartParry(), Is.True);
                Assert.That(parry.IsWindowActive, Is.True);

                actor.SetActive(false);
                Assert.That(hitboxCollider.enabled, Is.False);
                Assert.That(hitbox.IsConfigurationValid, Is.True);
                Assert.That(parry.IsWindowActive, Is.False);
            }
            finally
            {
                Object.Destroy(actor);
                Object.Destroy(attacker);
            }
        }

        private static IEnumerator AssertComboDamagesTarget(bool grounded, Vector3 actorPosition)
        {
            yield return LoadCombatLabHost();
            GameObject actorPrefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            GameObject targetPrefab = Resources.Load<GameObject>("Targets/DemonTarget");
            Assert.That(actorPrefab, Is.Not.Null);
            Assert.That(targetPrefab, Is.Not.Null);
            GameObject actor = Object.Instantiate(actorPrefab, actorPosition, Quaternion.identity);
            GameObject target = Object.Instantiate(targetPrefab, actorPosition + new Vector3(1.3f, 0f, 0f), Quaternion.identity);
            Action<CombatImpactEvent> onImpact = null;

            try
            {
                yield return null;
                Rigidbody2D actorBody = actor.GetComponent<Rigidbody2D>();
                actorBody.gravityScale = 0f;
                actorBody.linearVelocity = Vector2.zero;
                Physics2D.IgnoreCollision(actor.GetComponent<CapsuleCollider2D>(), target.GetComponent<CapsuleCollider2D>());
                DamageReceiver2D targetReceiver = target.GetComponent<DamageReceiver2D>();
                bool impactOccurred = false;
                AttackController2D attack = actor.GetComponent<AttackController2D>();
                onImpact = _ => impactOccurred = true;
                attack.ImpactOccurred += onImpact;

                Assert.That(attack.TryStartAttack(1f, grounded), Is.True);
                for (int i = 0; i < 30 && targetReceiver.CurrentHealth >= targetReceiver.MaximumHealth; i++)
                    yield return new WaitForFixedUpdate();

                Assert.That(targetReceiver.CurrentHealth, Is.LessThan(targetReceiver.MaximumHealth),
                    grounded ? "Ground sword attacks should damage a nearby target." : "Aerial sword attacks should damage a nearby target.");
                Assert.That(impactOccurred, Is.True);
            }
            finally
            {
                if (onImpact != null && actor != null)
                    actor.GetComponent<AttackController2D>().ImpactOccurred -= onImpact;
                Object.Destroy(actor);
                Object.Destroy(target);
            }
        }

        [UnityTest]
        public IEnumerator ActualMiraPrefabImmediatelyProtectsOnlyOptedInInRangeAllies()
        {
            yield return LoadCombatLabHost();
            var prefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            Assert.That(prefab, Is.Not.Null, "Actual Mira Resources prefab is required.");
            var mira = Object.Instantiate(prefab);
            var ally = GenericReceiver("generic ally", Vector2.right, true);
            var unopted = GenericReceiver("unopted ally", Vector2.left, false);
            var field = mira.GetComponent<MiraProtectionField2D>();
            var outside = GenericReceiver("outside ally", Vector2.right * (field.Definition.Radius + 1f), true);
            var enemy = Object.Instantiate(Resources.Load<GameObject>("Targets/DemonTarget"), Vector3.up * 10f, Quaternion.identity);
            try
            {
                var hit = mira.GetComponent<CharacterDefinitionApplier2D>().Definition.CombatCombo.GroundedSteps[0].Attack;
                Assert.That(field.TryActivate(), Is.True);
                Assert.That(field.GetComponentInChildren<CircleCollider2D>().radius, Is.EqualTo(field.Definition.Radius));
                Assert.That(ally.GetComponent<DamageProtection2D>().IsProtected, Is.True);
                Assert.That(mira.GetComponent<DamageProtection2D>().IsProtected, Is.True);
                Assert.That(outside.GetComponent<DamageProtection2D>().IsProtected, Is.False);
                var receiver = ally.GetComponent<DamageReceiver2D>();
                var attacker = enemy.GetComponent<DamageReceiver2D>();
                Assert.That(receiver.TryReceiveHit(hit, attacker, 1f), Is.False);
                Assert.That(receiver.CurrentHealth, Is.EqualTo(80f));
                Assert.That(ally.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.zero));
                Assert.That(unopted.GetComponent<DamageReceiver2D>().TryReceiveHit(hit, attacker, 1f), Is.True);
                Assert.That(outside.GetComponent<DamageReceiver2D>().TryReceiveHit(hit, attacker, 1f), Is.True);
                mira.SetActive(false);
                Assert.That(ally.GetComponent<DamageProtection2D>().IsProtected, Is.False);
                Assert.That(receiver.TryReceiveHit(hit, attacker, 1f), Is.True);
                Assert.That(receiver.CurrentHealth, Is.EqualTo(80f - hit.Damage));
            }
            finally
            {
                Object.Destroy(mira); Object.Destroy(ally); Object.Destroy(unopted); Object.Destroy(outside); Object.Destroy(enemy);
            }
        }
        private static GameObject GenericReceiver(string name, Vector2 position, bool optedIn)
        {
            var root = new GameObject(name); root.SetActive(false); root.transform.position = position;
            var receiver = root.AddComponent<DamageReceiver2D>();
            typeof(DamageReceiver2D).GetField("maximumHealth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(receiver, 80f);
            root.GetComponent<Rigidbody2D>().gravityScale = 0f;
            var box = root.AddComponent<BoxCollider2D>(); box.isTrigger = true; box.size = Vector2.one * .2f;
            root.AddComponent<Hurtbox2D>();
            if (optedIn) root.AddComponent<DamageProtection2D>();
            root.SetActive(true); return root;
        }
        private static void AssertConfigured(object controller, string field, object expected)
        {
            var member = controller.GetType().GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(member, Is.Not.Null, field);
            Assert.That(member.GetValue(controller), Is.EqualTo(expected), field);
        }
        private static IEnumerator LoadCombatLabHost()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("CombatLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("CombatLab"));
        }
    }
}
