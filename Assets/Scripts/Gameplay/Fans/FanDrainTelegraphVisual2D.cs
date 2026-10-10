using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent, RequireComponent(typeof(FanSoulDrainController2D))]
    public sealed class FanDrainTelegraphVisual2D : MonoBehaviour
    {
        private FanSoulDrainController2D drain;
        private GameObject visual;
        private Sprite sprite;
        private void Awake()
        {
            drain = GetComponent<FanSoulDrainController2D>();
            visual = new GameObject("Drain telegraph marker");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = Vector3.up;
            visual.transform.localScale = new Vector3(0.4f, 0.25f, 1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            renderer.sprite = sprite; renderer.color = Color.yellow; renderer.sortingOrder = 2;
            visual.SetActive(false);
        }
        private void Update() { if (visual != null) visual.SetActive(drain != null && drain.isActiveAndEnabled && drain.IsTelegraphing); }
        private void OnDisable() { if (visual != null) visual.SetActive(false); }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); }
    }
}
