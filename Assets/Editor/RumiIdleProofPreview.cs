using System;
using System.IO;
using HuntrX.Presentation.Art;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HuntrX.Editor
{
    /// <summary>Isolated visual QA scene; never replaces gameplay composition.</summary>
    public static class RumiIdleProofPreview
    {
        public const string ScenePath = RumiIdleProofImporter.DefaultTargetFolder + "/RumiIdleProofPreview.unity";

        public static void CreateScene()
        {
            var proof = RumiIdleProofImporter.ImportDefault();
            if (File.Exists(ScenePath)) throw new InvalidOperationException("Proof preview already exists; open it instead of overwriting it.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Proof Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.035f, .05f, .10f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            var prefab = Resources.Load<GameObject>("Art/Rumi_ArtPrototype");
            if (prefab == null) throw new InvalidOperationException("Rumi art presentation prefab is missing.");
            var actor = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            actor.name = "Rumi Idle V2 - visual proof only";
            actor.transform.position = new Vector3(0f, -1.7f, 0f);
            using (var serialized = new SerializedObject(actor.GetComponent<CharacterArtRenderer2D>()))
            {
                serialized.FindProperty("spriteSet").objectReferenceValue = proof.Catalog;
                serialized.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(actor.GetComponent<CharacterArtRenderer2D>());
            }
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save the isolated proof preview.");
            AssetDatabase.SaveAssets();
        }

        public static void OpenAndPlay()
        {
            if (!File.Exists(ScenePath)) throw new InvalidOperationException("Create the proof preview first.");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var catalog = AssetDatabase.LoadAssetAtPath<HuntrX.Data.CharacterSpriteSet2D>(
                RumiIdleProofImporter.DefaultTargetFolder + "/" + RumiIdleProofImporter.DefaultCatalogFileName);
            var actor = Array.Find(scene.GetRootGameObjects(), root => root.GetComponent<CharacterArtRenderer2D>() != null);
            if (catalog == null || actor == null || actor.GetComponent<CharacterArtRenderer2D>().SpriteSet != catalog)
                throw new InvalidOperationException("Saved proof scene is not bound to the imported catalog.");
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }
    }
}
