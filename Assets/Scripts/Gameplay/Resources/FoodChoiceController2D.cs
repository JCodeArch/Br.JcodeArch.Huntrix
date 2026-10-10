using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.Resources
{
    [DisallowMultipleComponent,RequireComponent(typeof(FoodInventory2D))]
    public sealed class FoodChoiceController2D : MonoBehaviour
    {
        [SerializeField] private DamageReceiver2D self;
        [SerializeField] private float fanUseRange=3f;
        private FoodInventory2D inventory;
        private void Awake() { inventory=GetComponent<FoodInventory2D>(); if(self==null) self=GetComponentInParent<DamageReceiver2D>(); }
        public bool TryBindSelf(DamageReceiver2D value)
        {
            if(inventory==null) inventory=GetComponent<FoodInventory2D>();
            if(inventory==null || inventory.IsBusy || (value!=null && (!value.isActiveAndEnabled || !value.IsAlive || value.Faction!=CombatFaction2D.HuntrX))) return false;
            self=value; return true;
        }
        public bool TryUseOnSelf(FoodDefinition food) => Ready(food) && inventory.TryConsume(food,()=>self.TryRestoreHealth(food.SelfRecovery,self));
        public bool TryUseOnFan(FoodDefinition food,FanActor2D fan)
        {
            if(!Ready(food) || fan==null || !fan.isActiveAndEnabled || !FinitePositive(fanUseRange) ) return false;
            float distance=Vector2.Distance(self.transform.position,fan.transform.position);
            if(float.IsNaN(distance) || float.IsInfinity(distance) || distance>fanUseRange) return false;
            return inventory.TryConsume(food,()=>fan.TryRecover(food.FanRecovery));
        }
        private bool Ready(FoodDefinition food) => isActiveAndEnabled && inventory!=null && inventory.isActiveAndEnabled && self!=null && self.isActiveAndEnabled && self.IsAlive && food!=null && food.IsValid(out _);
        private static bool FinitePositive(float value)=>value>0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
