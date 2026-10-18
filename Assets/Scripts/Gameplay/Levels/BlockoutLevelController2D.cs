using System;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    [DisallowMultipleComponent]
    public sealed class BlockoutLevelController2D : MonoBehaviour
    {
        [SerializeField] private StageBlockoutBuilder2D builder;
        [SerializeField] private StageRouteDefinition route;
        [SerializeField] private int initialStageIndex;
        [SerializeField] private CharacterManager2D player;
        [SerializeField] private CheckpointAttemptFlow2D flow;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool autoStart = true;
        private bool loading;
        public int CurrentStageIndex { get; private set; } = -1;
        public event Action<StageBlockoutDefinition> StageLoaded;
        private void Start() { if (autoStart && !TryLoad(initialStageIndex)) Debug.LogError("Blockout scene could not initialize.", this); }
        public bool TryLoad(int index)
        {
            if (!isActiveAndEnabled || loading || builder == null || route == null || !route.IsValid(out _) ||
                index < 0 || index >= route.StageCount || player == null || flow == null ||
                playerPrefab == null || playerPrefab.scene.IsValid()) return false;
            StageBlockoutDefinition stage = route.GetStage(index);
            loading = true;
            try
            {
                player.enabled = false;
                if (!isActiveAndEnabled) return false;
                builder.TryClear();
                if (!isActiveAndEnabled || builder == null || !builder.isActiveAndEnabled) return false;
                player.enabled = true;
                if (!builder.TryBuild(stage) || !player.Configure(playerPrefab, flow, builder.StartPoint, stage.StageId + ":start") || !player.TrySpawn())
                { player.enabled = false; builder.TryClear(); CurrentStageIndex = -1; return false; }
                if (!isActiveAndEnabled || !player.isActiveAndEnabled || player.ActiveCharacter == null || builder.RuntimeRoot == null)
                { player.enabled = false; builder.TryClear(); CurrentStageIndex = -1; return false; }
                CurrentStageIndex = index;
                foreach (CheckpointAnchor2D checkpoint in builder.CheckpointAnchors)
                {
                    BoxCollider2D area = checkpoint.gameObject.AddComponent<BoxCollider2D>();
                    area.isTrigger = true; area.size = new Vector2(1.5f, 2f);
                    checkpoint.gameObject.AddComponent<BlockoutCheckpointRegion2D>().Configure(checkpoint, flow, player);
                }
                Action<StageBlockoutDefinition> handlers = StageLoaded;
                if (handlers != null) foreach (Action<StageBlockoutDefinition> handler in handlers.GetInvocationList())
                    try { handler(stage); } catch (Exception exception) { Debug.LogException(exception, this); }
                return isActiveAndEnabled && builder.RuntimeRoot != null && player.ActiveCharacter != null;
            }
            finally { loading = false; }
        }
        private void OnDisable() { if (player != null) player.enabled = false; if (builder != null) builder.TryClear(); CurrentStageIndex = -1; }
        private void OnGUI()
        {
            if (route == null || CurrentStageIndex < 0 || CurrentStageIndex >= route.StageCount) return;
            GUILayout.BeginArea(new Rect(12, 12, 320, 95), GUI.skin.box);
            GUILayout.Label("Fase " + (CurrentStageIndex + 1) + "/" + route.StageCount + ": " + route.GetStage(CurrentStageIndex).DisplayName);
            if (GUILayout.Button("Próxima fase (" + ((CurrentStageIndex + 1) % route.StageCount + 1) + "/" + route.StageCount + ")")) TryLoad((CurrentStageIndex + 1) % route.StageCount);
            if (GUILayout.Button("Voltar à Campanha Principal")) UnityEngine.SceneManagement.SceneManager.LoadScene("VerticalSlice");
            GUILayout.EndArea();
        }
    }
}
