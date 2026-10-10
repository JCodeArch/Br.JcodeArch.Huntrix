using System;
using UnityEngine;
namespace HuntrX.Gameplay.Equipment
{
    /// <summary>Prototype-only local profile binding; no storage or narrative reward at startup.</summary>
    [RequireComponent(typeof(EquipmentProgress2D),typeof(EquipmentMechanics2D))]
    public sealed class EquipmentPrototypeBinding2D : MonoBehaviour
    {
        private EquipmentProgress2D progress;
        private EquipmentMechanics2D mechanics;
        private void Awake() { progress=GetComponent<EquipmentProgress2D>(); mechanics=GetComponent<EquipmentMechanics2D>(); }
        private void Start() { if(progress.ProfileId==null) progress.TryBindProfile(Guid.NewGuid().ToString("N")); mechanics.TryBindProgression(progress); }
        private void OnEnable() { if(progress!=null) progress.PieceUnlocked+=AutoEquip; }
        private void OnDisable() { if(progress!=null) progress.PieceUnlocked-=AutoEquip; }
        private void AutoEquip(HuntrX.Data.EquipmentPieceDefinition piece) => mechanics.TryEquip(piece);
    }
}
