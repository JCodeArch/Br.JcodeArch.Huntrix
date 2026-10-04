using System.Collections;
using System.Reflection;
using HuntrX.Bootstrap;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CombatLabScenePlayModeTests
    {
        [UnityTest]
        public IEnumerator LoadsAsAnEmptyGameplayHostWithTheApprovedCamera()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync("CombatLab", LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null);
            yield return loadOperation;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("CombatLab"));

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Exclude);
            Assert.That(cameras, Has.Length.EqualTo(1));
            Assert.That(cameras[0].orthographic, Is.True);
            Assert.That(cameras[0].orthographicSize, Is.EqualTo(5.4f).Within(0.001f));
            Assert.That(cameras[0].transform.position.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cameras[0].transform.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(Quaternion.Angle(cameras[0].transform.rotation, Quaternion.identity), Is.EqualTo(0f).Within(0.001f));

            GameObject globalLight = GameObject.Find("Global Light 2D");
            Assert.That(globalLight, Is.Not.Null);
            Assert.That(globalLight.activeInHierarchy, Is.True);

            GameBootstrap[] bootstraps = Object.FindObjectsByType<GameBootstrap>(FindObjectsInactive.Exclude);
            Assert.That(bootstraps, Has.Length.EqualTo(1));
            Assert.That(bootstraps[0].enabled, Is.True);
            Assert.That(bootstraps[0].gameObject.activeInHierarchy, Is.True);

            FieldInfo systemsField = typeof(GameBootstrap).GetField("systems", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(systemsField, Is.Not.Null);
            var systems = systemsField.GetValue(bootstraps[0]) as MonoBehaviour[];
            Assert.That(systems, Is.Not.Null);
            Assert.That(systems, Is.Empty);
        }
    }
}
