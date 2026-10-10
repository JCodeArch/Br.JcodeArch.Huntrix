using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Resources
{
    [DisallowMultipleComponent,RequireComponent(typeof(Collider2D))]
    public sealed class FoodPickup2D : MonoBehaviour
    {
        [SerializeField] private FoodDefinition definition;
        [SerializeField] private int quantity=1;
        private bool collected;
        public bool TryCollect(FoodInventory2D inventory)
        {
            if(!isActiveAndEnabled || collected || inventory==null) return false;
            collected=true;
            if(!inventory.TryAdd(definition,quantity)) { collected=false; return false; }
            gameObject.SetActive(false); return true;
        }
        private void OnTriggerEnter2D(Collider2D other) => TryCollect(other.GetComponentInParent<FoodInventory2D>());
    }
}
