using System;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class CharacterSpriteCatalogTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Texture2D texture;
        private Sprite frame;

        [SetUp]
        public void SetUp()
        {
            texture = new Texture2D(4, 4);
            owned.Add(texture);
            frame = MakeFrame(new Vector2(1, 1), Vector2.zero, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        private Sprite MakeFrame(Vector2 size, Vector2 pivot, float ppu)
        {
            Sprite result = Sprite.Create(texture, new Rect(Vector2.zero, size), pivot, ppu);
            owned.Add(result);
            return result;
        }

        private static Type RuntimeType(string name)
        {
            Type result = typeof(CharacterArtDefinition).Assembly.GetType("HuntrX.Data." + name);
            Assert.That(result, Is.Not.Null, "Explicit sprite catalog is not implemented: " + name);
            return result;
        }

        private ScriptableObject Catalog(string[] ids, SpriteAnimationClip2D[] clips)
        {
            Type type = RuntimeType("CharacterSpriteSet2D");
            Type entryType = RuntimeType("SpriteAnimationState2D");
            var result = ScriptableObject.CreateInstance(type);
            owned.Add(result);
            Set(result, "characterName", "Rumi");
            Set(result, "pixelsPerUnit", 1f);
            Set(result, "worldScale", 1f);
            Set(result, "footAnchor", Vector2.zero);
            Array states = Array.CreateInstance(entryType, ids.Length);
            for (int i = 0; i < ids.Length; i++)
                states.SetValue(Activator.CreateInstance(entryType, new object[] { ids[i], clips[i] }), i);
            Set(result, "states", states);
            return result;
        }

        private static void Set(object owner, string field, object value) =>
            owner.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(owner, value);
        private static bool Valid(object catalog) => (bool)catalog.GetType().GetProperty("IsValid").GetValue(catalog);
        private static bool Lookup(object catalog, string state, out SpriteAnimationClip2D clip)
        {
            object[] arguments = { state, null };
            bool result = (bool)catalog.GetType().GetMethod("TryGetClip").Invoke(catalog, arguments);
            clip = (SpriteAnimationClip2D)arguments[1];
            return result;
        }
        private SpriteAnimationClip2D Clip(Sprite sprite) => new SpriteAnimationClip2D(new[] { sprite }, new[] { .1f }, true);

        [Test]
        public void ValidStateResolvesExplicitClipAndMissingStateDoesNotInventFallback()
        {
            var idle = Clip(frame);
            var catalog = Catalog(new[] { "Idle" }, new[] { idle });
            Assert.That(Valid(catalog), Is.True);
            Assert.That(Lookup(catalog, "Idle", out var resolved), Is.True);
            Assert.That(resolved, Is.SameAs(idle));
            Assert.That(Lookup(catalog, "Unknown", out var absent), Is.False);
            Assert.That(absent, Is.Null);
        }

        [Test]
        public void DuplicateStateRejectsWholeCatalogInsteadOfPickingAnArbitraryClip()
        {
            var catalog = Catalog(new[] { "Idle", "Idle" }, new[] { Clip(frame), Clip(frame) });
            Assert.That(Valid(catalog), Is.False);
            Assert.That(Lookup(catalog, "Idle", out var resolved), Is.False);
            Assert.That(resolved, Is.Null);
        }

        [TestCase(2f, 1f, 0f, 1f)]
        [TestCase(1f, 1f, .5f, 1f)]
        [TestCase(1f, 1f, 0f, 2f)]
        public void DifferentCanvasPivotOrPpuRejectsLayoutThatWouldMakeFeetJump(float width, float height, float pivot, float ppu)
        {
            var other = MakeFrame(new Vector2(width, height), new Vector2(pivot, pivot), ppu);
            var catalog = Catalog(new[] { "Idle", "Run" }, new[] { Clip(frame), Clip(other) });
            Assert.That(Valid(catalog), Is.False);
        }

        [Test]
        public void LayoutValidationChecksEveryFrameNotOnlyFirstPose()
        {
            var other = MakeFrame(new Vector2(2f, 1f), Vector2.zero, 1f);
            var catalog = Catalog(new[] { "Run" }, new[] { new SpriteAnimationClip2D(new[] { frame, other }, new[] { .1f, .1f }, true) });
            Assert.That(Valid(catalog), Is.False);
        }

        [Test]
        public void MissingClipRejectsCatalog()
        {
            var catalog = Catalog(new[] { "Idle" }, new SpriteAnimationClip2D[] { null });
            Assert.That(Valid(catalog), Is.False);
        }
    }
}
