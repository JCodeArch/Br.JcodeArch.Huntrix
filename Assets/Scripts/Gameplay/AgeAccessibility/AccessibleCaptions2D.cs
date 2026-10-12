using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AccessibleCaptions2D : MonoBehaviour
    {
        [SerializeField] private AgeNarrativePresenter2D presenter;
        [SerializeField] private AccessibilityPreferences2D preferences;
        private void OnGUI()
        {
            if(!isActiveAndEnabled || preferences==null || !preferences.CaptionsEnabled || presenter==null || !presenter.isActiveAndEnabled || string.IsNullOrEmpty(presenter.CurrentText)) return;
            var style=new GUIStyle(GUI.skin.box) { fontSize=Mathf.Clamp(Screen.height/30,18,32),wordWrap=true,alignment=TextAnchor.MiddleCenter };
            GUI.Box(new Rect(Screen.width*.1f,Screen.height*.78f,Screen.width*.8f,Screen.height*.17f),presenter.CurrentText,style);
        }
    }
}
