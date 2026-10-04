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