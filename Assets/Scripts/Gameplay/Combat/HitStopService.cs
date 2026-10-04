using UnityEngine;

namespace HuntrX.Gameplay.Combat
{
    /// <summary>
    /// Coordinates the one global game-time freeze used for accepted combat impacts.
    /// The service is also the shared owner for pause requests that must coexist with hit stop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitStopService : MonoBehaviour
    {
        private static HitStopService instance;

        private double hitStopDeadline;
        private float baseTimeScale;
        private bool hasHitStopDeadline;
        private bool isPaused;
        private bool ownsTimeScale;
        private bool registeredAsInstance;

        public static HitStopService Instance => instance;

        /// <summary>
        /// Provides a deterministic fallback for editor tests or scenes that have not run
        /// the pre-scene bootstrap. Runtime startup normally creates the runner first.
        /// </summary>
        public static HitStopService EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            HitStopService[] loadedServices = Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include);
            HitStopService survivor = null;
            foreach (HitStopService candidate in loadedServices)
            {
                if (candidate == null || !candidate.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (survivor == null)
                {
                    survivor = candidate;
                }
                else
                {
                    // Remove only the duplicate component; its host may own other systems.
                    Destroy(candidate);
                }
            }

            if (survivor != null)
            {
                if (!survivor.gameObject.activeSelf)
                {
                    survivor.gameObject.SetActive(true);
                }
                if (!survivor.enabled)
                {
                    survivor.enabled = true;
                }

                survivor.RebindForCurrentSession();
                return survivor;
            }

            var runner = new GameObject("HitStopService");
            return runner.AddComponent<HitStopService>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootstrapBeforeGameplayScenes()
        {
            EnsureInstance();
        }

        private void Awake()
        {
            RegisterAsInstance();
        }

        private void OnEnable()
        {
            RegisterAsInstance();
        }

        private void Update()
        {
            if (!registeredAsInstance)
            {
                return;
            }

            if (DetectOwnershipLoss())
            {
                return;
            }

            if (!hasHitStopDeadline || Time.realtimeSinceStartupAsDouble < hitStopDeadline)
            {
                return;
            }

            hasHitStopDeadline = false;
            hitStopDeadline = 0d;
            if (!isPaused)
            {
                RestoreBaseTimeScale();
            }
        }

        /// <summary>
        /// Requests a global freeze for the given unscaled duration. Overlapping requests
        /// extend the active deadline to the later of the existing and requested deadlines.
        /// Requests are ignored while paused or when Unity was already paused externally.
        /// </summary>
        public void RequestHitStop(float durationSeconds)
        {
            if (!registeredAsInstance || !IsFinitePositive(durationSeconds))
            {
                return;
            }

            if (DetectOwnershipLoss())
            {
                return;
            }

            if (isPaused)
            {
                return;
            }

            if (!hasHitStopDeadline)
            {
                if (Time.timeScale <= 0f)
                {
                    return;
                }

                baseTimeScale = Time.timeScale;
            }

            double requestedDeadline = Time.realtimeSinceStartupAsDouble + durationSeconds;
            if (!hasHitStopDeadline || requestedDeadline > hitStopDeadline)
            {
                hitStopDeadline = requestedDeadline;
            }

            hasHitStopDeadline = true;
            ownsTimeScale = true;
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Applies or removes a pause through the shared time-scale owner. An active hit-stop
        /// deadline remains pending through pause and is reapplied on resume if still unexpired.
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (!registeredAsInstance || DetectOwnershipLoss())
            {
                return;
            }

            if (paused)
            {
                if (isPaused)
                {
                    return;
                }

                if (!ownsTimeScale)
                {
                    if (Time.timeScale <= 0f)
                    {
                        // Preserve an external pause; this owner has no base scale to restore.
                        isPaused = true;
                        return;
                    }

                    baseTimeScale = Time.timeScale;
                }

                isPaused = true;
                ownsTimeScale = true;
                Time.timeScale = 0f;
                return;
            }

            if (!isPaused)
            {
                return;
            }

            isPaused = false;
            if (hasHitStopDeadline && Time.realtimeSinceStartupAsDouble < hitStopDeadline)
            {
                ownsTimeScale = true;
                Time.timeScale = 0f;
                return;
            }

            hasHitStopDeadline = false;
            hitStopDeadline = 0d;
            RestoreBaseTimeScale();
        }

        private void RegisterAsInstance()
        {
            if (registeredAsInstance)
            {
                return;
            }

            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            registeredAsInstance = true;
            DontDestroyOnLoad(gameObject);
        }

        private void RebindForCurrentSession()
        {
            // Domain reload can clear statics without destroying this persistent component.
            // Release any scale ownership carried from the previous session before rebinding.
            ReleaseOwnedTimeScale();
            baseTimeScale = Time.timeScale;
            instance = this;
            registeredAsInstance = true;
            DontDestroyOnLoad(gameObject);
        }

        private bool DetectOwnershipLoss()
        {
            if (!ownsTimeScale || Time.timeScale == 0f)
            {
                return false;
            }

            // A nonzero direct write means another system took ownership. Preserve it and
            // discard every pending pause/freeze so this service cannot overwrite it later.
            hasHitStopDeadline = false;
            hitStopDeadline = 0d;
            isPaused = false;
            ownsTimeScale = false;
            return true;
        }

        private void RestoreBaseTimeScale()
        {
            if (ownsTimeScale && Time.timeScale == 0f)
            {
                Time.timeScale = baseTimeScale;
            }

            ownsTimeScale = false;
        }

        private void OnDisable()
        {
            ReleaseOwnedTimeScale();
            if (registeredAsInstance && instance == this)
            {
                instance = null;
            }

            registeredAsInstance = false;
        }

        private void OnDestroy()
        {
            ReleaseOwnedTimeScale();
            if (instance == this)
            {
                instance = null;
            }

            registeredAsInstance = false;
        }

        private void ReleaseOwnedTimeScale()
        {
            RestoreBaseTimeScale();
            hasHitStopDeadline = false;
            hitStopDeadline = 0d;
            isPaused = false;
        }

        private static bool IsFinitePositive(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }
}
