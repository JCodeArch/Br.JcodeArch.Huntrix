using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent,RequireComponent(typeof(AccessibilityPreferences2D))]
    public sealed class AccessibilityPanel2D : MonoBehaviour
    {
        [SerializeField] private bool visible=true;
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private AgeProfileData[] ageOptions=new AgeProfileData[0];
        private AccessibilityPreferences2D preferences;
        private string key="space";
        private GameplayControl selected=GameplayControl.Jump;
        private string feedback;
        private void Awake()=>preferences=GetComponent<AccessibilityPreferences2D>();
        private void OnGUI()
        {
            if(!visible || preferences==null || !preferences.isActiveAndEnabled) return;
            GUILayout.BeginArea(new Rect(Mathf.Max(0,Screen.width-310),10,300,410),GUI.skin.box);
            GUILayout.Label("Acessibilidade");
            if(profiles!=null && ageOptions!=null && ageOptions.Length<=5)
            {
                GUILayout.Label("Faixa etária: "+(profiles.Current!=null?profiles.Current.Band.ToString():"não definida"));
                if(GUILayout.Button("Trocar faixa etária"))
                {
                    int index=System.Array.IndexOf(ageOptions,profiles.Current);
                    if(ageOptions.Length>0) profiles.TrySelect(ageOptions[(index+1)%ageOptions.Length]);
                }
            }
            bool flashes=GUILayout.Toggle(preferences.ReduceFlashes,"Reduzir flashes");
            bool captions=GUILayout.Toggle(preferences.CaptionsEnabled,"Legendas");
            bool aim=GUILayout.Toggle(preferences.AssistedAim,"Ajuda na mira");
            preferences.SetPresentationPreferences(flashes,captions,aim);
            preferences.TrySetToggle(GameplayControl.Jump,GUILayout.Toggle(preferences.ToggleModeFor(GameplayControl.Jump),"Pulo: alternar em vez de segurar"));
            preferences.TrySetToggle(GameplayControl.Interact,GUILayout.Toggle(preferences.ToggleModeFor(GameplayControl.Interact),"Resgate: alternar em vez de segurar"));
            if(GUILayout.Button("Ação: "+selected)) selected=selected==GameplayControl.Pause?GameplayControl.MoveLeft:(GameplayControl)((int)selected+1);
            key=GUILayout.TextField(key,24);
            if(GUILayout.Button("Aplicar tecla")) feedback=preferences.TryRemap(selected,"<Keyboard>/"+key)?"Tecla atualizada":"Tecla inválida";
            if(GUILayout.Button("Restaurar tecla padrão")) { preferences.ClearRemap(selected); feedback="Tecla padrão restaurada"; }
            if(!string.IsNullOrEmpty(feedback)) GUILayout.Label(feedback);
            GUILayout.EndArea();
        }
    }
}
