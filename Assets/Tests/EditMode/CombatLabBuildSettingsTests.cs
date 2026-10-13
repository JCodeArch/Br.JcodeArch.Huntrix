using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace HuntrX.Tests.EditMode
{
    public sealed class CombatLabBuildSettingsTests
    {
        [Test]
        public void VerticalSliceStartsBeforeEnabledLegacyTestHosts()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            CollectionAssert.AreEqual(new[]
            {
                "Assets/Scenes/VerticalSlice.unity",
                "Assets/Scenes/SampleScene.unity",
                "Assets/Scenes/CombatLab.unity"
            }, enabledScenes, "The playable slice must start first; legacy scene fixtures remain loadable.");
        }
    }
}
