using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    [DisallowMultipleComponent]
    public sealed class SlicePlayerPresentation2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private Camera followCamera;
        private Sprite sprite;
        private DamageReceiver2D current;
        private GameObject visual;
        private bool characterVisible = true;
        public void SetCharacterVisible(bool visible) { characterVisible = visible; if (visual != null) visual.GetComponent<SpriteRenderer>().enabled = visible; }
        public bool TryBind(CharacterManager2D owner, Camera camera)
        {
            if (owner == null || camera == null) return false;
            Unbind(); manager = owner; followCamera = camera;
            if (isActiveAndEnabled) { manager.ActiveCharacterChanged += OnActor; OnActor(null, manager.ActiveCharacter); }
            return true;
        }
        private void Awake() => sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        private void OnEnable() { if (manager != null) { manager.ActiveCharacterChanged += OnActor; OnActor(null, manager.ActiveCharacter); } }
        private void OnActor(DamageReceiver2D before, DamageReceiver2D after)
        {
            if (visual != null) Destroy(visual);
            current = after;
            if (after == null) { visual = null; return; }
            visual = new GameObject("Player prototype silhouette"); visual.transform.SetParent(after.transform, false);
            visual.transform.localScale = new Vector3(0.6f, 1.2f, 1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.enabled = characterVisible; renderer.sortingOrder = 1;
            string identity = manager.ActivePrefab != null ? manager.ActivePrefab.name : string.Empty;
            renderer.color = identity.Contains("Mira") ? new Color(1f, 0.35f, 0.7f) : identity.Contains("Zoey") ? new Color(0.3f, 0.85f, 1f) : new Color(0.7f, 0.4f, 1f);
        }
        private void LateUpdate()
        {
            if (current == null || followCamera == null) return;
            Vector3 point = current.transform.position + new Vector3(2f, 1.5f, -10f);
            followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, point, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
        }
        private void Unbind() { if (manager != null) manager.ActiveCharacterChanged -= OnActor; if (visual != null) Destroy(visual); current = null; visual = null; }
        private void OnDisable() => Unbind();
        private void OnDestroy() { Unbind(); if (sprite != null) Destroy(sprite); }
    }
}
