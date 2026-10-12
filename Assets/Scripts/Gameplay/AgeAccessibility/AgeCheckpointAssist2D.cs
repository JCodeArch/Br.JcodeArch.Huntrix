using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent,RequireComponent(typeof(CircleCollider2D))]
    public sealed class AgeCheckpointAssist2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private CheckpointAttemptFlow2D flow;
        [SerializeField] private CheckpointAnchor2D checkpoint;
        private CircleCollider2D trigger;
        private void Awake() => trigger=GetComponent<CircleCollider2D>();
        private void Update()
        {
            bool enabled=profiles!=null && profiles.isActiveAndEnabled && profiles.Current!=null && profiles.Current.IsValid(out _) && profiles.Current.CheckpointAssistance;
            trigger.isTrigger=true; trigger.enabled=enabled;
            if(enabled) trigger.radius=profiles.Current.CheckpointRadius;
        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if(!isActiveAndEnabled || profiles==null || !profiles.isActiveAndEnabled || profiles.Current==null || !profiles.Current.IsValid(out _) || !profiles.Current.CheckpointAssistance) return;
            var actor=collider.GetComponentInParent<DamageReceiver2D>();
            if(actor!=null && actor.isActiveAndEnabled && actor.IsAlive && flow!=null && flow.isActiveAndEnabled && flow.BoundActor==actor && checkpoint!=null) flow.ActivateCheckpoint(checkpoint);
        }
    }
}
