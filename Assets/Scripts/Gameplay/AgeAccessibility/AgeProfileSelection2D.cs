using System;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AgeProfileSelection2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileData current;
        private bool publishing;
        private int propagationLocks;
        internal void BeginPropagation()=>propagationLocks++;
        internal void EndPropagation()=>propagationLocks=Mathf.Max(0,propagationLocks-1);
        public AgeProfileData Current=>current;
        public event Action<AgeProfileData> ProfileChanged;
        public bool TrySelect(AgeProfileData value)
        {
            if(publishing || propagationLocks>0 || value==null || !value.IsValid(out _)) return false;
            if(current==value) return true;
            current=value; publishing=true;
            try
            {
                var handlers=ProfileChanged;
                if(handlers!=null) foreach(Action<AgeProfileData> handler in handlers.GetInvocationList())
                    try { handler(value); } catch(Exception exception) { Debug.LogException(exception,this); }
                return true;
            }
            finally { publishing=false; }
        }
    }
}
