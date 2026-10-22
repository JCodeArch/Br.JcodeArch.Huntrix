using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AirplaneRequiredCaptainPlayModeTests
    {
        private FieldInfo definitionField;
        private EnemyAgent2D captainAsset;
        private object originalDefinition;
        private Scene slice;

        [UnitySetUp]
        public IEnumerator MakeCaptainUnableToSpawn()
        {
            var prefab = Resources.Load<GameObject>("Enemies/Miniboss_Prototype");
            Assert.That(prefab, Is.Not.Null);
            captainAsset = prefab.GetComponent<EnemyAgent2D>();
            Assert.That(captainAsset, Is.Not.Null);
            definitionField = typeof(EnemyAgent2D).GetField("definition", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(definitionField, Is.Not.Null);
            originalDefinition = definitionField.GetValue(captainAsset);
            Assert.That(originalDefinition, Is.Not.Null);
            definitionField.SetValue(captainAsset, null);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator RestoreCaptainAndScene()
        {
            if (captainAsset != null && definitionField != null)
                definitionField.SetValue(captainAsset, originalDefinition);
            if (slice.IsValid() && slice.isLoaded)
            {
                SceneManager.SetActiveScene(SceneManager.CreateScene("RequiredCaptainCleanup"));
                yield return SceneManager.UnloadSceneAsync(slice);
            }
        }

        [UnityTest]
        public IEnumerator AirplaneDoesNotStartWithoutItsRequiredCockpitCaptain()
        {
            LogAssert.Expect(LogType.Error, new Regex("^Slice beat failed:"));
            LogAssert.Expect(LogType.Error, "Vertical slice could not initialize its explicit composition.");
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/VerticalSlice.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("VerticalSlice", LoadSceneMode.Single);
#endif
            slice = SceneManager.GetActiveScene();
            yield return null;
            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            Assert.That(session, Is.Not.Null);
            Assert.That(session.IsRunning, Is.False,
                "The airplane must not run if the mandatory cockpit captain cannot be instantiated.");
        }
    }
}
