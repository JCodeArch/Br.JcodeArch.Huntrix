using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    public sealed class BlockoutCheckpointRegion2D : MonoBehaviour
    {
        private CheckpointAnchor2D anchor;
        private CheckpointAttemptFlow2D flow;
        private CharacterManager2D player;
        public void Configure(CheckpointAnchor2D checkpoint, CheckpointAttemptFlow2D attempt, CharacterManager2D owner)
        { anchor = checkpoint; flow = attempt; player = owner; }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (flow == null || player == null || other == null || anchor == null) return;
            if (other.GetComponentInParent<DamageReceiver2D>() == player.ActiveCharacter) flow.ActivateCheckpoint(anchor);
        }
    }
}
