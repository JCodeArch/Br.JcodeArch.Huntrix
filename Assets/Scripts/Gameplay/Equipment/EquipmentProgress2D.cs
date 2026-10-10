using System;
using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Equipment
{
    [DisallowMultipleComponent]
    public sealed class EquipmentProgress2D : MonoBehaviour
    {
        [SerializeField] private EquipmentPieceDefinition[] catalog=new EquipmentPieceDefinition[0];
        private readonly HashSet<string> unlocked=new HashSet<string>();
        private string profileId;
        private bool unlocking;
        public string ProfileId=>profileId;
        public event Action<EquipmentPieceDefinition> PieceUnlocked;
        public bool TryBindProfile(string opaqueId)
        {
            if(unlocking || !Guid.TryParseExact(opaqueId,"N",out _)) return false;
            if(profileId==opaqueId) return true;
            if(profileId!=null) return false;
            profileId=opaqueId; return true;
        }
        public bool IsUnlocked(EquipmentPieceDefinition piece)=>piece!=null && profileId!=null && unlocked.Contains(piece.Id);
        public string[] SnapshotUnlockedIds() { var result=new string[unlocked.Count]; unlocked.CopyTo(result); return result; }
        public bool TryCompleteNarrativePoint(string pointId)
        {
            if(!isActiveAndEnabled || unlocking || profileId==null || string.IsNullOrWhiteSpace(pointId) || catalog==null || catalog.Length>32) return false;
            unlocking=true; bool changed=false;
            try
            {
                foreach(var piece in catalog)
                {
                    if(piece==null || !piece.IsValid(out _) || piece.NarrativePointId!=pointId || !unlocked.Add(piece.Id)) continue;
                    changed=true; var handlers=PieceUnlocked;
                    if(handlers!=null) foreach(Action<EquipmentPieceDefinition> handler in handlers.GetInvocationList())
                        try { handler(piece); } catch(Exception exception) { Debug.LogException(exception,this); }
                    if(!isActiveAndEnabled) break;
                }
                return changed;
            }
            finally { unlocking=false; }
        }
    }
}
