using System;
using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent]
    public sealed class FanDrainEncounterBootstrap2D : MonoBehaviour
    {
        [SerializeField] private FanRegistry2D registry;
        [SerializeField] private GameObject drainerPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool autoStart = true;
        private GameObject owned;
        private bool creating;
        public FanSoulDrainController2D ActiveDrainer { get; private set; }
        public event Action<FanSoulDrainController2D> DrainerCreated;
        private void Start() { if (autoStart && !TryStart()) Debug.LogError("Fan drain encounter configuration is invalid.", this); }
        public bool TryStart()
        {
            if (!isActiveAndEnabled || creating || owned != null || registry == null || !registry.isActiveAndEnabled ||
                drainerPrefab == null || drainerPrefab.scene.IsValid() || spawnPoint == null ||
                drainerPrefab.GetComponent<FanSoulDrainController2D>() == null) return false;
            Vector3 position = spawnPoint.position; Quaternion rotation = spawnPoint.rotation;
            if (!Finite(position.x) || !Finite(position.y) || !Finite(position.z) || !Finite(rotation.x) || !Finite(rotation.y) ||
                !Finite(rotation.z) || !Finite(rotation.w) || (rotation.x == 0f && rotation.y == 0f && rotation.z == 0f && rotation.w == 0f)) return false;
            creating = true;
            GameObject candidate = null;
            try
            {
                candidate = Instantiate(drainerPrefab, position, rotation); // independent enemy root
                candidate.SetActive(true);
                FanSoulDrainController2D controller = candidate.GetComponent<FanSoulDrainController2D>();
                if (!isActiveAndEnabled || controller == null || !controller.TryBindRegistry(registry))
                { candidate.SetActive(false); Destroy(candidate); return false; }
                owned = candidate; ActiveDrainer = controller;
                Action<FanSoulDrainController2D> handlers = DrainerCreated;
                if (handlers != null)
                    foreach (Action<FanSoulDrainController2D> handler in handlers.GetInvocationList())
                        try { handler(controller); } catch (Exception exception) { Debug.LogException(exception, this); }
                return isActiveAndEnabled && owned != null;
            }
            finally { creating = false; }
        }
        public void Stop()
        {
            GameObject previous = owned; owned = null; ActiveDrainer = null;
            if (previous != null) { previous.SetActive(false); Destroy(previous); }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private void OnDisable() => Stop();
        private void OnDestroy() => Stop();
    }
}
