using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Original geometric placeholder; replace with reviewed character art in the art cards.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyAgent2D))]
    public sealed class EnemyPrototypeVisual2D : MonoBehaviour
    {
        private EnemyAgent2D agent;
        private SpriteRenderer renderer2D;
        private Sprite sprite;
        private void Awake()
        {
            agent = GetComponent<EnemyAgent2D>();
            GameObject visual = new GameObject("Prototype silhouette");
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(0.65f, 1.1f, 1f);
            renderer2D = visual.AddComponent<SpriteRenderer>();
            sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f, 1f);
            renderer2D.sprite = sprite;
        }
        private void OnEnable() { if (agent != null) { agent.StateChanged += UpdateColor; UpdateColor(agent.State); } }
        private void OnDisable() { if (agent != null) agent.StateChanged -= UpdateColor; }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); }
        private void UpdateColor(EnemyState2D state)
        {
            if (renderer2D == null) return;
            renderer2D.color = state == EnemyState2D.Telegraph ? Color.yellow :
                state == EnemyState2D.Attacking ? Color.white : state == EnemyState2D.Dead ? Color.gray :
                new Color(0.75f, 0.2f, 0.45f);
        }
    }
}
