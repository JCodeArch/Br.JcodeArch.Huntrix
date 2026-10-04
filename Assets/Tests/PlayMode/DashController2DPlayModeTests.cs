using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Gameplay.Movement;
using HuntrX.Gameplay.Dash;
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

        private DashController2D CreateDash(float dashSpeed, float dashDuration)
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