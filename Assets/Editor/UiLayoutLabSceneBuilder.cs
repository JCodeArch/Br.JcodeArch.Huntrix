using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HuntrX.Presentation.UI;

namespace HuntrX.Editor
{
    public static class UiLayoutLabSceneBuilder
    {
        [MenuItem("HUNTR/X/Build UI Layout Lab Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // The same factory is used by the committed scene at runtime and by this editor command.
            // Keeping one composition avoids a stale placeholder scene diverging from the builder.
            new GameObject("UiLayoutLab").AddComponent<SliceUiLabLayout2D>();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/UiLayoutLab.unity");
            AssetDatabase.Refresh();
        }
    }
}
