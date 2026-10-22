using System;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CharacterSpritePlaybackTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Sprite first, second, external;
        private SpriteRenderer target;
        private Behaviour playback;
        private CharacterSpriteSet2D set;
        private GameObject actor;

        [SetUp]
        public void SetUp()
        {
            var texture = new Texture2D(2, 2);
            owned.Add(texture);
            first = MakeFrame(texture);
            second = MakeFrame(texture);
            external = MakeFrame(texture);
            actor = new GameObject("Actor with independent physical transform");
            actor.transform.localScale = new Vector3(1.3f, .8f, 1f);
            actor.AddComponent<BoxCollider2D>();
            owned.Add(actor);
            var visual = new GameObject("Visual only");
            visual.transform.SetParent(actor.transform, false);
            visual.transform.localScale = new Vector3(.8f, .8f, 1f);
            target = visual.AddComponent<SpriteRenderer>();
            Type type = typeof(CharacterArtDefinition).Assembly.GetType("HuntrX.Presentation.Art.CharacterSpritePlayback2D");
            Assert.That(type, Is.Not.Null, "State playback is not implemented.");
            playback = (Behaviour)visual.AddComponent(type);
            set = ScriptableObject.CreateInstance<CharacterSpriteSet2D>();
            owned.Add(set);
            Set("characterName", "Rumi"); Set("pixelsPerUnit", 1f); Set("worldScale", 2f); Set("footAnchor", Vector2.zero);
            Set("states", new[]
            {
                new SpriteAnimationState2D("Idle", new SpriteAnimationClip2D(new[] { first, second }, new[] { .1f, .3f }, true)),
                new SpriteAnimationState2D("Run", new SpriteAnimationClip2D(new[] { first, second }, new[] { .2f, .2f }, true))
            });
            Assert.That((bool)Call("TryBind", target, set), Is.True);
        }

        private Sprite MakeFrame(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero, 1f);
            owned.Add(sprite); return sprite;
        }
        private void Set(string name, object value) => typeof(CharacterSpriteSet2D)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(set, value);
        private object Call(string name, params object[] values) => playback.GetType().GetMethod(name).Invoke(playback, values);
        private float Elapsed => (float)playback.GetType().GetProperty("Elapsed").GetValue(playback);

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        [Test]
        public void ChangingStateResetsClockButRepeatedStateDoesNotRestartMotion()
        {
            Call("Tick", .11f);
            Assert.That(target.sprite, Is.SameAs(second));
            Assert.That((bool)Call("SetState", "Run", false), Is.True);
            Assert.That(Elapsed, Is.Zero);
            Assert.That(target.sprite, Is.SameAs(first));
            Call("Tick", .21f);
            Assert.That((bool)Call("SetState", "Run", false), Is.True);
            Assert.That(Elapsed, Is.EqualTo(.21f).Within(.0001f));
            Assert.That(target.sprite, Is.SameAs(second));
        }

        [Test]
        public void ExplicitRestartBeginsSameStateAtFirstFrame()
        {
            Call("Tick", .11f);
            Call("SetState", "Idle", true);
            Assert.That(Elapsed, Is.Zero);
            Assert.That(target.sprite, Is.SameAs(first));
        }

        [Test]
        public void UnknownStateDoesNotClearValidPresentation()
        {
            Call("Tick", .11f);
            Assert.That((bool)Call("SetState", "Unknown", false), Is.False);
            Assert.That(target.sprite, Is.SameAs(second));
            Assert.That(Elapsed, Is.EqualTo(.11f).Within(.0001f));
        }

        [Test]
        public void DisableReleasesOwnedSprite()
        {
            playback.enabled = false;
            Assert.That(target.sprite, Is.Null);
            playback.enabled = true;
            Call("Tick", .2f);
            Assert.That(target.sprite, Is.Null, "Re-enable must not resurrect the old actor binding.");
        }

        [Test]
        public void DisablePreservesSpriteReplacedByAnotherOwner()
        {
            target.sprite = external;
            playback.enabled = false;
            Assert.That(target.sprite, Is.SameAs(external));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidDeltaCannotCorruptCurrentClock(float delta)
        {
            Call("Tick", .11f);
            Call("Tick", delta);
            Assert.That(Elapsed, Is.EqualTo(.11f).Within(.0001f));
            Assert.That(target.sprite, Is.SameAs(second));
        }

        [Test]
        public void PlaybackDoesNotRescalePhysicsOrVisualPerFrame()
        {
            Vector3 rootScale = actor.transform.localScale;
            Vector3 visualScale = target.transform.localScale;
            Call("Tick", .11f); Call("SetState", "Run", false); Call("Tick", .21f);
            Assert.That(actor.transform.localScale, Is.EqualTo(rootScale));
            Assert.That(target.transform.localScale, Is.EqualTo(visualScale));
            Assert.That(actor.GetComponent<BoxCollider2D>().size, Is.EqualTo(Vector2.one));
        }
    }
}
