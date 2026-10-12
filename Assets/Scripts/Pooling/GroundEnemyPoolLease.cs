using System;
using System.Collections.Generic;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Pooling
{
    [DisallowMultipleComponent]
    public sealed class GroundEnemyPoolLease : MonoBehaviour,IPooledLifecycle
    {
        private static readonly List<MonoBehaviour> inspection=new List<MonoBehaviour>(16);
        public static bool Supports(GameObject instance)
        {
            if(instance==null||instance.GetComponent<GroundEnemyController2D>()==null||instance.GetComponent<EnemyAgent2D>()==null)return false;
            inspection.Clear();instance.GetComponents(inspection);
            if(inspection.Count>16)return false;
            foreach(MonoBehaviour component in inspection)
            {
                if(component==null)return false;
                string type=component.GetType().FullName;
                if(type!="HuntrX.Gameplay.Enemies.GroundEnemyController2D"&&type!="HuntrX.Gameplay.Enemies.EnemyAgent2D"&&
                   type!="HuntrX.Gameplay.Combat.DamageReceiver2D"&&type!="HuntrX.Gameplay.Protection.DamageProtection2D"&&type!="HuntrX.Gameplay.Combat.Hurtbox2D"&&
                   type!="HuntrX.Gameplay.Enemies.EnemyPrototypeVisual2D"&&type!="HuntrX.Pooling.PooledInstance"&&
                   type!="HuntrX.Pooling.GroundEnemyPoolLease"&&type!="HuntrX.Pooling.CombatImpactVfxPool2D"&&type!="HuntrX.Gameplay.AgeAccessibility.AgeDifficultyPolicy2D")return false;
            }
            return true;
        }
        public void PrepareForRent()
        {
            if(!Supports(gameObject)||!GetComponent<EnemyAgent2D>().TryResetForSpawn())
                throw new InvalidOperationException("Ground enemy lease reset rejected; no live/dead action is reused.");
        }
        public void PrepareForReturn(){}
    }
}
