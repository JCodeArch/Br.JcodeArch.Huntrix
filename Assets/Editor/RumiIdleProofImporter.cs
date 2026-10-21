using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HuntrX.Data;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace HuntrX.Editor
{
    public sealed class RumiIdleProofImportResult
    {
        public RumiIdleProofImportResult(
            string textureAssetPath,
            string catalogAssetPath,
            string sha256,
            Texture2D texture,
            Sprite[] sprites,
            CharacterSpriteSet2D catalog)
        {
            TextureAssetPath = textureAssetPath;
            CatalogAssetPath = catalogAssetPath;
            Sha256 = sha256;
            Texture = texture;
            Sprites = sprites;
            Catalog = catalog;
        }

        public string TextureAssetPath { get; }
        public string CatalogAssetPath { get; }
        public string Sha256 { get; }
        public Texture2D Texture { get; }
        public Sprite[] Sprites { get; }
        public CharacterSpriteSet2D Catalog { get; }
    }

    /// <summary>
    /// Deterministic Editor importer for the RumiIdleV2 4-frame animation proof.
    /// Uses Unity.2D.Sprite.Editor public data providers (ISpriteEditorDataProvider / ISpriteNameFileIdDataProvider)
    /// and TextureImporterSettings without touching pixels or using runtime reflection.
    /// </summary>
    public static class RumiIdleProofImporter
    {
        public const string CharacterId = "Rumi";
        public const string IdleStateId = "Idle";
        public const string ExpectedSha256 = "2bee6f1261a993e6d17a1eacd9e1df3c6b703c479d5c4cec89137c3ef73cf8eb";
        public const int ExpectedTextureWidth = 1024;
        public const int ExpectedTextureHeight = 1536;
        public const float DefaultPixelsPerUnit = 200f;
        public const float DefaultWorldScale = 1f;
        public const string ImporterUserDataTag = "HuntrX.RumiIdleProofV2";
        public const string CatalogOwnershipId = "huntrx.proof.rumi_idle_v2";

        public const string DefaultSourceRelativePath = "Art/HD/AnimationProofs/rumi-idle-proof-v2.png";
        public const string DefaultLayoutRelativePath = "Art/HD/AnimationProofs/rumi-idle-proof-v2.layout.json";
        public const string DefaultTargetFolder = "Assets/Art/HD/Rumi/IdleProofV2";
        public const string DefaultTextureFileName = "rumi-idle-proof-v2.png";
        public const string DefaultCatalogFileName = "RumiIdleProofV2.asset";

        public static readonly Vector2 ExpectedPivot = new Vector2(0.60f, 0.03629f);
        public static readonly Vector2 ExpectedFrameSize = new Vector2(448f, 744f);

        public static readonly string[] ExpectedFrameNames =
        {
            "rumi_idle_00",
            "rumi_idle_01",
            "rumi_idle_02",
            "rumi_idle_03"
        };

        public static readonly Rect[] ExpectedFrameRects =
        {
            new Rect(64f, 776f, 448f, 744f),
            new Rect(544f, 776f, 448f, 744f),
            new Rect(64f, 8f, 448f, 744f),
            new Rect(544f, 8f, 448f, 744f)
        };

        private static readonly string[] DeterministicSpriteGuids =
        {
            "2bee6f1261a993e6d17a1eacd9e1df00",
            "2bee6f1261a993e6d17a1eacd9e1df01",
            "2bee6f1261a993e6d17a1eacd9e1df02",
            "2bee6f1261a993e6d17a1eacd9e1df03"
        };

        [Serializable]
        private sealed class LayoutFrameSpec
        {
            public string name = string.Empty;
            public float[] rect = Array.Empty<float>();
        }

        [Serializable]
        private sealed class LayoutSpec
        {
            public int schemaVersion = 0;
            public string source = string.Empty;
            public string sha256 = string.Empty;
            public int[] textureSize = Array.Empty<int>();
            public string rectCoordinateSystem = string.Empty;
            public float[] pivotNormalized = Array.Empty<float>();
            public string meshType = string.Empty;
            public LayoutFrameSpec[] frames = Array.Empty<LayoutFrameSpec>();
        }

        [MenuItem("HUNTR/X/Art/Import Rumi Idle Proof V2")]
        public static void ImportFromMenu()
        {
            RumiIdleProofImportResult result = ImportDefault();
            Debug.Log(
                $"Imported RumiIdleProofV2 ({result.Sprites.Length} frames, PPU={result.Catalog.PixelsPerUnit}) at '{result.CatalogAssetPath}'.",
                result.Catalog);
        }

        public static RumiIdleProofImportResult ImportDefault() => Import();

        public static string ResolveDefaultSourcePngPath() => Path.GetFullPath(DefaultSourceRelativePath);

        public static string ResolveDefaultLayoutJsonPath() => Path.GetFullPath(DefaultLayoutRelativePath);

        public static RumiIdleProofImportResult Import(
            string targetFolder = DefaultTargetFolder,
            float pixelsPerUnit = DefaultPixelsPerUnit,
            IReadOnlyList<float> frameDurations = null,
            float worldScale = DefaultWorldScale,
            string sourcePngPath = null,
            string layoutJsonPath = null)
        {
            ValidateParameters(targetFolder, pixelsPerUnit, frameDurations, worldScale, out string normalizedFolder, out float[] resolvedDurations);

            string resolvedSourcePng = string.IsNullOrWhiteSpace(sourcePngPath) ? ResolveDefaultSourcePngPath() : sourcePngPath;
            string resolvedLayoutJson = string.IsNullOrWhiteSpace(layoutJsonPath) ? ResolveDefaultLayoutJsonPath() : layoutJsonPath;

            if (!File.Exists(resolvedSourcePng))
                throw new FileNotFoundException("RumiIdleProofV2 source PNG was not found.", resolvedSourcePng);
            if (!File.Exists(resolvedLayoutJson))
                throw new FileNotFoundException("RumiIdleProofV2 layout JSON was not found.", resolvedLayoutJson);

            ValidateLayoutJsonFile(resolvedLayoutJson);

            byte[] sourceBytes = File.ReadAllBytes(resolvedSourcePng);
            string actualSha256 = ComputeSha256Hex(sourceBytes);
            if (!string.Equals(actualSha256, ExpectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Source PNG SHA256 mismatch. Expected '{ExpectedSha256}', got '{actualSha256}'.");

            ValidatePngDimensions(sourceBytes, ExpectedTextureWidth, ExpectedTextureHeight);

            string textureAssetPath = normalizedFolder + "/" + DefaultTextureFileName;
            string catalogAssetPath = normalizedFolder + "/" + DefaultCatalogFileName;

            VerifyDestinationNotForeign(normalizedFolder, textureAssetPath, catalogAssetPath);
            EnsureAssetFolderExists(normalizedFolder);

            string textureDiskPath = ToDiskPath(textureAssetPath);
            if (!File.Exists(textureDiskPath))
            {
                File.WriteAllBytes(textureDiskPath, sourceBytes);
                AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
            if (importer == null)
                throw new InvalidOperationException($"Failed to acquire TextureImporter at '{textureAssetPath}'.");

            ConfigureTextureAndSlices(importer, pixelsPerUnit);

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
            if (texture == null || texture.width != ExpectedTextureWidth || texture.height != ExpectedTextureHeight)
                throw new InvalidOperationException($"Imported texture at '{textureAssetPath}' is invalid.");

            Sprite[] orderedSprites = TryGetValidatedSprites(textureAssetPath, pixelsPerUnit);
            if (orderedSprites == null)
                throw new InvalidOperationException($"Expected {ExpectedFrameRects.Length} valid sliced sprites at '{textureAssetPath}'.");

            CharacterSpriteSet2D catalog = CreateOrUpdateCatalogAsset(catalogAssetPath, pixelsPerUnit, worldScale, orderedSprites, resolvedDurations);
            return new RumiIdleProofImportResult(textureAssetPath, catalogAssetPath, actualSha256, texture, orderedSprites, catalog);
        }

        public static string ComputeSha256Hex(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private static void ConfigureTextureAndSlices(TextureImporter importer, float pixelsPerUnit)
        {
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (width != ExpectedTextureWidth || height != ExpectedTextureHeight)
                throw new InvalidOperationException($"Texture size {width}x{height} != {ExpectedTextureWidth}x{ExpectedTextureHeight}.");

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
                throw new InvalidOperationException($"ISpriteEditorDataProvider unavailable for '{importer.assetPath}'.");
            dataProvider.InitSpriteEditorDataProvider();

            if (IsImporterAndSlicesAlreadyConfigured(importer, dataProvider, pixelsPerUnit) &&
                TryGetValidatedSprites(importer.assetPath, pixelsPerUnit) != null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.userData = ImporterUserDataTag;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Multiple;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = ExpectedPivot;
            settings.spritePixelsPerUnit = pixelsPerUnit;
            settings.spriteExtrude = 1;
            settings.alphaIsTransparency = true;
            settings.mipmapEnabled = false;
            settings.readable = false;
            importer.SetTextureSettings(settings);

            dataProvider.InitSpriteEditorDataProvider();
            SpriteRect[] existingRects = dataProvider.GetSpriteRects() ?? Array.Empty<SpriteRect>();
            var targetRects = new SpriteRect[ExpectedFrameRects.Length];
            var fileIdPairs = new SpriteNameFileIdPair[ExpectedFrameRects.Length];

            for (int i = 0; i < ExpectedFrameRects.Length; i++)
            {
                string frameName = ExpectedFrameNames[i];
                SpriteRect match = existingRects.FirstOrDefault(r => r != null && string.Equals(r.name, frameName, StringComparison.Ordinal));
                GUID spriteId = match != null && !match.spriteID.Empty() ? match.spriteID : new GUID(DeterministicSpriteGuids[i]);

                targetRects[i] = new SpriteRect
                {
                    name = frameName,
                    spriteID = spriteId,
                    rect = ExpectedFrameRects[i],
                    alignment = SpriteAlignment.Custom,
                    pivot = ExpectedPivot,
                    border = Vector4.zero
                };
                fileIdPairs[i] = new SpriteNameFileIdPair(frameName, spriteId);
            }

            dataProvider.SetSpriteRects(targetRects);
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>()?.SetNameFileIdPairs(fileIdPairs);
            dataProvider.Apply();
            importer.SaveAndReimport();
        }

        private static bool IsImporterAndSlicesAlreadyConfigured(
            TextureImporter importer,
            ISpriteEditorDataProvider dataProvider,
            float pixelsPerUnit)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Multiple ||
                !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit) ||
                !importer.alphaIsTransparency ||
                importer.mipmapEnabled ||
                importer.isReadable ||
                importer.textureCompression != TextureImporterCompression.Uncompressed ||
                importer.maxTextureSize != 2048 ||
                importer.filterMode != FilterMode.Bilinear ||
                importer.wrapMode != TextureWrapMode.Clamp ||
                settings.spriteMeshType != SpriteMeshType.FullRect ||
                settings.spriteAlignment != (int)SpriteAlignment.Custom ||
                Vector2.Distance(settings.spritePivot, ExpectedPivot) > 0.0001f ||
                settings.spriteExtrude != 1 ||
                !string.Equals(importer.userData, ImporterUserDataTag, StringComparison.Ordinal))
            {
                return false;
            }

            SpriteRect[] rects = dataProvider.GetSpriteRects();
            if (rects == null || rects.Length != ExpectedFrameRects.Length) return false;

            var fileIdProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            SpriteNameFileIdPair[] pairs = fileIdProvider?.GetNameFileIdPairs()?.ToArray();
            if (pairs == null || pairs.Length != ExpectedFrameRects.Length) return false;

            for (int i = 0; i < ExpectedFrameRects.Length; i++)
            {
                SpriteRect r = rects[i];
                if (r == null ||
                    !string.Equals(r.name, ExpectedFrameNames[i], StringComparison.Ordinal) ||
                    r.rect != ExpectedFrameRects[i] ||
                    r.alignment != SpriteAlignment.Custom ||
                    Vector2.Distance(r.pivot, ExpectedPivot) > 0.0001f ||
                    r.spriteID.Empty())
                {
                    return false;
                }

                bool hasMatchingPair = pairs.Any(p =>
                    p != null &&
                    string.Equals(p.name, ExpectedFrameNames[i], StringComparison.Ordinal) &&
                    p.GetFileGUID() == r.spriteID);
                if (!hasMatchingPair) return false;
            }

            return true;
        }

        private static Sprite[] TryGetValidatedSprites(string textureAssetPath, float expectedPixelsPerUnit)
        {
            Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(textureAssetPath).OfType<Sprite>().ToArray();
            if (allSprites.Length != ExpectedFrameRects.Length) return null;

            var ordered = new Sprite[ExpectedFrameRects.Length];
            Vector2 expectedPixelPivot = Vector2.Scale(ExpectedFrameSize, ExpectedPivot);
            for (int i = 0; i < ExpectedFrameRects.Length; i++)
            {
                string expectedName = ExpectedFrameNames[i];
                Sprite match = allSprites.FirstOrDefault(s => s != null && string.Equals(s.name, expectedName, StringComparison.Ordinal));
                if (match == null ||
                    match.rect != ExpectedFrameRects[i] ||
                    !Mathf.Approximately(match.pixelsPerUnit, expectedPixelsPerUnit) ||
                    Vector2.Distance(match.pivot, expectedPixelPivot) > 0.01f ||
                    match.vertices == null || match.vertices.Length != 4)
                {
                    return null;
                }

                ordered[i] = match;
            }

            return ordered;
        }

        private static CharacterSpriteSet2D CreateOrUpdateCatalogAsset(
            string catalogAssetPath,
            float pixelsPerUnit,
            float worldScale,
            Sprite[] sprites,
            float[] durations)
        {
            CharacterSpriteSet2D catalog = AssetDatabase.LoadAssetAtPath<CharacterSpriteSet2D>(catalogAssetPath);
            bool isNewAsset = catalog == null;
            if (isNewAsset) catalog = ScriptableObject.CreateInstance<CharacterSpriteSet2D>();

            bool modified = isNewAsset;
            using (var so = new SerializedObject(catalog))
            {
                modified |= SetString(so.FindProperty("id"), CatalogOwnershipId);
                modified |= SetString(so.FindProperty("characterName"), CharacterId);
                modified |= SetFloat(so.FindProperty("pixelsPerUnit"), pixelsPerUnit);
                modified |= SetFloat(so.FindProperty("worldScale"), worldScale);
                modified |= SetVector2(so.FindProperty("footAnchor"), ExpectedPivot);

                SerializedProperty statesProp = so.FindProperty("states");
                if (statesProp.arraySize != 1) { statesProp.arraySize = 1; modified = true; }

                SerializedProperty idleState = statesProp.GetArrayElementAtIndex(0);
                modified |= SetString(idleState.FindPropertyRelative("id"), IdleStateId);

                SerializedProperty clipProp = idleState.FindPropertyRelative("clip");
                SerializedProperty loopProp = clipProp.FindPropertyRelative("loop");
                if (!loopProp.boolValue) { loopProp.boolValue = true; modified = true; }

                SerializedProperty framesProp = clipProp.FindPropertyRelative("frames");
                SerializedProperty durationsProp = clipProp.FindPropertyRelative("durations");
                if (framesProp.arraySize != sprites.Length) { framesProp.arraySize = sprites.Length; modified = true; }
                if (durationsProp.arraySize != durations.Length) { durationsProp.arraySize = durations.Length; modified = true; }

                for (int i = 0; i < sprites.Length; i++)
                {
                    SerializedProperty frameElem = framesProp.GetArrayElementAtIndex(i);
                    if (frameElem.objectReferenceValue != sprites[i])
                    {
                        frameElem.objectReferenceValue = sprites[i];
                        modified = true;
                    }

                    modified |= SetFloat(durationsProp.GetArrayElementAtIndex(i), durations[i]);
                }

                if (modified) so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(catalog, catalogAssetPath);
                AssetDatabase.SaveAssets();
            }
            else if (modified)
            {
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }

            AssetImporter catalogImporter = AssetImporter.GetAtPath(catalogAssetPath);
            if (catalogImporter != null && !string.Equals(catalogImporter.userData, ImporterUserDataTag, StringComparison.Ordinal))
            {
                catalogImporter.userData = ImporterUserDataTag;
                catalogImporter.SaveAndReimport();
            }

            if (!catalog.IsValid || !catalog.TryGetClip(IdleStateId, out SpriteAnimationClip2D idleClip) || idleClip == null || !idleClip.IsValid)
                throw new InvalidOperationException($"Generated CharacterSpriteSet2D at '{catalogAssetPath}' failed validation.");

            return catalog;
        }

        private static void ValidateParameters(
            string targetFolder,
            float pixelsPerUnit,
            IReadOnlyList<float> frameDurations,
            float worldScale,
            out string normalizedFolder,
            out float[] resolvedDurations)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target asset folder cannot be blank.", nameof(targetFolder));

            normalizedFolder = targetFolder.Trim().Replace('\\', '/').TrimEnd('/');
            if (!normalizedFolder.StartsWith("Assets/Art/HD/", StringComparison.Ordinal) ||
                normalizedFolder.Contains("..", StringComparison.Ordinal) ||
                normalizedFolder.Contains(":", StringComparison.Ordinal))
            {
                throw new ArgumentException("Target folder must reside under 'Assets/Art/HD/'.", nameof(targetFolder));
            }

            if (!IsFinite(pixelsPerUnit) || pixelsPerUnit <= 0f || pixelsPerUnit > 4096f)
                throw new ArgumentOutOfRangeException(nameof(pixelsPerUnit), "PixelsPerUnit must be within (0, 4096].");
            if (!IsFinite(worldScale) || worldScale <= 0f || worldScale > 10f)
                throw new ArgumentOutOfRangeException(nameof(worldScale), "WorldScale must be within (0, 10].");

            if (frameDurations == null)
            {
                // Relaxed hold, subtle motion, brief authored blink, then return.
                resolvedDurations = new[] { 2.0f, 0.4f, 0.10f, 0.4f };
                return;
            }

            if (frameDurations.Count != ExpectedFrameRects.Length)
                throw new ArgumentException($"Expected {ExpectedFrameRects.Length} durations.", nameof(frameDurations));

            resolvedDurations = new float[ExpectedFrameRects.Length];
            for (int i = 0; i < frameDurations.Count; i++)
            {
                if (!IsFinite(frameDurations[i]) || frameDurations[i] <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(frameDurations), $"Duration[{i}] must be finite and > 0.");
                resolvedDurations[i] = frameDurations[i];
            }
        }

        private static void ValidateLayoutJsonFile(string layoutJsonPath)
        {
            LayoutSpec spec = JsonUtility.FromJson<LayoutSpec>(File.ReadAllText(layoutJsonPath, Encoding.UTF8));
            if (spec == null || spec.schemaVersion != 1 ||
                !string.Equals(spec.source, DefaultTextureFileName, StringComparison.Ordinal) ||
                !string.Equals(spec.sha256, ExpectedSha256, StringComparison.OrdinalIgnoreCase) ||
                spec.textureSize == null || spec.textureSize.Length != 2 ||
                spec.textureSize[0] != ExpectedTextureWidth || spec.textureSize[1] != ExpectedTextureHeight ||
                !string.Equals(spec.rectCoordinateSystem, "Unity-bottom-left", StringComparison.Ordinal) ||
                !string.Equals(spec.meshType, "FullRect", StringComparison.Ordinal) ||
                spec.pivotNormalized == null || spec.pivotNormalized.Length != 2 ||
                Mathf.Abs(spec.pivotNormalized[0] - ExpectedPivot.x) > 0.0001f ||
                Mathf.Abs(spec.pivotNormalized[1] - ExpectedPivot.y) > 0.0001f ||
                spec.frames == null || spec.frames.Length != ExpectedFrameRects.Length)
            {
                throw new InvalidOperationException("Layout JSON does not match approved RumiIdleProofV2 geometry.");
            }

            for (int i = 0; i < ExpectedFrameRects.Length; i++)
            {
                LayoutFrameSpec frame = spec.frames[i];
                Rect expected = ExpectedFrameRects[i];
                if (frame == null || !string.Equals(frame.name, ExpectedFrameNames[i], StringComparison.Ordinal) ||
                    frame.rect == null || frame.rect.Length != 4 ||
                    new Rect(frame.rect[0], frame.rect[1], frame.rect[2], frame.rect[3]) != expected)
                {
                    throw new InvalidOperationException($"Layout JSON frame {i} does not match expected rect.");
                }
            }
        }

        private static void ValidatePngDimensions(byte[] pngBytes, int expectedWidth, int expectedHeight)
        {
            if (pngBytes == null || pngBytes.Length < 24 ||
                pngBytes[0] != 0x89 || pngBytes[1] != 0x50 || pngBytes[2] != 0x4E || pngBytes[3] != 0x47)
            {
                throw new InvalidOperationException("Source file is not a valid PNG.");
            }

            int width = (pngBytes[16] << 24) | (pngBytes[17] << 16) | (pngBytes[18] << 8) | pngBytes[19];
            int height = (pngBytes[20] << 24) | (pngBytes[21] << 16) | (pngBytes[22] << 8) | pngBytes[23];
            if (width != expectedWidth || height != expectedHeight)
                throw new InvalidOperationException($"PNG size ({width}x{height}) != ({expectedWidth}x{expectedHeight}).");
        }

        private static void VerifyDestinationNotForeign(string targetFolder, string textureAssetPath, string catalogAssetPath)
        {
            if (File.Exists(ToDiskPath(targetFolder)))
                throw new InvalidOperationException($"Refusing to overwrite file at folder path '{targetFolder}'.");

            string textureDiskPath = ToDiskPath(textureAssetPath);
            if (Directory.Exists(textureDiskPath))
                throw new InvalidOperationException($"Refusing to overwrite directory at '{textureAssetPath}'.");
            if (File.Exists(textureDiskPath))
            {
                string existingHash = ComputeSha256Hex(File.ReadAllBytes(textureDiskPath));
                if (!string.Equals(existingHash, ExpectedSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Refusing to overwrite foreign texture at '{textureAssetPath}'.");

                var existingImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
                if (existingImporter == null ||
                    !string.Equals(existingImporter.userData, ImporterUserDataTag, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Refusing to overwrite texture at '{textureAssetPath}' without explicit ownership marker '{ImporterUserDataTag}'.");
                }
            }

            string catalogDiskPath = ToDiskPath(catalogAssetPath);
            if (Directory.Exists(catalogDiskPath))
                throw new InvalidOperationException($"Refusing to overwrite directory at '{catalogAssetPath}'.");
            if (File.Exists(catalogDiskPath))
            {
                UnityEngine.Object existingMain = AssetDatabase.LoadMainAssetAtPath(catalogAssetPath);
                if (existingMain == null)
                {
                    AssetDatabase.ImportAsset(catalogAssetPath, ImportAssetOptions.ForceSynchronousImport);
                    existingMain = AssetDatabase.LoadMainAssetAtPath(catalogAssetPath);
                }

                var existingCatalog = existingMain as CharacterSpriteSet2D;
                if (existingCatalog == null)
                    throw new InvalidOperationException($"Refusing to overwrite non-CharacterSpriteSet2D asset at '{catalogAssetPath}'.");

                AssetImporter existingCatalogImporter = AssetImporter.GetAtPath(catalogAssetPath);
                if (!string.Equals(existingCatalog.Id, CatalogOwnershipId, StringComparison.Ordinal) ||
                    existingCatalogImporter == null ||
                    !string.Equals(existingCatalogImporter.userData, ImporterUserDataTag, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Refusing to overwrite CharacterSpriteSet2D at '{catalogAssetPath}' without explicit ownership marker '{CatalogOwnershipId}'.");
                }

                using (var so = new SerializedObject(existingCatalog))
                {
                    string existingName = so.FindProperty("characterName")?.stringValue ?? string.Empty;
                    if (!string.Equals(existingName, CharacterId, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Refusing to overwrite CharacterSpriteSet2D for '{existingName}'.");

                    SerializedProperty statesProp = so.FindProperty("states");
                    if (statesProp == null || statesProp.arraySize != 1)
                        throw new InvalidOperationException("Refusing to overwrite CharacterSpriteSet2D with unexpected state count.");
                    string stateId = statesProp.GetArrayElementAtIndex(0).FindPropertyRelative("id")?.stringValue ?? string.Empty;
                    if (!string.Equals(stateId, IdleStateId, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Refusing to overwrite non-Idle state '{stateId}'.");
                }
            }
        }

        private static void EnsureAssetFolderExists(string targetFolder)
        {
            if (AssetDatabase.IsValidFolder(targetFolder)) return;
            string[] parts = targetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string ToDiskPath(string assetPath) =>
            Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace('/', Path.DirectorySeparatorChar));

        private static bool SetString(SerializedProperty prop, string value)
        {
            if (string.Equals(prop.stringValue, value, StringComparison.Ordinal)) return false;
            prop.stringValue = value;
            return true;
        }

        private static bool SetFloat(SerializedProperty prop, float value)
        {
            if (Mathf.Approximately(prop.floatValue, value)) return false;
            prop.floatValue = value;
            return true;
        }

        private static bool SetVector2(SerializedProperty prop, Vector2 value)
        {
            if (Vector2.Distance(prop.vector2Value, value) <= 0.00001f) return false;
            prop.vector2Value = value;
            return true;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
