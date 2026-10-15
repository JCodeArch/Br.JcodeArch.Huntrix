using UnityEngine;

namespace HuntrX.Presentation.UI
{
    /// <summary>
    /// Fits a RectTransform inside the device's safe area by converting Screen.safeArea
    /// into normalized anchors, without hardcoding orientations or breaking on asymmetric notches.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SliceSafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool applyOnUpdate = false;

        private RectTransform targetRect;
        private Rect lastSafeArea = Rect.zero;
        private Vector2Int lastScreenSize = Vector2Int.zero;

        private void Awake()
        {
            targetRect = GetComponent<RectTransform>();
            if (applyOnAwake)
            {
                ApplySafeArea();
            }
        }

        private void OnEnable()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            if (applyOnUpdate || HasScreenOrSafeAreaChanged() || !Application.isPlaying)
            {
                if (HasScreenOrSafeAreaChanged())
                {
                    ApplySafeArea();
                }
            }
        }

        public bool HasScreenOrSafeAreaChanged()
        {
            Rect currentSafeArea = Screen.safeArea;
            Vector2Int currentScreen = new Vector2Int(Screen.width, Screen.height);
            return currentSafeArea != lastSafeArea || currentScreen != lastScreenSize;
        }

        public void ApplySafeArea()
        {
            if (targetRect == null)
            {
                targetRect = GetComponent<RectTransform>();
            }

            if (targetRect == null)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            var (anchorMin, anchorMax) = CalculateNormalizedSafeArea(safeArea, screenSize);

            targetRect.anchorMin = anchorMin;
            targetRect.anchorMax = anchorMax;
            targetRect.offsetMin = Vector2.zero;
            targetRect.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }

        public static (Vector2 anchorMin, Vector2 anchorMax) CalculateNormalizedSafeArea(Rect safeArea, Vector2 screenSize)
        {
            // A transient empty/invalid report must not collapse or invert the entire UI.
            // Keep a usable full-screen fallback until the next valid screen report.
            if (!IsFinite(screenSize.x) || !IsFinite(screenSize.y) ||
                screenSize.x <= 0f || screenSize.y <= 0f ||
                !IsFinite(safeArea.x) || !IsFinite(safeArea.y) ||
                !IsFinite(safeArea.width) || !IsFinite(safeArea.height) ||
                safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return (Vector2.zero, Vector2.one);
            }

            Vector2 anchorMin = new Vector2(
                Mathf.Clamp01(safeArea.x / screenSize.x),
                Mathf.Clamp01(safeArea.y / screenSize.y)
            );

            Vector2 anchorMax = new Vector2(
                Mathf.Clamp01((safeArea.x + safeArea.width) / screenSize.x),
                Mathf.Clamp01((safeArea.y + safeArea.height) / screenSize.y)
            );

            if (anchorMax.x <= anchorMin.x || anchorMax.y <= anchorMin.y)
            {
                return (Vector2.zero, Vector2.one);
            }

            return (anchorMin, anchorMax);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
