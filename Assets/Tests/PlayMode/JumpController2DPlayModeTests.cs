using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Gameplay.Jump;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class JumpController2DPlayModeTests
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
        public IEnumerator GroundJumpAppliesVerticalVelocityAndPreservesHorizontalMovement()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            Vector2 worldGravity = Physics2D.gravity;
            body.linearVelocityX = 2.25f;
            jump.SetGrounded(true);
            jump.PressJump();

            yield return new WaitForFixedUpdate();

            Assert.That(body.gravityScale, Is.EqualTo(0.01f).Within(0.0001f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
            Assert.That(body.linearVelocityX, Is.EqualTo(2.25f).Within(0.001f));
            Assert.That(Physics2D.gravity, Is.EqualTo(worldGravity));
        }

        [UnityTest]
        public IEnumerator PressWithinCoyoteWindowJumpsAfterLeavingGround()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            jump.SetGrounded(false);
            jump.PressJump();

            yield return new WaitForFixedUpdate();

            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator PressAfterCoyoteWindowDoesNotJump()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.1f, 0.1f, 0.5f);
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            jump.SetGrounded(false);

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            jump.PressJump();
            yield return new WaitForFixedUpdate();

            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.LessThanOrEqualTo(0f));
        }

        [UnityTest]
        public IEnumerator BufferedPressJumpsWhenGroundedBeforeBufferExpires()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.2f, 0.5f);
            jump.SetGrounded(false);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.LessThanOrEqualTo(0f));

            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();

            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator BufferedPressExpiresBeforeLanding()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.1f, 0.1f, 0.5f);
            jump.SetGrounded(false);
            jump.PressJump();

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();

            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.LessThanOrEqualTo(0f));
        }

        [UnityTest]
        public IEnumerator EarlyReleaseCutsRisingJumpOnceButHoldingDoesNot()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            Vector2 worldGravity = Physics2D.gravity;
            jump.SetGrounded(true);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            float heldVelocity = body.linearVelocityY;

            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.linearVelocityY, Is.GreaterThan(heldVelocity - 0.1f));
            jump.ReleaseJump();
            yield return new WaitForFixedUpdate();
            float cutVelocity = body.linearVelocityY;
            Assert.That(cutVelocity, Is.GreaterThan(0f).And.LessThan(heldVelocity * 0.6f));

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(cutVelocity - 0.1f));
        }

        [UnityTest]
        public IEnumerator ReleaseBeforeBufferedLaunchStillCutsTheJump()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            jump.SetGrounded(true);
            jump.PressJump();
            jump.ReleaseJump();

            yield return new WaitForFixedUpdate();

            float velocity = jump.GetComponent<Rigidbody2D>().linearVelocityY;
            Assert.That(velocity, Is.GreaterThan(0f).And.LessThan(5f));
        }

        [UnityTest]
        public IEnumerator InvalidConfigurationReportsOnceAndDoesNotJump()
        {
            JumpController2D jump = CreateJump(0f, 0.01f, 0.15f, 0.15f, 0.5f);
            jump.SetGrounded(true);
            jump.PressJump();
            LogAssert.Expect(LogType.Error,
                "JumpController2D requires positive jump velocity, gravity scale, coyote time, and jump buffer, plus a jump cut multiplier between 0 and 1.");

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.LessThanOrEqualTo(0f));
        }

        [UnityTest]
        public IEnumerator JumpSuppressionDiscardsBufferedAndActiveJumpAndCanResume()
        {
            JumpController2D jump = CreateJump(8f, 1f, 0.15f, 0.15f, 0.5f);
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(true);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(7f));

            jump.ReleaseJump();
            jump.SetJumpSuppressed(true);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            float suppressedVelocity = body.linearVelocityY;
            Assert.That(suppressedVelocity, Is.GreaterThan(7f));

            jump.SetJumpSuppressed(false);
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.LessThan(suppressedVelocity));

            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(7f));
        }

        [UnityTest]
        public IEnumerator WallJumpPushesAwayFromLeftWall()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(10f).Within(0.001f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
        }

        [UnityTest]
        public IEnumerator WallJumpPushesAwayFromRightWall()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.left);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(-10f).Within(0.001f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
        }

        [UnityTest]
        public IEnumerator WallContactTakesPriorityOverCoyoteJump()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.left);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(-10f).Within(0.001f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
        }

        [UnityTest]
        public IEnumerator GroundJumpKeepsPriorityWhenGroundedBesideWall()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            body.linearVelocityX = 2f;
            jump.SetGrounded(true);
            SetWallContact(jump, true, Vector2.left);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
            Assert.That(body.linearVelocityX, Is.EqualTo(2f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator CoyoteJumpStillWorksWithoutWall()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            jump.SetGrounded(true);
            yield return new WaitForFixedUpdate();
            jump.SetGrounded(false);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(7.9f));
        }

        [UnityTest]
        public IEnumerator WallJumpCanBeUsedOnlyOnceUntilContactIsLost()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            body.linearVelocityX = 0f;
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator WallJumpRearmsAfterExplicitContactLoss()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            SetWallContact(jump, false, Vector2.zero);
            SetWallContact(jump, true, Vector2.left);
            body.linearVelocityX = 0f;
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(-10f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ChangingWallSideWithoutLossDoesNotRearmWallJump()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            SetWallContact(jump, true, Vector2.left);
            body.linearVelocityX = 0f;
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator InvalidVerticalWallContactClearsCurrentContact()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            SetWallContact(jump, true, Vector2.up);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ReleasingWallJumpUsesTheExistingJumpCut()
        {
            JumpController2D jump = CreateWallJump();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            jump.ReleaseJump();
            yield return new WaitForFixedUpdate();
            Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityY, Is.GreaterThan(0f).And.LessThan(5f));
        }

        [UnityTest]
        public IEnumerator MissingAndInvalidWallNormalsDoNotLaunchHorizontally()
        {
            Vector2[] invalidNormals = { Vector2.zero, Vector2.up, new Vector2(1f, 1f), new Vector2(float.NaN, 0f), new Vector2(1f, float.PositiveInfinity) };
            foreach (Vector2 normal in invalidNormals)
            {
                JumpController2D jump = CreateWallJump();
                jump.SetGrounded(false);
                if (!normal.Equals(Vector2.zero))
                {
                    SetWallContact(jump, true, Vector2.right);
                    SetWallContact(jump, true, normal);
                }
                jump.PressJump();
                yield return new WaitForFixedUpdate();
                Assert.That(jump.GetComponent<Rigidbody2D>().linearVelocityX, Is.EqualTo(0f).Within(0.001f));
            }
        }

        [UnityTest]
        public IEnumerator InvalidWallSpeedDoesNotConsumeContactOrChangeVelocity()
        {
            float[] invalidSpeeds = { 0f, -1f, float.NaN, float.PositiveInfinity };
            foreach (float invalidSpeed in invalidSpeeds)
            {
                JumpController2D jump = CreateWallJump();
                Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
                jump.SetGrounded(false);
                SetWallContact(jump, true, Vector2.right);
                body.linearVelocity = new Vector2(2f, 3f);
                SetField(jump, "wallJumpHorizontalSpeed", invalidSpeed);
                LogAssert.Expect(LogType.Error, "JumpController2D requires a positive finite wall jump horizontal speed.");
                jump.PressJump();
                yield return new WaitForFixedUpdate();
                Assert.That(body.linearVelocityX, Is.EqualTo(2f).Within(0.001f));
                Assert.That(body.linearVelocityY, Is.EqualTo(3f).Within(0.01f));
                SetField(jump, "wallJumpHorizontalSpeed", 10f);
                yield return new WaitForFixedUpdate();
                Assert.That(body.linearVelocityX, Is.EqualTo(10f).Within(0.001f));
                Object.Destroy(jump.gameObject);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator BufferedJumpStartsWhenWallContactBegins()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            SetWallContact(jump, true, Vector2.right);
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(10f).Within(0.001f));
            Assert.That(body.linearVelocityY, Is.GreaterThan(7.9f));
        }

        [UnityTest]
        public IEnumerator SlantedHorizontalWallNormalLaunchesAwayFromWall()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, new Vector2(4f, 1f));
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(10f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator EqualHorizontalAndVerticalNormalIsNotAWall()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, new Vector2(1f, 1f));
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator InvalidWallSpeedLogsOnlyOncePerController()
        {
            JumpController2D jump = CreateWallJump();
            Rigidbody2D body = jump.GetComponent<Rigidbody2D>();
            jump.SetGrounded(false);
            SetWallContact(jump, true, Vector2.right);
            SetField(jump, "wallJumpHorizontalSpeed", 0f);
            LogAssert.Expect(LogType.Error, "JumpController2D requires a positive finite wall jump horizontal speed.");
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            jump.PressJump();
            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
        }
        private JumpController2D CreateWallJump()
        {
            JumpController2D jump = CreateJump(8f, 0.01f, 0.15f, 0.15f, 0.5f);
            SetField(jump, "wallJumpHorizontalSpeed", 10f);
            return jump;
        }

        private static void SetWallContact(JumpController2D jump, bool touching, Vector2 outwardNormal)
        {
            var method = jump.GetType().GetMethod("SetWallContact", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "Expected JumpController2D.SetWallContact(bool, Vector2).");
            method.Invoke(jump, new object[] { touching, outwardNormal });
        }

        private JumpController2D CreateJump(
            float jumpVelocity,
            float gravityScale,
            float coyoteTime,
            float jumpBufferTime,
            float jumpCutMultiplier)
        {
            var gameObject = new GameObject("JumpTestSubject");
            createdObjects.Add(gameObject);
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            JumpController2D jump = gameObject.AddComponent<JumpController2D>();
            SetField(jump, "jumpVelocity", jumpVelocity);
            SetField(jump, "gravityScale", gravityScale);
            SetField(jump, "coyoteTime", coyoteTime);
            SetField(jump, "jumpBufferTime", jumpBufferTime);
            SetField(jump, "jumpCutMultiplier", jumpCutMultiplier);
            return jump;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected serialized tuning field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
