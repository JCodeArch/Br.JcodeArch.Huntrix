using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceMinibossBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private GameObject minibossPrefab;
        private VerticalSliceSession2D session;
        private HuntrX.Gameplay.Fans.FanRescueController2D interacting;
        public override string Title => "Mini-boss: vença o chefe e proteja os fãs";
        public override bool TryBegin(VerticalSliceSession2D value)
        {
            if (value == null || minibossPrefab == null || !value.TryLoadStage(stage) || value.Fans.RuntimeRoot == null) return false;
            session = value; IsComplete = false; value.Fans.RuntimeRoot.transform.position = stage.ArenaCenter;
            return value.BossArena.Configure(value.Stage, value.Manager, minibossPrefab, value.Fans.Registry, value.Honmoon, value.Performance) && value.BossArena.TrySpawn();
        }
        public override void Tick(float deltaTime) { if (session != null) IsComplete = session.BossArena.IsComplete; }
        public override void OnInput(SliceInputFrame frame)
        {
            if (session == null) return;
            if (!frame.InteractHeld || frame.InteractReleased) { interacting?.Cancel(); interacting = null; return; }
            var fan = session.NearestFan(); var rescue = fan != null ? fan.GetComponent<HuntrX.Gameplay.Fans.FanRescueController2D>() : null;
            if (rescue != null && !rescue.IsRecovering && rescue.TryBeginRescue(session.Manager.ActiveCharacter)) interacting = rescue;
        }
        public override void Exit() { interacting?.Cancel(); interacting = null; if (session != null && session.BossArena != null) session.BossArena.Clear(); session = null; base.Exit(); }
    }
}
