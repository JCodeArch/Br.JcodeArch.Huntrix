using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using UnityEngine;
namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Explicit prototype encounter wiring; no controls, camera, save, or global player lookup.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyEncounterBootstrap2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D playerManager;
        [SerializeField] private CheckpointAttemptFlow2D playerFlow;
        [SerializeField] private GameObject initialPlayerPrefab;
        [SerializeField] private Transform stageStart;
        [SerializeField] private string stageStartId = "enemy-lab-start";
        [SerializeField] private HordeController2D horde;
        [SerializeField] private bool autoStart = true;
        private bool starting;
        private bool started;
        private void Start()
        {
            if (autoStart && !TryBegin()) Debug.LogError("Enemy encounter bootstrap rejected its configuration; fix references before retrying.", this);
        }
        public bool TryBegin()
        {
            if (!isActiveAndEnabled || starting || started || playerManager == null || horde == null ||
                !playerManager.isActiveAndEnabled || !horde.isActiveAndEnabled) return false;
            starting = true;
            try
            {
                if (playerManager.ActiveCharacter == null &&
                    (!playerManager.Configure(initialPlayerPrefab, playerFlow, stageStart, stageStartId) || !playerManager.TrySpawn())) return false;
                if (!isActiveAndEnabled || !playerManager.isActiveAndEnabled ||
                    playerManager.State != CharacterManagerState2D.Alive || playerManager.ActiveCharacter == null ||
                    !playerManager.ActiveCharacter.IsAlive || !horde.isActiveAndEnabled || !horde.TryStart()) return false;
                started = true;
                return true;
            }
            finally { starting = false; }
        }
        private void OnDisable() { if (horde != null) horde.Stop(); started = false; }
    }
}
