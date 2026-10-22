using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.EditMode
{
    public sealed class CharacterDeathAnimationAssetTests
    {
        private readonly List<Object> owned = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
            {
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            }
            owned.Clear();
        }

        [TestCase("Rumi")]
        [TestCase("Mira")]
        public void RuntimeSpriteSetLoadsFourDistinct512DeathFramesAlignedToIdleFootBaseline(string heroKey)
        {
            LogAssert.NoUnexpectedReceived();

            Texture2D idle0 = Resources.Load<Texture2D>("Art/Animations/" + heroKey + "/Idle/0");
            Assert.That(idle0, Is.Not.Null, $"Missing Idle/0 texture for {heroKey}.");
            Assert.That(idle0.width, Is.EqualTo(512));
            Assert.That(idle0.height, Is.EqualTo(512));

            MeasureAlphaBounds(idle0, out int idleMinX, out int idleMinY, out int idleMaxX, out _, out int idleBorderCount);
            Assert.That(idleBorderCount, Is.EqualTo(0), $"{heroKey}/Idle/0 must not clip at canvas borders.");
            float idleCenterX = (idleMinX + idleMaxX) * 0.5f;

            MethodInfo builder = typeof(CharacterArtRenderer2D).GetMethod(
                "BuildRuntimeSpriteSet",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(string), typeof(float), typeof(List<Object>) },
                null);
            Assert.That(builder, Is.Not.Null, "CharacterArtRenderer2D.BuildRuntimeSpriteSet was not found.");

            var spriteSet = (CharacterSpriteSet2D)builder.Invoke(null, new object[] { heroKey, 2.25f, owned });
            Assert.That(spriteSet, Is.Not.Null);
            Assert.That(spriteSet.IsValid, Is.True);

            Assert.That(spriteSet.TryGetClip("Idle", out SpriteAnimationClip2D idleClip), Is.True);
            Assert.That(spriteSet.TryGetClip("Death", out SpriteAnimationClip2D deathClip), Is.True);
            Assert.That(deathClip, Is.Not.Null);
            Assert.That(deathClip.IsValid, Is.True);
            Assert.That(deathClip.Loop, Is.False, $"{heroKey} Death clip must be non-looping.");
            Assert.That(deathClip.FrameCount, Is.EqualTo(4),
                $"{heroKey} Death clip must load all 4 authored frames instead of falling back to 1-frame Idle.");

            var seenTextures = new HashSet<Texture2D>();
            for (int i = 0; i < 4; i++)
            {
                Texture2D rawFrame = Resources.Load<Texture2D>("Art/Animations/" + heroKey + "/Death/" + i);
                Assert.That(rawFrame, Is.Not.Null, $"Missing {heroKey}/Death/{i} texture.");
                Assert.That(rawFrame.width, Is.EqualTo(512), $"{heroKey}/Death/{i} width must match 512 Idle canvas.");
                Assert.That(rawFrame.height, Is.EqualTo(512), $"{heroKey}/Death/{i} height must match 512 Idle canvas.");
                Assert.That(rawFrame, Is.Not.SameAs(idleClip.FrameAt(0f).texture),
                    $"{heroKey}/Death/{i} must not be the Idle/0 fallback texture.");
                Assert.That(seenTextures.Add(rawFrame), Is.True, $"{heroKey}/Death/{i} must be a distinct frame texture.");

                MeasureAlphaBounds(rawFrame, out int minX, out int minY, out int maxX, out int maxY, out int borderAlphaCount);
                Assert.That(borderAlphaCount, Is.EqualTo(0),
                    $"{heroKey}/Death/{i} must have 0 pixels with alpha > 8 on the 512x512 canvas borders.");
                Assert.That(minY, Is.EqualTo(idleMinY).Within(1),
                    $"{heroKey}/Death/{i} foot baseline (minY={minY}) must match {heroKey}/Idle/0 foot baseline (minY={idleMinY}).");

                float centerX = (minX + maxX) * 0.5f;
                Assert.That(centerX, Is.EqualTo(idleCenterX).Within(2.5f),
                    $"{heroKey}/Death/{i} horizontal center ({centerX:F1}) must align with {heroKey}/Idle/0 ({idleCenterX:F1}).");
                Assert.That(maxY, Is.GreaterThan(minY + 200),
                    $"{heroKey}/Death/{i} must contain a full character silhouette.");

                Sprite clipSprite = deathClip.FrameAt(i * 0.12f + 0.01f);
                Assert.That(clipSprite, Is.Not.Null);
                Assert.That(clipSprite.texture, Is.SameAs(rawFrame));
                Assert.That(clipSprite.rect.size, Is.EqualTo(new Vector2(512f, 512f)));
            }
        }

        private static void MeasureAlphaBounds(
            Texture2D tex,
            out int minX,
            out int minY,
            out int maxX,
            out int maxY,
            out int borderAlphaAbove8)
        {
            int w = tex.width;
            int h = tex.height;
            Color32[] pixels = tex.GetPixels32();
            minX = w;
            minY = h;
            maxX = -1;
            maxY = -1;
            borderAlphaAbove8 = 0;

            for (int y = 0; y < h; y++)
            {
                int rowOffset = y * w;
                for (int x = 0; x < w; x++)
                {
                    byte a = pixels[rowOffset + x].a;
                    if ((x == 0 || x == w - 1 || y == 0 || y == h - 1) && a > 8)
                    {
                        borderAlphaAbove8++;
                    }

                    if (a > 10)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }
        }
    }
}
