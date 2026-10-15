using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Hud;
using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI
{
    /// <summary>Read-only session bridge. Owns no gameplay input, pause, or session lifecycle.</summary>
    [DisallowMultipleComponent]
    public sealed class SliceReadOnlyHud2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private SliceHud2D owner;
        private SliceHud2D claimedOwner;
        private Canvas canvas;
        private SliceReadOnlyHudView2D view;
        private SliceUiState2D lastState;
        private bool hasState;

        private void Awake()
        {
            var root = new GameObject("ReadOnlyHudCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.enabled = false;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SliceUiTokens.ReferenceResolution;
            scaler.matchWidthOrHeight = .5f;
            var safe = new GameObject("SafeArea", typeof(RectTransform), typeof(SliceSafeAreaFitter));
            safe.transform.SetParent(root.transform, false);
            safe.GetComponent<SliceSafeAreaFitter>().ApplySafeArea();
            view = safe.AddComponent<SliceReadOnlyHudView2D>();
        }

        private void LateUpdate()
        {
            if (claimedOwner != owner) Release();
            bool visible = session != null && session.isActiveAndEnabled && session.IsRunning && owner != null &&
                owner.isActiveAndEnabled && owner.Session == session && !owner.IsOptionsOpen && !session.IsComplete;
            if (!visible || !owner.TryClaimTelemetry(this))
            {
                Release();
                canvas.enabled = false;
                return;
            }
            claimedOwner = owner;
            var state = Capture(session);
            if (!hasState || state != lastState)
            { view.RenderState(state); lastState = state; hasState = true; }
            canvas.enabled = true;
        }

        private void Release()
        {
            if (claimedOwner != null) claimedOwner.ReleaseTelemetry(this);
            claimedOwner = null;
        }
        private void OnDisable() { Release(); if (canvas != null) canvas.enabled = false; }
        private void OnDestroy() { Release(); if (canvas != null) Destroy(canvas.gameObject); }

        // Negative values are presentation-only unavailable sentinels, never gameplay state.
        // SpecialCharge is raw charge here, not the lab view's normalized progress fraction.
        public static SliceUiState2D Capture(VerticalSliceSession2D source)
        {
            var state = new SliceUiState2D {
                HealthCurrent = -1, HonmoonCurrent = -1, SpecialCharge = -1,
                FoodCount = -1, FansRescued = -1, ActiveCharacter = "indisponível", BeatTitle = "Preparando"
            };
            if (source == null || !source.isActiveAndEnabled) return state;
            state.BeatTitle = source.BeatTitle;
            state.IsRunning = source.IsRunning;
            state.IsComplete = source.IsComplete;
            var manager = source.Manager;
            var actor = manager != null && manager.isActiveAndEnabled ? manager.ActiveCharacter : null;
            if (actor != null && actor.isActiveAndEnabled && Finite(actor.CurrentHealth) && Finite(actor.MaximumHealth))
            {
                state.HealthCurrent = Mathf.CeilToInt(actor.CurrentHealth);
                state.HealthMaximum = Mathf.CeilToInt(actor.MaximumHealth);
                state.ActiveCharacter = manager.ActivePrefab != null ? manager.ActivePrefab.name : actor.name;
            }
            if (source.Honmoon != null && source.Honmoon.isActiveAndEnabled && source.Honmoon.Definition != null &&
                source.Honmoon.Definition.IsValid(out _) && Finite(source.Honmoon.Value))
                state.HonmoonCurrent = Mathf.RoundToInt(source.Honmoon.Value);
            if (source.Inventory != null && source.Inventory.isActiveAndEnabled) state.FoodCount = source.Inventory.TotalCount;
            if (source.Special != null && source.Special.isActiveAndEnabled && source.Special.Definition != null &&
                source.Special.Definition.IsValid(out _) && Finite(source.Special.Charge))
            { state.SpecialCharge = source.Special.Charge; state.SpecialReady = source.Special.IsReady; }
            if (source.Performance != null && source.Performance.isActiveAndEnabled && source.Performance.IsRunning)
                state.FansRescued = source.Performance.Snapshot.RescuedFans;
            return state;
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
