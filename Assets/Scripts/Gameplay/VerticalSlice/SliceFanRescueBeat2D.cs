using HuntrX.Data;
using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceFanRescueBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private GameObject drainerPrefab;
        private VerticalSliceSession2D session;
        private FanRescueController2D interacting;
        private FanSoulDrainController2D drain;
        private bool subscribed;
        public override string Title => "Resgate sob pressão: segure E perto de um fã";
        public override bool TryBegin(VerticalSliceSession2D owner)
        {
            if (owner == null || drainerPrefab == null || !owner.TryLoadStage(stage) || owner.Fans.RuntimeRoot == null) return false;
            session = owner; IsComplete = false;
            owner.Fans.RuntimeRoot.transform.position = stage.ArenaCenter;
            if (!owner.TrySpawnEnemy(drainerPrefab, stage.ArenaCenter + Vector2.left * 2f, out var enemy)) return false;
            drain = enemy.GetComponent<FanSoulDrainController2D>();
            if (drain == null || !drain.TryBindRegistry(owner.Fans.Registry)) return false;
            drain.FanDrainAccepted += Drained; owner.Fans.RescueCompleted += Rescued; subscribed = true;
            return true;
        }
        public override void OnInput(SliceInputFrame frame)
        {
            if (session == null) return;
            if (frame.InteractReleased || !frame.InteractHeld) { interacting?.Cancel(); interacting = null; return; }
            if (frame.Interact || interacting == null)
            {
                FanActor2D fan = session.NearestFan();
                FanRescueController2D rescue = fan != null ? fan.GetComponent<FanRescueController2D>() : null;
                if (rescue != null && rescue.TryBeginRescue(session.Manager.ActiveCharacter)) interacting = rescue;
            }
        }
        private void Drained(FanActor2D fan, float actualLost) { if (session != null) session.Honmoon.TryChange(-actualLost * .25f); }
        private void Rescued(FanActor2D fan, int reward) { if (session != null) { var owner = session; owner.Honmoon.TryChange(10f); if (session == owner && owner.isActiveAndEnabled && owner.IsRunning) IsComplete = true; } }
        public override void Exit()
        {
            interacting?.Cancel(); interacting = null;
            if (subscribed) { if (drain != null) drain.FanDrainAccepted -= Drained; if (session != null && session.Fans != null) session.Fans.RescueCompleted -= Rescued; }
            subscribed = false; drain = null; session = null; base.Exit();
        }
    }
}
