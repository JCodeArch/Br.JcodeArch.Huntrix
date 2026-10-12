using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.FanEncounters
{
    [DisallowMultipleComponent, RequireComponent(typeof(FanEncounterController2D))]
    public sealed class FanEncounterBootstrap2D : MonoBehaviour
    {
        [SerializeField] private FanEncounterDefinition definition;
        [SerializeField] private Transform origin;
        [SerializeField] private bool autoStart = true;
        private FanEncounterController2D controller;
        private void Awake() => controller = GetComponent<FanEncounterController2D>();
        private void Start() { if (autoStart && !TryStart()) Debug.LogError("Fan encounter requires an explicit definition and origin.",this); }
        public bool TryStart() => isActiveAndEnabled && controller != null && origin != null && controller.TryStart(definition, origin.position);
        private void OnDisable() { if (controller != null) controller.Stop(); }
    }
}
