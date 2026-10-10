using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent, RequireComponent(typeof(FanActor2D))]
    public sealed class FanPrototypeVisual2D : MonoBehaviour
    {
        private FanActor2D fan;
        private SpriteRenderer spriteRenderer;
        private Sprite sprite;
        private void Awake()
        {
            fan = GetComponent<FanActor2D>();
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            spriteRenderer.sprite = sprite;
            transform.localScale = new Vector3(0.5f, 0.8f, 1f);
        }
        private void OnEnable() { fan.StateChanged += OnState; fan.PresentationChanged += OnIntensity; Apply(); }
        private void OnDisable() { fan.StateChanged -= OnState; fan.PresentationChanged -= OnIntensity; }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); }
        private void OnState(FanActor2D actor, FanState before, FanState after) => Apply();
        private void OnIntensity(FanContentIntensity intensity) => Apply();
        private void Apply()
        {
            if (spriteRenderer == null) return;
            bool gentle = fan.ContentIntensity == FanContentIntensity.Gentle;
            spriteRenderer.color = fan.State == FanState.Normal ? Color.cyan : fan.State == FanState.Drained ? Color.yellow :
                fan.State == FanState.Critical ? (gentle ? new Color(1f, 0.65f, 0.5f) : Color.red) :
                (gentle ? new Color(0.65f, 0.65f, 0.9f) : new Color(0.35f, 0.15f, 0.45f));
        }
    }
}
