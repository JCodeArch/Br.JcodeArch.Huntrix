using System;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class CharacterSpriteSetTests
    {
        private Sprite first;
        private Sprite second;

        [SetUp]
        public void SetUp()
        {
            first = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero, 1);
            second = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero, 1);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(first);
            UnityEngine.Object.DestroyImmediate(second);
        }

        // Reflection keeps the test assembly runnable before the new runtime contract exists.
        // Missing feature is an assertion failure, rather than a compilation error.
        private static object Clip(Sprite[] frames, float[] durations, bool loop)
        {
            Type type = typeof(HuntrX.Data.CharacterArtDefinition).Assembly.GetType("HuntrX.Data.SpriteAnimationClip2D");
            Assert.That(type, Is.Not.Null, "State-specific sprite clips are not implemented.");
            return Activator.CreateInstance(type, new object[] { frames, durations, loop });
        }

        private static bool Valid(object clip) => (bool)clip.GetType().GetProperty("IsValid").GetValue(clip);
        private static Sprite Frame(object clip, float time) =>
            (Sprite)clip.GetType().GetMethod("FrameAt").Invoke(clip, new object[] { time });

        [Test]
        public void UnevenTimingPreservesAnticipationAndContactFrames()
        {
            object clip = Clip(new[] { first, second }, new[] { .1f, .3f }, false);
            Assert.That(Valid(clip), Is.True);
            Assert.That(Frame(clip, 0f), Is.SameAs(first));
            Assert.That(Frame(clip, .099f), Is.SameAs(first));
            Assert.That(Frame(clip, .1f), Is.SameAs(second));
            Assert.That(Frame(clip, .39f), Is.SameAs(second));
        }

        [Test]
        public void NonLoopingDefeatHoldsLastFrame()
        {
            object clip = Clip(new[] { first, second }, new[] { .1f, .3f }, false);
            Assert.That(Frame(clip, 1000f), Is.SameAs(second));
        }

        [Test]
        public void LoopReturnsToFirstFrameWithoutChangingTiming()
        {
            object clip = Clip(new[] { first, second }, new[] { .1f, .3f }, true);
            Assert.That(Frame(clip, .4f), Is.SameAs(first));
            Assert.That(Frame(clip, .51f), Is.SameAs(second));
        }

        [TestCase(0f)]
        [TestCase(-.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDurationCannotProduceAFrame(float duration)
        {
            object clip = Clip(new[] { first }, new[] { duration }, false);
            Assert.That(Valid(clip), Is.False);
            Assert.That(Frame(clip, 0f), Is.Null);
        }

        [Test]
        public void MissingFrameOrMismatchedTimingRejectsWholeClip()
        {
            Assert.That(Valid(Clip(new Sprite[] { first, null }, new[] { .1f, .1f }, false)), Is.False);
            Assert.That(Valid(Clip(new[] { first, second }, new[] { .1f }, false)), Is.False);
            Assert.That(Valid(Clip(Array.Empty<Sprite>(), Array.Empty<float>(), false)), Is.False);
        }

        [Test]
        public void InvalidClockDoesNotSelectAnArbitraryFrame()
        {
            object clip = Clip(new[] { first, second }, new[] { .1f, .3f }, true);
            Assert.That(Frame(clip, float.NaN), Is.Null);
            Assert.That(Frame(clip, float.PositiveInfinity), Is.Null);
            Assert.That(Frame(clip, -1f), Is.SameAs(first));
        }
    }
}
