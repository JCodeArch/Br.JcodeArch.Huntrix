using HuntrX.Gameplay.Narrative;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class AirplaneCutsceneArtAssetTests
    {
        [Test]
        public void UnknownSequenceDoesNotResolveToVictoryExit()
        {
            var unknown = (AirplaneCutsceneSequenceId)999;
            Assert.That(AirplaneCutsceneArtCatalog.GetSequence(unknown), Is.Null);
            Assert.That(AirplaneCutsceneArtCatalog.TryResolveFrameAtTime(unknown, 0f,
                out _, out _), Is.False);
            Assert.That(AirplaneCutsceneArtCatalog.TryLoadSprites(unknown, out _), Is.False);
        }

        [TestCase(AirplaneCutsceneSequenceId.CockpitCaptainEntry, 3.50f)]
        [TestCase(AirplaneCutsceneSequenceId.PostVictoryDramaticExit, 3.80f)]
        public void CatalogDefinesContiguousFourFrameSequenceAndTimings(
            AirplaneCutsceneSequenceId sequenceId,
            float expectedTotalSeconds)
        {
            AirplaneCutsceneSequenceSpec spec = AirplaneCutsceneArtCatalog.GetSequence(sequenceId);
            Assert.That(spec, Is.Not.Null);
            Assert.That(spec.Frames, Is.Not.Null);
            Assert.That(spec.Frames.Length, Is.EqualTo(4), "Each Stage 1 cutscene sequence must have 4 storyboard frames.");
            Assert.That(spec.TargetWidthPixels, Is.EqualTo(1376));
            Assert.That(spec.TargetHeightPixels, Is.EqualTo(768));
            Assert.That(spec.RecommendedCaptainToHeroScaleRatio, Is.GreaterThanOrEqualTo(1.25f));
            Assert.That(spec.TotalDurationSeconds, Is.EqualTo(expectedTotalSeconds).Within(0.01f));

            float cursor = 0f;
            for (int i = 0; i < spec.Frames.Length; i++)
            {
                AirplaneCutsceneFrameSpec f = spec.Frames[i];
                Assert.That(f.FrameIndex, Is.EqualTo(i));
                Assert.That(f.StartTimeSeconds, Is.EqualTo(cursor).Within(0.001f));
                Assert.That(f.DurationSeconds, Is.GreaterThanOrEqualTo(0.75f).And.LessThanOrEqualTo(1.25f));
                Assert.That(f.CrossfadeDurationSeconds, Is.GreaterThan(0f).And.LessThan(f.DurationSeconds));
                cursor += f.DurationSeconds;

                bool resolved = AirplaneCutsceneArtCatalog.TryResolveFrameAtTime(
                    sequenceId,
                    f.StartTimeSeconds + f.DurationSeconds * 0.5f,
                    out AirplaneCutsceneFrameSpec midFrame,
                    out float norm);
                Assert.That(resolved, Is.True);
                Assert.That(midFrame.FrameIndex, Is.EqualTo(i));
                Assert.That(norm, Is.EqualTo(0.5f).Within(0.02f));
            }
        }

        [TestCase(AirplaneCutsceneSequenceId.CockpitCaptainEntry)]
        [TestCase(AirplaneCutsceneSequenceId.PostVictoryDramaticExit)]
        public void CutsceneFramesImportAsHighRes16By9SpritesAndTexturesWithExpectedPaletteAndDistinctFrames(
            AirplaneCutsceneSequenceId sequenceId)
        {
            Assert.That(AirplaneCutsceneArtCatalog.TryLoadTextures(sequenceId, out Texture2D[] textures), Is.True,
                $"Expected all 4 Texture2D assets for {sequenceId} to load from Resources.");
            Assert.That(AirplaneCutsceneArtCatalog.TryLoadSprites(sequenceId, out Sprite[] sprites), Is.True,
                $"Expected all 4 Sprite assets for {sequenceId} to load from Resources.");

            Assert.That(textures.Length, Is.EqualTo(4));
            Assert.That(sprites.Length, Is.EqualTo(4));

            AirplaneCutsceneSequenceSpec spec = AirplaneCutsceneArtCatalog.GetSequence(sequenceId);
            Color32[][] sampledFrames = new Color32[4][];

            for (int i = 0; i < 4; i++)
            {
                Texture2D tex = textures[i];
                Sprite spr = sprites[i];
                Assert.That(tex, Is.Not.Null, $"Missing Texture2D at frame {i} ({spec.Frames[i].ResourcePath}).");
                Assert.That(spr, Is.Not.Null, $"Missing Sprite at frame {i} ({spec.Frames[i].ResourcePath}).");
                Assert.That(tex.width, Is.EqualTo(AirplaneCutsceneArtCatalog.StandardFrameWidth),
                    $"Frame {i} width must match 1376px 16:9 cutscene standard.");
                Assert.That(tex.height, Is.EqualTo(AirplaneCutsceneArtCatalog.StandardFrameHeight),
                    $"Frame {i} height must match 768px 16:9 cutscene standard.");
                Assert.That(spr.rect.width, Is.EqualTo(1376f).Within(1f));
                Assert.That(spr.rect.height, Is.EqualTo(768f).Within(1f));

                string assetPath = AssetDatabase.GetAssetPath(tex);
                Assert.That(assetPath, Does.StartWith("Assets/Resources/Art/Cutscenes/"));
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Assert.That(importer, Is.Not.Null);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));

                Texture2D readable = LoadReadableCopy(assetPath);
                try
                {
                    Color32[] pixels = readable.GetPixels32();
                    sampledFrames[i] = pixels;

                    int cyanBluePixels = 0;
                    int warmAccentPixels = 0;
                    for (int p = 0; p < pixels.Length; p += 16)
                    {
                        Color32 c = pixels[p];
                        if (c.b > 150 && c.g > 110 && c.b > c.r + 30)
                            cyanBluePixels++;
                        if (c.r > 150 && c.r > c.b + 25)
                            warmAccentPixels++;
                    }

                    Assert.That(cyanBluePixels, Is.GreaterThan(150),
                        $"Frame {i} of {sequenceId} must preserve Rumi's cyan/blue Saingeom sword / Honmoon palette signature.");
                    Assert.That(warmAccentPixels, Is.GreaterThan(150),
                        $"Frame {i} of {sequenceId} must preserve warm crimson/amber/mustard contrast for character/enemy readability.");
                }
                finally
                {
                    Object.DestroyImmediate(readable);
                }
            }

            // Ensure all 4 frames in the sequence are genuinely distinct illustrations (not duplicated placeholders)
            for (int i = 0; i < 3; i++)
            {
                Color32[] a = sampledFrames[i];
                Color32[] b = sampledFrames[i + 1];
                int diffCount = 0;
                int samples = 0;
                for (int p = 0; p < a.Length; p += 64)
                {
                    samples++;
                    if (Mathf.Abs(a[p].r - b[p].r) + Mathf.Abs(a[p].g - b[p].g) + Mathf.Abs(a[p].b - b[p].b) > 24)
                        diffCount++;
                }

                float diffRatio = diffCount / (float)Mathf.Max(1, samples);
                Assert.That(diffRatio, Is.GreaterThan(0.15f),
                    $"Frames {i} and {i + 1} of {sequenceId} must be visually distinct keyframes (diffRatio={diffRatio:F2}).");
            }
        }

        private static Texture2D LoadReadableCopy(string assetPath)
        {
            byte[] raw = System.IO.File.ReadAllBytes(assetPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(raw, false);
            return tex;
        }
    }
}
