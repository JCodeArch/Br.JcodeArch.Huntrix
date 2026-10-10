using System;
using System.Collections.Generic;
using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Resources
{
    [DisallowMultipleComponent]
    public sealed class FoodInventory2D : MonoBehaviour
    {
        [SerializeField] private int capacity=32;
        private readonly Dictionary<string,int> quantities=new Dictionary<string,int>();
        private bool consuming;
        private bool publishing;
        public bool IsBusy => consuming || publishing;
        public int TotalCount { get; private set; }
        public event Action<FoodDefinition,int> QuantityChanged;
        public int Count(FoodDefinition food) => food!=null && quantities.TryGetValue(food.Id,out var count)?count:0;
        public bool TryAdd(FoodDefinition food,int amount=1)
        {
            if(!isActiveAndEnabled || consuming || publishing || food==null || !food.IsValid(out _) || amount<1 || capacity<1 || capacity>256 || amount>capacity-TotalCount) return false;
            quantities[food.Id]=Count(food)+amount; TotalCount+=amount; Publish(food); return true;
        }
        internal bool TryConsume(FoodDefinition food,Func<bool> benefit)
        {
            if(!isActiveAndEnabled || consuming || publishing || food==null || !food.IsValid(out _) || Count(food)<1 || benefit==null) return false;
            consuming=true;
            try
            {
                quantities[food.Id]=Count(food)-1; TotalCount--;
                bool accepted;
                try { accepted=benefit(); }
                catch { quantities[food.Id]=Count(food)+1; TotalCount++; throw; }
                if(!accepted) { quantities[food.Id]=Count(food)+1; TotalCount++; return false; }
                Publish(food); return true;
            }
            finally { consuming=false; }
        }
        private void Publish(FoodDefinition food)
        {
            int count=Count(food); var handlers=QuantityChanged;
            publishing=true;
            try
            {
                if(handlers!=null) foreach(Action<FoodDefinition,int> handler in handlers.GetInvocationList())
                    try { handler(food,count); } catch(Exception exception) { Debug.LogException(exception,this); }
            }
            finally { publishing=false; }
        }
    }
}
