using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent, RequireComponent(typeof(FanActor2D))]
    public sealed class FanPrototypeVisual2D : MonoBehaviour
    {
        private FanActor2D fan;
        private SpriteRenderer spriteRenderer;
        private Sprite fallbackSprite;
        private Sprite fanSprite;
        private bool ownsFanSprite;

        private void Awake()
        {
            fan = GetComponent<FanActor2D>();
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 10;
            fallbackSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);

            fanSprite = UnityEngine.Resources.Load<Sprite>("Art/Characters/KpopFan");
            if (fanSprite == null)
            {
                Texture2D tex = UnityEngine.Resources.Load<Texture2D>("Art/Characters/KpopFan");
                if (tex != null)
                {
                    fanSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
                    ownsFanSprite = true;
                }
            }

            if (fanSprite != null && fanSprite.bounds.size.y > 0.01f)
            {
                spriteRenderer.sprite = fanSprite;
                float s = 1.45f / fanSprite.bounds.size.y;
                transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                spriteRenderer.sprite = fallbackSprite;
                transform.localScale = new Vector3(0.5f, 0.8f, 1f);
            }
        }

        private void OnEnable() { fan.StateChanged += OnState; fan.PresentationChanged += OnIntensity; Apply(); }
        private void OnDisable() { fan.StateChanged -= OnState; fan.PresentationChanged -= OnIntensity; }
        private void OnDestroy()
        {
            if (ownsFanSprite && fanSprite != null) Destroy(fanSprite);
            if (fallbackSprite != null) Destroy(fallbackSprite);
        }
        private void OnState(FanActor2D actor, FanState before, FanState after) => Apply();
        private void OnIntensity(FanContentIntensity intensity) => Apply();
        private void Apply()
        {
            if (spriteRenderer == null) return;
            if (fanSprite != null)
            {
                spriteRenderer.color = fan.State == FanState.Normal ? Color.white :
                    fan.State == FanState.Drained ? new Color(1f, 0.9f, 0.55f) :
                    fan.State == FanState.Critical ? new Color(1f, 0.55f, 0.55f) :
                    new Color(0.65f, 0.55f, 0.85f);
                return;
            }
            bool gentle = fan.ContentIntensity == FanContentIntensity.Gentle;
            spriteRenderer.color = fan.State == FanState.Normal ? Color.cyan : fan.State == FanState.Drained ? Color.yellow :
                fan.State == FanState.Critical ? (gentle ? new Color(1f, 0.65f, 0.5f) : Color.red) :
                (gentle ? new Color(0.65f, 0.65f, 0.9f) : new Color(0.35f, 0.15f, 0.45f));
        }
    }
}
