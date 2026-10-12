using System;
using System.Collections.Generic;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AccessibilityPreferences2D : MonoBehaviour,IInputPreferenceSource,IInputAimPreferenceSource
    {
        [SerializeField] private bool reduceFlashes=true;
        [SerializeField] private bool captionsEnabled=true;
        [SerializeField] private bool assistedAim=true;
        [SerializeField] private HuntrX.Gameplay.Characters.CharacterManager2D manager;
        [SerializeField] private AgeVisualFeedback2D[] visuals=new AgeVisualFeedback2D[0];
        private readonly Dictionary<GameplayControl,string> bindings=new Dictionary<GameplayControl,string>();
        private readonly HashSet<GameplayControl> toggles=new HashSet<GameplayControl>();
        public bool TryBindManager(HuntrX.Gameplay.Characters.CharacterManager2D owner)
        {
            if(owner==null) return false;
            if(manager!=owner && manager!=null && manager.ActiveCharacter!=null)
            { var previous=manager.ActiveCharacter.GetComponent<AgeVisualFeedback2D>(); if(previous!=null) previous.SetReducedFlashes(false); }
            manager=owner; return true;
        }
        public bool ReduceFlashes=>reduceFlashes;
        public bool CaptionsEnabled=>captionsEnabled;
        public bool AssistedAim=>assistedAim;
        private void Update() { var active=manager!=null?manager.ActiveCharacter:null; var activeVisual=active!=null?active.GetComponent<AgeVisualFeedback2D>():null; if(activeVisual!=null) activeVisual.SetReducedFlashes(reduceFlashes); if(visuals!=null && visuals.Length<=32) foreach(var visual in visuals) if(visual!=null) visual.SetReducedFlashes(reduceFlashes); }
        public string KeyboardBinding(GameplayControl control)=>bindings.TryGetValue(control,out var path)?path:null;
        public bool ToggleModeFor(GameplayControl control)=>toggles.Contains(control);
        public bool TryRemap(GameplayControl control,string keyboardPath)
        {
            if(!Enum.IsDefined(typeof(GameplayControl),control) || string.IsNullOrWhiteSpace(keyboardPath) || !keyboardPath.StartsWith("<Keyboard>/",StringComparison.Ordinal) || keyboardPath.Length>48) return false;
            string key=keyboardPath.Substring(11);
            foreach(char value in key) if(!char.IsLetterOrDigit(value)) return false;
            if(key.Length<1) return false;
            bool single=key.Length==1 && ((key[0]>='a' && key[0]<='z') || (key[0]>='A' && key[0]<='Z') || (key[0]>='0' && key[0]<='9'));
            bool named=!char.IsDigit(key[0]) && Enum.TryParse<UnityEngine.InputSystem.Key>(key,true,out var parsed) && Enum.IsDefined(typeof(UnityEngine.InputSystem.Key),parsed) && parsed!=UnityEngine.InputSystem.Key.None;
            if(!single && !named) return false;
            if(single && key[0]>='0' && key[0]<='9') keyboardPath="<Keyboard>/digit"+key;
            bindings[control]=keyboardPath; return true;
        }
        public void ClearRemap(GameplayControl control) { bindings.Remove(control); }
        public bool TrySetToggle(GameplayControl control,bool enabled)
        {
            if(control!=GameplayControl.Jump && control!=GameplayControl.Parry && control!=GameplayControl.Interact) return false;
            if(enabled) toggles.Add(control); else toggles.Remove(control); return true;
        }
        public void SetPresentationPreferences(bool reducedFlashes,bool captions,bool aimAssist) { reduceFlashes=reducedFlashes; captionsEnabled=captions; assistedAim=aimAssist; }
    }
}
