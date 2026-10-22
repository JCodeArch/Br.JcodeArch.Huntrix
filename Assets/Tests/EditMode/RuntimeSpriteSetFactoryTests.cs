using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using HuntrX.Data;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.EditMode
{
    public sealed class RuntimeSpriteSetFactoryTests
    {
        private readonly List<Object> owned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        private Texture2D Texture(int width, int height)
        {
            var texture = new Texture2D(width, height);
            owned.Add(texture);
            return texture;
        }

        private SpriteAnimationState2D State(string id, Texture2D texture, float ppu = 1f)
        {
            Sprite frame = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, ppu);
            owned.Add(frame);
            return new SpriteAnimationState2D(id, new SpriteAnimationClip2D(new[] { frame }, new[] { .1f }, true));
        }

        private bool Create(string name, float ppu, float scale, Vector2 anchor, SpriteAnimationState2D[] states,
            out CharacterSpriteSet2D result)
        {
            MethodInfo factory = typeof(CharacterSpriteSet2D).GetMethod("TryCreate", BindingFlags.Public | BindingFlags.Static);
            Assert.That(factory, Is.Not.Null, "Validated runtime sprite-set factory is not implemented.");
            object[] args = { name, ppu, scale, anchor, states, null };
            bool accepted = (bool)factory.Invoke(null, args);
            result = (CharacterSpriteSet2D)args[5];
            if (result != null) owned.Add(result);
            return accepted;
        }

        [Test]
        public void FactoryAcceptsRunOnlyAndCopiesCallerStateArray()
        {
            SpriteAnimationState2D run = State("Run", Texture(2, 2));
            var states = new[] { run };
            Assert.That(Create("Rumi", 1f, 2f, Vector2.zero, states, out var result), Is.True);
            states[0] = null;
            Assert.That(result.IsValid, Is.True, "Caller array mutation must not invalidate an accepted catalog.");
            Assert.That(result.TryGetClip("Run", out var resolved), Is.True);
            Assert.That(resolved, Is.SameAs(run.Clip));
            Assert.That(result.TryGetClip("Idle", out _), Is.False, "The factory must preserve the existing Run-only contract.");
            Assert.That(result.WorldScale, Is.EqualTo(2f));
        }

        [TestCase("name")]
        [TestCase("ppu")]
        [TestCase("scale")]
        [TestCase("anchor")]
        [TestCase("duplicate")]
        [TestCase("missing")]
        [TestCase("canvas")]
        [TestCase("frame-ppu")]
        public void RejectedFactoryInputLeavesNoCandidateAsset(string invalid)
        {
            string name = invalid == "name" ? " " : "Rumi";
            float ppu = invalid == "ppu" ? 0f : 1f;
            float scale = invalid == "scale" ? float.NaN : 1f;
            Vector2 anchor = invalid == "anchor" ? new Vector2(2f, 0f) : Vector2.zero;
            var states = new[] { State("Idle", Texture(2, 2)) };
            if (invalid == "duplicate") states = new[] { states[0], states[0] };
            if (invalid == "missing") states = null;
            if (invalid == "canvas") states = new[] { states[0], State("Run", Texture(4, 2)) };
            if (invalid == "frame-ppu") states = new[] { states[0], State("Run", Texture(2, 2), 2f) };
            int previousCandidates = Resources.FindObjectsOfTypeAll<CharacterSpriteSet2D>().Length;
            Assert.That(Create(name, ppu, scale, anchor, states, out var rejected), Is.False);
            Assert.That(rejected, Is.Null);
            Assert.That(Resources.FindObjectsOfTypeAll<CharacterSpriteSet2D>().Length, Is.EqualTo(previousCandidates),
                "Rejected construction must not leave a candidate ScriptableObject alive.");
        }

        private CharacterSpriteSet2D Build(Func<string, Texture2D> load, List<Object> assets)
        {
            MethodInfo builder = typeof(CharacterArtRenderer2D).GetMethod("BuildRuntimeSpriteSet",
                BindingFlags.NonPublic | BindingFlags.Static, null,
                new[] { typeof(string), typeof(float), typeof(List<Object>), typeof(Func<string, Texture2D>) }, null);
            Assert.That(builder, Is.Not.Null, "Runtime texture loading must expose a deterministic dimension-validation seam.");
            return (CharacterSpriteSet2D)builder.Invoke(null, new object[] { "RuntimeDimensionProbe", 2.25f, assets, load });
        }

        [TestCase("Attack", 1024, 512, 0)]
        [TestCase("Attack", 512, 1024, 1)]
        [TestCase("Death", 384, 512, 0)]
        [TestCase("Death", 256, 512, 1)]
        public void IncompatibleClipUsesIntactIdleFallbackWithoutDiscardingOtherStatesOrPartialSprites(
            string rejectedState, int width, int height, int badIndex)
        {
            Texture2D idle = Texture(512, 512);
            Texture2D run = Texture(512, 512);
            Texture2D partial = Texture(512, 512);
            Texture2D incompatible = Texture(width, height);
            Texture2D sentinel = Texture(1, 1);
            var assets = new List<Object> { sentinel };
            LogAssert.Expect(LogType.Warning, new Regex(".*Art/Animations/RuntimeDimensionProbe/" + rejectedState + "/" + badIndex +
                ".*" + width + "x" + height + ".*512x512.*Idle fallback.*"));
            CharacterSpriteSet2D result = null;
            try
            {
                result = Build(path => path.EndsWith("/Idle/0") ? idle : path.EndsWith("/Run/0") ? run :
                    path.EndsWith("/" + rejectedState + "/" + badIndex) ? incompatible :
                    badIndex == 1 && path.EndsWith("/" + rejectedState + "/0") ? partial : null, assets);
                Assert.That(result, Is.Not.Null, "One incompatible state must not discard the character's entire presentation.");
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.TryGetClip(rejectedState, out var fallback), Is.True);
                Assert.That(fallback.FrameCount, Is.EqualTo(1), "Do not keep the compatible prefix of a rejected clip.");
                Assert.That(fallback.FrameAt(0f).texture, Is.SameAs(idle));
                Assert.That(fallback.FrameAt(0f).rect.size, Is.EqualTo(new Vector2(512, 512)), "Fallback must retain the full Idle canvas.");
                Assert.That(result.TryGetClip("Run", out var preserved), Is.True);
                Assert.That(preserved.FrameAt(0f).texture, Is.SameAs(run), "A valid state must keep its authored artwork.");
                Assert.That(assets[0], Is.SameAs(sentinel), "Resources owned before this attempt must remain untouched.");
                Assert.That(Resources.FindObjectsOfTypeAll<Sprite>().Where(s => s.texture == partial || s.texture == incompatible), Is.Empty,
                    "An incompatible clip must never allocate native sprites for either its valid prefix or its bad frame.");
            }
            finally
            {
                foreach (Object asset in assets) if (asset != sentinel && asset != null) Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void Existing512CanvasFallbackStillBuildsValidFullSizeFrames()
        {
            Texture2D idle = Texture(512, 512);
            var assets = new List<Object>();
            try
            {
                CharacterSpriteSet2D result = Build(path => path.EndsWith("/Idle/0") ? idle : null, assets);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.IsValid, Is.True);
                Assert.That(result.TryGetClip("Attack", out var clip), Is.True);
                Assert.That(clip.FrameAt(0f).rect.size, Is.EqualTo(new Vector2(512, 512)));
                Assert.That(clip.FrameAt(0f).texture, Is.SameAs(idle));
                Assert.That(result.PixelsPerUnit, Is.EqualTo(512f / 2.25f).Within(.001f));
                Assert.That(assets, Does.Contain(result), "Successful generated assets transfer to the caller's ownership.");
            }
            finally
            {
                foreach (Object asset in assets) if (asset != null) Object.DestroyImmediate(asset);
            }
        }
    }
}
