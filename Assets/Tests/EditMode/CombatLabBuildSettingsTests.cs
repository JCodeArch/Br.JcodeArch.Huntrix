using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace HuntrX.Tests.EditMode
{
    public sealed class CombatLabBuildSettingsTests
    {
        [Test]
        public void CombatLabIsEnabledAfterTheUnchangedStartupScene()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(enabledScenes, Does.Contain("Assets/Scenes/SampleScene.unity"));
            Assert.That(enabledScenes, Does.Contain("Assets/Scenes/CombatLab.unity"));
            Assert.That(enabledScenes[0], Is.EqualTo("Assets/Scenes/SampleScene.unity"));
            Assert.That(enabledScenes[1], Is.EqualTo("Assets/Scenes/CombatLab.unity"));
        }
    }
}
