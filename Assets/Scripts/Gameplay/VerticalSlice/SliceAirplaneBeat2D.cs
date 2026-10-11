using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceAirplaneBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private GameObject groundEnemy;
        [SerializeField] private GameObject flyingEnemy;
        private VerticalSliceSession2D session;
        public override string Title => "Avião: avance e vença os demônios";
        public override bool TryBegin(VerticalSliceSession2D owner)
        {
            if (owner == null || groundEnemy == null || flyingEnemy == null || !owner.TryLoadStage(stage)) return false;
            session = owner; IsComplete = false;
            for (int i = 0; i < stage.EnemySpawnCount; i++) if (!owner.TrySpawnEnemy(groundEnemy, stage.GetEnemySpawn(i), out _)) return false;
            return owner.Aerial.Configure(owner.Stage, owner.Manager, flyingEnemy) && owner.Aerial.TrySpawn() && owner.TryBeginPerformance();
        }
        public override void Tick(float deltaTime)
        {
            if (session == null || session.Manager.ActiveCharacter == null || session.LivingEnemies > 0) return;
            for (int i = 0; i < session.Aerial.Count; i++) if (session.Aerial.GetAt(i) != null && session.Aerial.GetAt(i).Self.IsAlive) return;
            IsComplete = Vector2.Distance(session.Manager.ActiveCharacter.transform.position, session.Stage.EndPoint.position) <= 2f;
        }
        public override void Exit() { base.Exit(); session = null; }
    }
}
