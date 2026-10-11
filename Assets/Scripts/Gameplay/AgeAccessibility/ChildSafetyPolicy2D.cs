using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    public enum ChildSensitiveFeature { Chat, LootBoxes, StaminaGate, IntrusiveAds, Purchases, PersonalData }
    [DisallowMultipleComponent]
    public sealed class ChildSafetyPolicy2D : MonoBehaviour
    {
        public bool Allows(ChildSensitiveFeature feature)=>false;
        public bool TryRequest(ChildSensitiveFeature feature)=>false;
        public bool AcceptsProfileIdentifier(string identifier)=>System.Guid.TryParseExact(identifier,"N",out _);
    }
}
