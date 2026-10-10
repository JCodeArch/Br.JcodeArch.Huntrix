using UnityEngine;
namespace HuntrX.Gameplay.Equipment
{
    [DisallowMultipleComponent,RequireComponent(typeof(Collider2D))]
    public sealed class EquipmentStoryPoint2D : MonoBehaviour
    {
        [SerializeField] private string narrativePointId;
        public bool TryComplete(EquipmentProgress2D progress)=>isActiveAndEnabled && progress!=null && progress.TryCompleteNarrativePoint(narrativePointId);
        private void OnTriggerEnter2D(Collider2D collider)
        {
            var progress=collider.GetComponentInParent<EquipmentProgress2D>();
            if(progress!=null) TryComplete(progress);
        }
    }
}
