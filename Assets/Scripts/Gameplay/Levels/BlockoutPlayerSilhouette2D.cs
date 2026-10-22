using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    /// <summary>Original geometric blockout marker, hidden when HD character presentation is active.</summary>
    public sealed class BlockoutPlayerSilhouette2D : MonoBehaviour
    {
        private Texture2D texture;
        private Sprite sprite;
        private GameObject visual;
        private void Awake()
        {
            texture = new Texture2D(1,1); texture.SetPixel(0,0,Color.white); texture.Apply();
            sprite = Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1f);
            visual = new GameObject("Original_Blockout_Silhouette"); visual.transform.SetParent(transform,false);
            visual.transform.localScale = new Vector3(.5f,1.1f,1f);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>(); renderer.sprite = sprite;
            renderer.color = new Color(.65f,.35f,.95f,0f); renderer.sortingOrder = 5;
            renderer.enabled = false;
        }
        private void OnDestroy() { if (sprite != null) Destroy(sprite); if (texture != null) Destroy(texture); }
        private void OnDisable() { if (visual != null) visual.SetActive(false); }
        private void OnEnable() { if (visual != null) visual.SetActive(true); }
    }
}
