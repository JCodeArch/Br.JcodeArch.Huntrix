using HuntrX.Data;
using HuntrX.Gameplay.Resources;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceRamenBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition stage;
        [SerializeField] private GameObject pickupPrefab;
        private VerticalSliceSession2D session;
        private GameObject owned;
        private FoodPickup2D pickup;
        private bool collected;
        public override string Title => "Ramen: colete comida. R para você; F para um fã";
        public override bool TryBegin(VerticalSliceSession2D owner)
        {
            if (owner == null || pickupPrefab == null || pickupPrefab.GetComponent<FoodPickup2D>() == null || !owner.TryLoadStage(stage)) return false;
            session = owner; collected = false; IsComplete = false;
            owned = Instantiate(pickupPrefab, stage.StartPosition + Vector2.right * 3f, Quaternion.identity);
            owned.SetActive(true); pickup = owned.GetComponent<FoodPickup2D>();
            return isActiveAndEnabled && pickup != null;
        }
        public override void Tick(float deltaTime)
        {
            if (session == null || session.Manager.ActiveCharacter == null) return;
            var owner = session; var actor = session.Manager.ActiveCharacter;
            if (!collected && pickup != null && Vector2.Distance(session.Manager.ActiveCharacter.transform.position, pickup.transform.position) <= 1.2f)
                collected = pickup.TryCollect(session.Inventory);
            if (session != owner || !owner.isActiveAndEnabled || !owner.IsRunning || owner.Manager.ActiveCharacter != actor || !actor.IsAlive) return;
            IsComplete = collected && Vector2.Distance(session.Manager.ActiveCharacter.transform.position, session.Stage.EndPoint.position) <= 2f;
        }
        public override void Exit() { base.Exit(); if (owned != null) { owned.SetActive(false); Destroy(owned); } owned = null; pickup = null; session = null; }
    }
}
