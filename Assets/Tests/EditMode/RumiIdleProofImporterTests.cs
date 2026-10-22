using System;
using System.IO;
using System.Linq;
using HuntrX.Data;
using HuntrX.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class RumiIdleProofImporterTests
    {
        private const string TempFolderPrefix = "Assets/Art/HD/Rumi/IdleProofV2_Test_";

        private static void AssertSpriteAsset(Sprite actual, Sprite expected, string message)
        {
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(actual, out string actualGuid, out long actualId), Is.True, message);
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(expected, out string expectedGuid, out long expectedId), Is.True, message);
            Assert.That(actualGuid, Is.EqualTo(expectedGuid), message);
            Assert.That(actualId, Is.EqualTo(expectedId), message);
        }

        [Test]
        public void ImportDefault_CopiesIntactPng_SlicesExactFourBottomLeftFullRectFrames_AndBuildsValidIdleOnlyCatalog()
        {
            RumiIdleProofImportResult result = RumiIdleProofImporter.ImportDefault();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.TextureAssetPath, Is.EqualTo("Assets/Art/HD/Rumi/IdleProofV2/rumi-idle-proof-v2.png"));
            Assert.That(result.CatalogAssetPath, Is.EqualTo("Assets/Art/HD/Rumi/IdleProofV2/RumiIdleProofV2.asset"));
            Assert.That(result.Sha256, Is.EqualTo(RumiIdleProofImporter.ExpectedSha256).IgnoreCase);

            string sourcePath = RumiIdleProofImporter.ResolveDefaultSourcePngPath();
            string importedDiskPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                result.TextureAssetPath.Replace('/', Path.DirectorySeparatorChar));
            byte[] sourceBytes = File.ReadAllBytes(sourcePath);
            byte[] importedBytes = File.ReadAllBytes(importedDiskPath);
            Assert.That(importedBytes.Length, Is.EqualTo(sourceBytes.Length));
            Assert.That(RumiIdleProofImporter.ComputeSha256Hex(importedBytes), Is.EqualTo(RumiIdleProofImporter.ExpectedSha256));

            Assert.That(result.Texture, Is.Not.Null);
            Assert.That(result.Texture.width, Is.EqualTo(1024));
            Assert.That(result.Texture.height, Is.EqualTo(1536));

            var importer = AssetImporter.GetAtPath(result.TextureAssetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Multiple));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(RumiIdleProofImporter.DefaultPixelsPerUnit).Within(0.001f));
            Assert.That(importer.isReadable, Is.False);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.userData, Is.EqualTo(RumiIdleProofImporter.ImporterUserDataTag));

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.That(settings.spriteMeshType, Is.EqualTo(SpriteMeshType.FullRect));
            Assert.That(settings.spriteAlignment, Is.EqualTo((int)SpriteAlignment.Custom));

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            Assert.That(dataProvider, Is.Not.Null);
            dataProvider.InitSpriteEditorDataProvider();
            SpriteRect[] providerRects = dataProvider.GetSpriteRects();
            Assert.That(providerRects, Is.Not.Null);
            Assert.That(providerRects.Length, Is.EqualTo(4));

            Rect[] expectedRects =
            {
                new Rect(64f, 776f, 448f, 744f),
                new Rect(544f, 776f, 448f, 744f),
                new Rect(64f, 8f, 448f, 744f),
                new Rect(544f, 8f, 448f, 744f)
            };
            Vector2 expectedNormalizedPivot = new Vector2(0.60f, 0.03629f);
            Vector2 expectedPixelPivot = new Vector2(448f * 0.60f, 744f * 0.03629f);

            Assert.That(result.Sprites, Is.Not.Null);
            Assert.That(result.Sprites.Length, Is.EqualTo(4));

            for (int i = 0; i < 4; i++)
            {
                Assert.That(providerRects[i].name, Is.EqualTo($"rumi_idle_0{i}"));
                Assert.That(providerRects[i].rect, Is.EqualTo(expectedRects[i]));
                Assert.That(providerRects[i].alignment, Is.EqualTo(SpriteAlignment.Custom));
                Assert.That(Vector2.Distance(providerRects[i].pivot, expectedNormalizedPivot), Is.LessThanOrEqualTo(0.0001f));

                Sprite sprite = result.Sprites[i];
                Assert.That(sprite, Is.Not.Null);
                Assert.That(sprite.name, Is.EqualTo($"rumi_idle_0{i}"));
                Assert.That(sprite.rect, Is.EqualTo(expectedRects[i]));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(RumiIdleProofImporter.DefaultPixelsPerUnit).Within(0.001f));
                Assert.That(Vector2.Distance(sprite.pivot, expectedPixelPivot), Is.LessThanOrEqualTo(0.01f));

                Vector2 normalizedPivot = new Vector2(
                    sprite.pivot.x / sprite.rect.width,
                    sprite.pivot.y / sprite.rect.height);
                Assert.That(normalizedPivot.x, Is.EqualTo(expectedNormalizedPivot.x).Within(0.0001f));
                Assert.That(normalizedPivot.y, Is.EqualTo(expectedNormalizedPivot.y).Within(0.0001f));
                Assert.That(sprite.vertices.Length, Is.EqualTo(4));
            }

            CharacterSpriteSet2D catalog = result.Catalog;
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Id, Is.EqualTo(RumiIdleProofImporter.CatalogOwnershipId));
            Assert.That(catalog.CharacterName, Is.EqualTo("Rumi"));
            Assert.That(catalog.PixelsPerUnit, Is.EqualTo(RumiIdleProofImporter.DefaultPixelsPerUnit).Within(0.001f));
            Assert.That(catalog.WorldScale, Is.EqualTo(RumiIdleProofImporter.DefaultWorldScale).Within(0.001f));
            Assert.That(Vector2.Distance(catalog.FootAnchor, expectedNormalizedPivot), Is.LessThanOrEqualTo(0.0001f));

            AssetImporter catalogImporter = AssetImporter.GetAtPath(result.CatalogAssetPath);
            Assert.That(catalogImporter, Is.Not.Null);
            Assert.That(catalogImporter.userData, Is.EqualTo(RumiIdleProofImporter.ImporterUserDataTag));

            Assert.That(catalog.TryGetClip("Idle", out SpriteAnimationClip2D idleClip), Is.True);
            Assert.That(idleClip, Is.Not.Null);
            Assert.That(idleClip.IsValid, Is.True);
            Assert.That(idleClip.Loop, Is.True);
            Assert.That(idleClip.FrameCount, Is.EqualTo(4));
            Assert.That(idleClip.Duration, Is.EqualTo(2.9f).Within(.001f), "Idle should not blink every 0.6 seconds.");
            AssertSpriteAsset(idleClip.FrameAt(1.5f), result.Sprites[0], "Hold the relaxed open-eye pose.");
            AssertSpriteAsset(idleClip.FrameAt(2.45f), result.Sprites[2], "Keep the authored blink brief.");
            AssertSpriteAsset(idleClip.FrameAt(2.55f), result.Sprites[3], "Eyes must reopen after the blink window.");

            Assert.That(catalog.TryGetClip("Run", out _), Is.False);
            Assert.That(catalog.TryGetClip("Jump", out _), Is.False);
            Assert.That(catalog.TryGetClip("Fall", out _), Is.False);
            Assert.That(catalog.TryGetClip("Attack", out _), Is.False);
            Assert.That(catalog.TryGetClip("Dash", out _), Is.False);

            using (var catalogSo = new SerializedObject(catalog))
            {
                SerializedProperty statesProp = catalogSo.FindProperty("states");
                Assert.That(statesProp, Is.Not.Null);
                Assert.That(statesProp.arraySize, Is.EqualTo(1));
            }
        }

        [Test]
        public void Import_SupportsConfigurablePixelsPerUnitWorldScaleAndInitialFrameDurations()
        {
            string tempFolder = CreateUniqueTempAssetFolderName("CustomParams");
            try
            {
                float[] customDurations = { 0.10f, 0.12f, 0.16f, 0.22f };
                RumiIdleProofImportResult result = RumiIdleProofImporter.Import(
                    targetFolder: tempFolder,
                    pixelsPerUnit: 256f,
                    frameDurations: customDurations,
                    worldScale: 1.25f);

                Assert.That(result.Catalog.IsValid, Is.True);
                Assert.That(result.Catalog.PixelsPerUnit, Is.EqualTo(256f).Within(0.001f));
                Assert.That(result.Catalog.WorldScale, Is.EqualTo(1.25f).Within(0.001f));

                for (int i = 0; i < result.Sprites.Length; i++)
                {
                    Assert.That(result.Sprites[i].pixelsPerUnit, Is.EqualTo(256f).Within(0.001f));
                }

                Assert.That(result.Catalog.TryGetClip("Idle", out SpriteAnimationClip2D clip), Is.True);
                Assert.That(clip.Duration, Is.EqualTo(0.60f).Within(0.0001f));
                Assert.That(clip.FrameAt(0.05f), Is.SameAs(result.Sprites[0]));
                Assert.That(clip.FrameAt(0.15f), Is.SameAs(result.Sprites[1]));
                Assert.That(clip.FrameAt(0.30f), Is.SameAs(result.Sprites[2]));
                Assert.That(clip.FrameAt(0.50f), Is.SameAs(result.Sprites[3]));
            }
            finally
            {
                DeleteTempAssetFolderIfExists(tempFolder);
            }
        }

        [Test]
        public void Import_IsIdempotentAcrossRepeatedRuns_AndPreservesGuidLocalFileIdAndBytesAfterForcedReimport()
        {
            RumiIdleProofImportResult first = RumiIdleProofImporter.ImportDefault();
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(first.Catalog, out string firstCatalogGuid, out long firstCatalogLocalId), Is.True);

            var firstSpriteIds = new (string guid, long localId)[4];
            for (int i = 0; i < 4; i++)
            {
                Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(first.Sprites[i], out string g, out long lid), Is.True);
                firstSpriteIds[i] = (g, lid);
            }

            string textureMetaPath = Path.Combine(Directory.GetCurrentDirectory(), (first.TextureAssetPath + ".meta").Replace('/', Path.DirectorySeparatorChar));
            string catalogDiskPath = Path.Combine(Directory.GetCurrentDirectory(), first.CatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            string firstMetaContent = File.ReadAllText(textureMetaPath);
            string firstCatalogContent = File.ReadAllText(catalogDiskPath);

            AssetDatabase.ImportAsset(first.TextureAssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(first.CatalogAssetPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            RumiIdleProofImportResult second = RumiIdleProofImporter.ImportDefault();
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(second.Catalog, out string secondCatalogGuid, out long secondCatalogLocalId), Is.True);
            Assert.That(secondCatalogGuid, Is.EqualTo(firstCatalogGuid));
            Assert.That(secondCatalogLocalId, Is.EqualTo(firstCatalogLocalId));

            for (int i = 0; i < 4; i++)
            {
                Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(second.Sprites[i], out string g, out long lid), Is.True);
                Assert.That(g, Is.EqualTo(firstSpriteIds[i].guid));
                Assert.That(lid, Is.EqualTo(firstSpriteIds[i].localId));
            }

            Assert.That(File.ReadAllText(textureMetaPath), Is.EqualTo(firstMetaContent));
            Assert.That(File.ReadAllText(catalogDiskPath), Is.EqualTo(firstCatalogContent));

            Sprite[] allSpritesAtPath = AssetDatabase.LoadAllAssetsAtPath(second.TextureAssetPath)
                .OfType<Sprite>()
                .ToArray();
            Assert.That(allSpritesAtPath.Length, Is.EqualTo(4));
        }

        [Test]
        public void Import_RejectsMismatchedSha256SourcePng_AndRejectsInvalidParameters()
        {
            string tempFolder = CreateUniqueTempAssetFolderName("RejectInvalid");
            string tempCorruptedPng = Path.Combine(Path.GetTempPath(), "rumi-idle-corrupted-" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                byte[] bytes = File.ReadAllBytes(RumiIdleProofImporter.ResolveDefaultSourcePngPath());
                bytes[bytes.Length - 16] ^= 0xFF;
                File.WriteAllBytes(tempCorruptedPng, bytes);

                Assert.Throws<InvalidOperationException>(() =>
                    RumiIdleProofImporter.Import(
                        targetFolder: tempFolder,
                        sourcePngPath: tempCorruptedPng));

                Assert.That(AssetDatabase.IsValidFolder(tempFolder), Is.False);

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder, pixelsPerUnit: 0f));

                Assert.Throws<ArgumentException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder, frameDurations: new[] { 0.1f, 0.1f }));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder, frameDurations: new[] { 0.1f, -0.1f, 0.1f, 0.1f }));
            }
            finally
            {
                if (File.Exists(tempCorruptedPng)) File.Delete(tempCorruptedPng);
                DeleteTempAssetFolderIfExists(tempFolder);
            }
        }

        [Test]
        public void Import_RefusesToOverwriteForeignTextureOrForeignCatalogAsset()
        {
            string tempFolder = CreateUniqueTempAssetFolderName("RefuseForeign");
            try
            {
                EnsureTempFolderHierarchy(tempFolder);

                string foreignTextureAssetPath = tempFolder + "/" + RumiIdleProofImporter.DefaultTextureFileName;
                string foreignTextureDiskPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    foreignTextureAssetPath.Replace('/', Path.DirectorySeparatorChar));
                byte[] foreignBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x01, 0x02, 0x03 };
                File.WriteAllBytes(foreignTextureDiskPath, foreignBytes);

                Assert.Throws<InvalidOperationException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder));
                Assert.That(File.ReadAllBytes(foreignTextureDiskPath), Is.EqualTo(foreignBytes));

                File.Delete(foreignTextureDiskPath);

                string foreignCatalogAssetPath = tempFolder + "/" + RumiIdleProofImporter.DefaultCatalogFileName;
                var foreignAsset = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
                AssetDatabase.CreateAsset(foreignAsset, foreignCatalogAssetPath);
                AssetDatabase.SaveAssets();

                Assert.Throws<InvalidOperationException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder));
                Assert.That(AssetDatabase.LoadMainAssetAtPath(foreignCatalogAssetPath), Is.InstanceOf<StageBlockoutDefinition>());
            }
            finally
            {
                DeleteTempAssetFolderIfExists(tempFolder);
            }
        }

        [Test]
        public void Import_RefusesToOverwriteSameHashPngWithoutOwnershipMarker_AndRefusesUnmarkedRumiIdleCatalog()
        {
            string tempFolder = CreateUniqueTempAssetFolderName("RefuseUnmarked");
            try
            {
                EnsureTempFolderHierarchy(tempFolder);

                // Case 1: Exact same PNG (same SHA256) already imported at destination, but without ownership marker in userData.
                string textureAssetPath = tempFolder + "/" + RumiIdleProofImporter.DefaultTextureFileName;
                string textureDiskPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    textureAssetPath.Replace('/', Path.DirectorySeparatorChar));
                File.Copy(RumiIdleProofImporter.ResolveDefaultSourcePngPath(), textureDiskPath, true);
                AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceSynchronousImport);

                var unmarkedImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
                Assert.That(unmarkedImporter, Is.Not.Null);
                unmarkedImporter.filterMode = FilterMode.Point;
                unmarkedImporter.spriteImportMode = SpriteImportMode.Single;
                unmarkedImporter.userData = string.Empty;
                unmarkedImporter.SaveAndReimport();

                Assert.Throws<InvalidOperationException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder));

                var afterAttemptImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
                Assert.That(afterAttemptImporter.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(afterAttemptImporter.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
                Assert.That(afterAttemptImporter.userData, Is.Empty);

                AssetDatabase.DeleteAsset(textureAssetPath);

                // Case 2: Existing CharacterSpriteSet2D for "Rumi" / "Idle" (or empty) without explicit ownership marker.
                string catalogAssetPath = tempFolder + "/" + RumiIdleProofImporter.DefaultCatalogFileName;
                var unmarkedCatalog = ScriptableObject.CreateInstance<CharacterSpriteSet2D>();
                using (var so = new SerializedObject(unmarkedCatalog))
                {
                    so.FindProperty("characterName").stringValue = "Rumi";
                    so.FindProperty("pixelsPerUnit").floatValue = 123f;
                    so.FindProperty("worldScale").floatValue = 2.5f;
                    SerializedProperty states = so.FindProperty("states");
                    states.arraySize = 1;
                    states.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "Idle";
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                AssetDatabase.CreateAsset(unmarkedCatalog, catalogAssetPath);
                AssetDatabase.SaveAssets();

                Assert.Throws<InvalidOperationException>(() =>
                    RumiIdleProofImporter.Import(targetFolder: tempFolder));

                var preservedCatalog = AssetDatabase.LoadAssetAtPath<CharacterSpriteSet2D>(catalogAssetPath);
                Assert.That(preservedCatalog, Is.Not.Null);
                Assert.That(preservedCatalog.PixelsPerUnit, Is.EqualTo(123f).Within(0.001f));
                Assert.That(preservedCatalog.WorldScale, Is.EqualTo(2.5f).Within(0.001f));
            }
            finally
            {
                DeleteTempAssetFolderIfExists(tempFolder);
            }
        }

        private static void EnsureTempFolderHierarchy(string tempFolder)
        {
            string parent = "Assets/Art/HD/Rumi";
            string leaf = tempFolder.Substring(parent.Length + 1);
            if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder("Assets/Art/HD")) AssetDatabase.CreateFolder("Assets/Art", "HD");
            if (!AssetDatabase.IsValidFolder(parent)) AssetDatabase.CreateFolder("Assets/Art/HD", "Rumi");
            if (!AssetDatabase.IsValidFolder(tempFolder)) AssetDatabase.CreateFolder(parent, leaf);
        }

        private static string CreateUniqueTempAssetFolderName(string suffix)
        {
            return TempFolderPrefix + suffix + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        private static void DeleteTempAssetFolderIfExists(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                AssetDatabase.DeleteAsset(assetFolder);
            }
        }
    }
}
