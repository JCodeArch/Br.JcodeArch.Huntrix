using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class HorizontalMovement2DPlayModeTests
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
        public IEnumerator AcceleratesToClampedHorizontalSpeedAndPreservesVerticalVelocity()
        {
            HorizontalMovement2D movement = CreateMovement(4f, 8f, 16f);
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(0f, 2f);
            movement.SetMovementInput(new Vector2(2f, 5f));

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.GreaterThan(0f).And.LessThan(4f));
            Assert.That(body.linearVelocityY, Is.EqualTo(2f).Within(0.001f));
            Assert.That(movement.State, Is.EqualTo(HorizontalMovementState.Moving));

            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.linearVelocityX, Is.EqualTo(4f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator DeceleratesToIdleWhenInputIsReleased()
        {
            HorizontalMovement2D movement = CreateMovement(4f, 8f, 16f);
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(4f, -1.5f);
            movement.SetMovementInput(Vector2.zero);

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.LessThan(4f).And.GreaterThan(0f));
            Assert.That(body.linearVelocityY, Is.EqualTo(-1.5f).Within(0.001f));

            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
            Assert.That(movement.State, Is.EqualTo(HorizontalMovementState.Idle));
        }

        [UnityTest]
        public IEnumerator DirectionChangeDeceleratesBeforeMovingInTheOppositeDirection()
        {
            HorizontalMovement2D movement = CreateMovement(4f, 8f, 16f);
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            body.linearVelocityX = 2f;
            movement.SetMovementInput(Vector2.left);

            yield return new WaitForFixedUpdate();
            Assert.That(body.linearVelocityX, Is.GreaterThan(0f).And.LessThan(2f));

            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(body.linearVelocityX, Is.LessThan(0f));
            Assert.That(movement.State, Is.EqualTo(HorizontalMovementState.Moving));
        }

        [UnityTest]
        public IEnumerator InvalidTuningReportsOnceAndDoesNotMove()
        {
            HorizontalMovement2D movement = CreateMovement(0f, 8f, 16f);
            Rigidbody2D body = movement.GetComponent<Rigidbody2D>();
            movement.SetMovementInput(Vector2.right);

            LogAssert.Expect(LogType.Error,
                "HorizontalMovement2D requires positive maximum speed, acceleration, and deceleration values.");
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.That(body.linearVelocityX, Is.EqualTo(0f).Within(0.001f));
            Assert.That(movement.State, Is.EqualTo(HorizontalMovementState.Idle));
        }

        private HorizontalMovement2D CreateMovement(float maxSpeed, float acceleration, float deceleration)
        {
            var gameObject = new GameObject("HorizontalMovementTestSubject");
            createdObjects.Add(gameObject);
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            HorizontalMovement2D movement = gameObject.AddComponent<HorizontalMovement2D>();
            SetField(movement, "maxHorizontalSpeed", maxSpeed);
            SetField(movement, "acceleration", acceleration);
            SetField(movement, "deceleration", deceleration);
            return movement;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected serialized tuning field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}