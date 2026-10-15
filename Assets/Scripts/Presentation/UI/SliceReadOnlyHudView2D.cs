using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI
{
    /// <summary>Telemetry only: no Selectables, raycaster, EventSystem, or command events.</summary>
    public sealed class SliceReadOnlyHudView2D : MonoBehaviour
    {
        private Text health, honmoon, special, food, fans, character, beat;
        private void Awake()
        {
            // Leave the top 48 reference units for the existing IMGUI options button.
            health = Label("Health", .02f, .86f, .32f, .93f);
            honmoon = Label("Honmoon", .02f, .79f, .32f, .86f);
            character = Label("Character", .34f, .86f, .66f, .93f);
            special = Label("Special", .34f, .79f, .66f, .86f);
            food = Label("Food", .68f, .86f, .98f, .93f);
            fans = Label("Fans", .68f, .79f, .98f, .86f);
            beat = Label("Beat", .34f, .72f, .98f, .79f);
        }
        public void RenderState(SliceUiState2D state)
        {
            health.text = state.HealthCurrent < 0 || state.HealthMaximum <= 0 ? "Vida: indisponível" : $"Vida: {state.HealthCurrent} / {state.HealthMaximum}";
            honmoon.text = state.HonmoonCurrent < 0 ? "Honmoon: indisponível" : $"Honmoon: {state.HonmoonCurrent}";
            special.text = state.SpecialCharge < 0 ? "Special: indisponível" : state.SpecialReady ? "Special: pronto" : $"Special: carregando {Mathf.RoundToInt(state.SpecialCharge)}";
            food.text = state.FoodCount < 0 ? "Comida: indisponível" : $"Comida: {state.FoodCount}";
            fans.text = state.FansRescued < 0 ? "Resgates: indisponível" : $"Resgates nesta execução: {state.FansRescued}";
            character.text = state.ActiveCharacter;
            beat.text = state.BeatTitle;
        }
        private Text Label(string name, float x, float y, float xx, float yy)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(transform, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(x, y); rect.anchorMax = new Vector2(xx, yy);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var background = root.GetComponent<Image>();
            background.color = SliceUiTokens.Background; background.raycastTarget = false;
            var label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(root.transform, false);
            var textRect = label.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 2); textRect.offsetMax = new Vector2(-8, -2);
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = SliceUiTokens.BodyFontSize; text.color = SliceUiTokens.TextPrimary;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            return text;
        }
    }
}
