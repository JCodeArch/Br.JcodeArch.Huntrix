using HuntrX.Gameplay.Fans;
using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "FanEncounterDefinition", menuName = "HUNTR/X/Data/Fan Encounter")]
    public sealed class FanEncounterDefinition : GameDataDefinition
    {
        [SerializeField] private GameObject groupPrefab;
        [SerializeField] private float initialSoulLoss = 20f;
        public GameObject GroupPrefab => groupPrefab;
        public float InitialSoulLoss => initialSoulLoss;
        public bool IsValid(out string error)
        {
            bool valid = groupPrefab != null && !groupPrefab.scene.IsValid() && groupPrefab.GetComponent<FanRegistry2D>() != null &&
                !float.IsNaN(initialSoulLoss) && !float.IsInfinity(initialSoulLoss) && initialSoulLoss >= 0f;
            if (valid) { int count = groupPrefab.GetComponentsInChildren<FanActor2D>(true).Length; valid = count > 0 && count <= 64; }
            error = valid ? string.Empty : "Fan encounter requires an explicit registry group of 1..64 fans and finite nonnegative initial loss.";
            return valid;
        }
    }
}
