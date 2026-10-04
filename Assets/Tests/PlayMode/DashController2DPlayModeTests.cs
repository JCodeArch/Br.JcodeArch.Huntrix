using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Gameplay.Movement;
using HuntrX.Gameplay.Dash;
using HuntrX.Gameplay.Jump;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class DashController2DPlayModeTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.Destroy(createdObject);
                }
            }

            createdObjects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator GroundDashOwnsHorizontalMovementThenReturnsControl()
        {
            DashController2D dash = CreateDash(12f, 0.1f);
            HorizontalMovement2D movement = dash.GetComponent<HorizontalMovement2D>();
            Rigidbody2D body = dash.GetComponent<Rigidbody2D>();
            body.linearVelocityY = 2f;
            dash.SetGrounded(true);
            var transitions = new List<bool>();
            dash.DashStateChanged += transitions.Add;

            Assert.That(dash.TryStartDash(-1f), Is.True);
            Assert.That(dash.IsDashing, Is.True);
            Assert.That(dash.IsInvulnerable, Is.True);
            Assert.That(dash.TryStartDash(1f), Is.False);

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(-12f).Within(0.001f));
            Assert.That(body.linearVelocityY, Is.EqualTo(2f).Within(0.001f));

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
            CollectionAssert.AreEqual(new[] { true, false }, transitions);

            movement.SetMovementInput(Vector2.right);
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.GreaterThan(-12f));
        }

        [UnityTest]
        public IEnumerator DashCannotStartWithoutGroundOrDirection()
        {
            DashController2D dash = CreateDash(12f, 0.1f);
            Assert.That(dash.TryStartDash(1f), Is.False);

            dash.SetGrounded(true);
            LogAssert.Expect(LogType.Error,
                "DashController2D requires a finite, non-zero horizontal direction.");
            Assert.That(dash.TryStartDash(0f), Is.False);
            Assert.That(dash.TryStartDash(float.NaN), Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DisabledDashControllerCannotAcquireMovementOverride()
        {
            DashController2D dash = CreateDash(12f, 0.1f);
            HorizontalMovement2D movement = dash.GetComponent<HorizontalMovement2D>();
            Rigidbody2D body = dash.GetComponent<Rigidbody2D>();
            dash.enabled = false;
            dash.SetGrounded(true);

            Assert.That(dash.TryStartDash(1f), Is.False);
            movement.SetMovementInput(Vector2.right);
            yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocityX, Is.GreaterThan(0f).And.LessThan(6f));
            Assert.That(dash.IsDashing, Is.False);
        }

        [UnityTest]
        public IEnumerator DashShorterThanFixedStepAppliesOneMovementTickThenEnds()
        {
            DashController2D dash = CreateDash(12f, Time.fixedDeltaTime / 2f);
            dash.SetGrounded(true);

            Assert.That(dash.TryStartDash(1f), Is.True);
            yield return new WaitForFixedUpdate();

            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.GetComponent<Rigidbody2D>().linearVelocityX, Is.EqualTo(12f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator DashExpiresOnConfiguredFixedTick()
        {
            DashController2D dash = CreateDash(12f, Time.fixedDeltaTime * 5f);
            dash.SetGrounded(true);
            Assert.That(dash.TryStartDash(1f), Is.True);

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.That(dash.IsDashing, Is.True);
            }

            yield return new WaitForFixedUpdate();
            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
        }
        [UnityTest]
        public IEnumerator InvalidDashTuningReportsOnceAndDoesNotStart()
        {
            DashController2D dash = CreateDash(0f, 0.1f);
            dash.SetGrounded(true);
            LogAssert.Expect(LogType.Error,
                "DashController2D requires positive dash speed and duration values.");

            Assert.That(dash.TryStartDash(1f), Is.False);
            yield return new WaitForFixedUpdate();
            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
        }

        [UnityTest]
        public IEnumerator AirDashNormalizesDirectionLocksHorizontalVelocityAndLeavesGravityOnVerticalVelocity()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 10f, 0.2f);
            Rigidbody2D body = dash.GetComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            dash.SetGrounded(false);

            Assert.That(dash.TryStartAirDash(Vector2.one), Is.True);
            Assert.That(dash.IsDashing, Is.True);
            Assert.That(dash.IsInvulnerable, Is.True);

            yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocityX, Is.EqualTo(10f / Mathf.Sqrt(2f)).Within(0.01f));
            Assert.That(body.linearVelocityY, Is.LessThan(10f / Mathf.Sqrt(2f)));
            Assert.That(body.linearVelocityY, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator AirDashCanBeUsedOnceUntilGroundedSignalRechargesIt()
        {
            DashController2D dash = CreateDash(12f, 0.04f, 8f, 0.04f);
            dash.SetGrounded(false);

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            for (int i = 0; i < 5 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.TryStartAirDash(Vector2.left), Is.False);

            dash.SetGrounded(true);
            dash.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.left), Is.True);
        }

        [UnityTest]
        public IEnumerator LandingDuringAirDashRechargesWithoutEndingTheActiveDash()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.1f);
            dash.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.up), Is.True);

            dash.SetGrounded(true);
            Assert.That(dash.IsDashing, Is.True);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.False);

            for (int i = 0; i < 10 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            dash.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
        }

        [UnityTest]
        public IEnumerator InvalidAirDirectionDoesNotConsumeCharge()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.1f);
            dash.SetGrounded(false);
            LogAssert.Expect(LogType.Error,
                "DashController2D requires a finite, non-zero air dash direction.");

            Assert.That(dash.TryStartAirDash(Vector2.zero), Is.False);
            Assert.That(dash.TryStartAirDash(new Vector2(float.NaN, 1f)), Is.False);
            Assert.That(dash.TryStartAirDash(new Vector2(float.PositiveInfinity, 1f)), Is.False);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            Assert.That(dash.IsInvulnerable, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InvalidAirDashSettingsDoNotStartOrConsumeCharge()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 0f, 0.1f);
            dash.SetGrounded(false);
            LogAssert.Expect(LogType.Error,
                "DashController2D requires positive air dash speed and duration values.");

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.False);
            Assert.That(dash.IsDashing, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AirDashCannotTakeHorizontalOverrideFromAnotherOwner()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.1f);
            HorizontalMovement2D movement = dash.GetComponent<HorizontalMovement2D>();
            dash.SetGrounded(false);
            Assert.That(movement.TrySetHorizontalVelocityOverride(movement, 3f), Is.True);

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.False);
            movement.ClearHorizontalVelocityOverride(movement);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AirDashSuppressesJumpCutAndBufferedJumpUntilDashEnds()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.1f);
            JumpController2D jump = dash.gameObject.AddComponent<JumpController2D>();
            SetField(jump, "jumpVelocity", 8f);
            SetField(jump, "gravityScale", 0.01f);
            SetField(jump, "coyoteTime", 0.15f);
            SetField(jump, "jumpBufferTime", 0.15f);
            SetField(jump, "jumpCutMultiplier", 0.5f);
            Rigidbody2D body = dash.GetComponent<Rigidbody2D>();

            dash.SetGrounded(true);
            jump.SetGrounded(true);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(7f));

            jump.ReleaseJump();
            dash.SetGrounded(false);
            jump.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            jump.PressJump();
            yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocityY, Is.LessThan(0f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(-0.1f));

            for (int i = 0; i < 10 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(dash.IsDashing, Is.False);
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.LessThan(0f));

            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(7f));
        }

        [UnityTest]
        public IEnumerator DisablingAirDashClearsOverrideReleasesJumpAndPairsStateEvents()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.2f);
            JumpController2D jump = dash.gameObject.AddComponent<JumpController2D>();
            SetField(jump, "jumpVelocity", 8f);
            SetField(jump, "gravityScale", 0.01f);
            SetField(jump, "coyoteTime", 0.15f);
            SetField(jump, "jumpBufferTime", 0.15f);
            SetField(jump, "jumpCutMultiplier", 0.5f);
            HorizontalMovement2D movement = dash.GetComponent<HorizontalMovement2D>();
            Rigidbody2D body = dash.GetComponent<Rigidbody2D>();
            var transitions = new List<bool>();
            dash.DashStateChanged += transitions.Add;
            dash.SetGrounded(false);

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            dash.enabled = false;

            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
            CollectionAssert.AreEqual(new[] { true, false }, transitions);

            jump.SetGrounded(true);
            jump.PressJump();
            movement.SetMovementInput(Vector2.left);
            yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocityX, Is.LessThan(8f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7f));
        }

        [UnityTest]
        public IEnumerator AirDashExpiryPairsEventsAndRequiresLeavingGroundAfterRecharge()
        {
            DashController2D dash = CreateDash(12f, 0.1f, 8f, 0.04f);
            var transitions = new List<bool>();
            dash.DashStateChanged += transitions.Add;
            dash.SetGrounded(false);

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            Assert.That(dash.IsInvulnerable, Is.True);
            for (int i = 0; i < 5 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(dash.IsDashing, Is.False);
            Assert.That(dash.IsInvulnerable, Is.False);
            CollectionAssert.AreEqual(new[] { true, false }, transitions);

            dash.SetGrounded(true);
            Assert.That(dash.TryStartAirDash(Vector2.left), Is.False);
            dash.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.left), Is.True);
        }

        [UnityTest]
        public IEnumerator InfiniteAirDashSpeedIsRejectedWithoutConsumingCharge()
        {
            DashController2D dash = CreateDash(12f, 0.1f, float.PositiveInfinity, 0.1f);
            dash.SetGrounded(false);
            LogAssert.Expect(LogType.Error,
                "DashController2D requires positive air dash speed and duration values.");

            Assert.That(dash.TryStartAirDash(Vector2.right), Is.False);
            Assert.That(dash.IsDashing, Is.False);
            SetField(dash, "airDashSpeed", 8f);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressDuringAirDashSuppressionCannotDeferWallJump()
        {
            var gameObject = new GameObject("AirDashWallJumpSuppressionTestSubject");
            createdObjects.Add(gameObject);
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            HorizontalMovement2D movement = gameObject.AddComponent<HorizontalMovement2D>();
            SetField(movement, "maxHorizontalSpeed", 6f);
            SetField(movement, "acceleration", 10f);
            SetField(movement, "deceleration", 12f);
            JumpController2D jump = gameObject.AddComponent<JumpController2D>();
            SetField(jump, "jumpVelocity", 8f);
            SetField(jump, "gravityScale", 0.01f);
            SetField(jump, "coyoteTime", 0.15f);
            SetField(jump, "jumpBufferTime", 0.15f);
            SetField(jump, "jumpCutMultiplier", 0.5f);
            SetField(jump, "wallJumpHorizontalSpeed", 10f);
            DashController2D dash = gameObject.AddComponent<DashController2D>();
            SetField(dash, "dashSpeed", 12f);
            SetField(dash, "dashDuration", 0.1f);
            SetField(dash, "airDashSpeed", 8f);
            SetField(dash, "airDashDuration", 0.04f);
            dash.SetGrounded(false);
            Assert.That(dash.TryStartAirDash(Vector2.right), Is.True);
            SetWallContact(jump, true, Vector2.left);
            jump.PressJump();

            for (int i = 0; i < 6 && dash.IsDashing; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForFixedUpdate();
            Assert.That(dash.IsDashing, Is.False);
            Assert.That(body.linearVelocityY, Is.LessThanOrEqualTo(0f));
            Assert.That(body.linearVelocityX, Is.GreaterThan(0f), "A suppressed wall jump must not reverse the air-dash velocity.");
        }

        private static void SetWallContact(JumpController2D jump, bool touching, Vector2 outwardNormal)
        {
            var method = jump.GetType().GetMethod("SetWallContact", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "Expected JumpController2D.SetWallContact(bool, Vector2).");
            method.Invoke(jump, new object[] { touching, outwardNormal });
        }

        private DashController2D CreateDash(
            float dashSpeed,
            float dashDuration,
            float airDashSpeed = 8f,
            float airDashDuration = 0.1f)
        {
            var gameObject = new GameObject("DashTestSubject");
            createdObjects.Add(gameObject);
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            HorizontalMovement2D movement = gameObject.AddComponent<HorizontalMovement2D>();
            SetField(movement, "maxHorizontalSpeed", 6f);
            SetField(movement, "acceleration", 10f);
            SetField(movement, "deceleration", 12f);

            DashController2D dash = gameObject.AddComponent<DashController2D>();
            SetField(dash, "dashSpeed", dashSpeed);
            SetField(dash, "dashDuration", dashDuration);
            SetField(dash, "airDashSpeed", airDashSpeed);
            SetField(dash, "airDashDuration", airDashDuration);
            return dash;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected serialized tuning field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}