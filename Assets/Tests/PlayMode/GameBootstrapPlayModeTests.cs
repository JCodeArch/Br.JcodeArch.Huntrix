using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Bootstrap;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace HuntrX.Tests.PlayMode
{
    public sealed class GameBootstrapPlayModeTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            GameSystemProbe.InitializationOrder.Clear();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.Destroy(createdObject);
                }
            }

            createdObjects.Clear();
            yield return null;
            GameSystemProbe.InitializationOrder.Clear();
        }

        [UnityTest]
        public IEnumerator InitializesSystemsInDeclaredOrderOnce()
        {
            GameSystemProbe firstSystem = CreateSystem("First", 1);
            GameSystemProbe secondSystem = CreateSystem("Second", 2);
            GameBootstrap bootstrap = CreateBootstrap();
            SetSystems(bootstrap, firstSystem, secondSystem);

            yield return null;
            CollectionAssert.AreEqual(new[] { 1, 2 }, GameSystemProbe.InitializationOrder);

            yield return null;
            CollectionAssert.AreEqual(new[] { 1, 2 }, GameSystemProbe.InitializationOrder);
        }

        [UnityTest]
        public IEnumerator RejectsInvalidSystemBeforeInitializingAnySystem()
        {
            GameSystemProbe firstSystem = CreateSystem("First", 1);
            GameObject invalidObject = CreateObject("NotAnInitializableSystem");
            MonoBehaviour invalidSystem = invalidObject.AddComponent<NonInitializableProbe>();
            GameBootstrap bootstrap = CreateBootstrap();
            SetSystems(bootstrap, firstSystem, invalidSystem);

            LogAssert.Expect(
                LogType.Error,
                "GameBootstrap component 'NonInitializableProbe' does not implement IGameInitializable. No systems were initialized.");

            yield return null;
            CollectionAssert.IsEmpty(GameSystemProbe.InitializationOrder);
        }

        [UnityTest]
        public IEnumerator RejectsDuplicateReferenceBeforeInitializingAnySystem()
        {
            GameSystemProbe system = CreateSystem("Repeated", 1);
            GameBootstrap bootstrap = CreateBootstrap();
            SetSystems(bootstrap, system, system);

            LogAssert.Expect(
                LogType.Error,
                "GameBootstrap references the same component more than once at index 1. No systems were initialized.");

            yield return null;
            CollectionAssert.IsEmpty(GameSystemProbe.InitializationOrder);
        }

        [UnityTest]
        public IEnumerator EmptyCompositionStartsWithoutInitializingSystems()
        {
            CreateBootstrap();

            yield return null;
            CollectionAssert.IsEmpty(GameSystemProbe.InitializationOrder);
        }

        [UnityTest]
        public IEnumerator StartupSceneContainsOneActiveBootstrap()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            yield return loadOperation;

            GameBootstrap[] bootstraps = Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Exclude);
            Assert.That(bootstraps, Has.Length.EqualTo(1));
            Assert.That(bootstraps[0].enabled, Is.True);
            Assert.That(bootstraps[0].gameObject.activeInHierarchy, Is.True);
        }
        private GameSystemProbe CreateSystem(string objectName, int order)
        {
            GameSystemProbe system = CreateObject(objectName).AddComponent<GameSystemProbe>();
            system.Order = order;
            return system;
        }

        private GameBootstrap CreateBootstrap()
        {
            return CreateObject("GameBootstrap").AddComponent<GameBootstrap>();
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new GameObject(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static void SetSystems(GameBootstrap bootstrap, params MonoBehaviour[] systems)
        {
            FieldInfo systemsField = typeof(GameBootstrap).GetField(
                "systems",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(systemsField, Is.Not.Null, "The serialized systems field is required for scene composition.");
            systemsField.SetValue(bootstrap, systems);
        }
    }

    public sealed class GameSystemProbe : MonoBehaviour, IGameInitializable
    {
        public static readonly List<int> InitializationOrder = new List<int>();
        public int Order;

        public void Initialize()
        {
            InitializationOrder.Add(Order);
        }
    }

    public sealed class NonInitializableProbe : MonoBehaviour
    {
    }
}
