using UnityEngine;
using UnityEngine.UI;

namespace HuntrX.Presentation.UI
{
    /// <summary>Capcom Mega Man X / Zero Arcade HUD View: vertical segmented Energy/Honmoon tanks + sleek top arcade bar. No Selectables/raycasters.</summary>
    public sealed class SliceReadOnlyHudView2D : MonoBehaviour
    {
        private Text health, honmoon, special, food, fans, character, beat, heroBadgeLabel;
        private RectTransform healthBarFill, honmoonBarFill, specialBarFill;
        private Image heroBadgeBorder;

        private void Awake()
        {
            // Sleek compact top arcade status strip (leaves 94% of the screen completely clear for gameplay!)
            beat = Label("Beat", .01f, .952f, .23f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(1f, 0.86f, 0.28f), new Color(0.22f, 0.85f, 1f, 0.55f));
            character = Label("Character", .234f, .952f, .38f, .992f, new Color(0.08f, 0.04f, 0.18f, 0.88f), new Color(0.35f, 0.96f, 1f), new Color(0.95f, 0.28f, 0.82f, 0.55f));
            health = Label("Health", .384f, .952f, .52f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(0.35f, 1f, 0.62f), new Color(0.25f, 0.95f, 0.55f, 0.55f));
            honmoon = Label("Honmoon", .524f, .952f, .65f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(1f, 0.88f, 0.35f), new Color(1f, 0.82f, 0.22f, 0.55f));
            special = Label("Special", .654f, .952f, .785f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(1f, 0.48f, 0.88f), new Color(0.95f, 0.28f, 0.82f, 0.55f));
            food = Label("Food", .789f, .952f, .875f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(0.45f, 1f, 0.65f), new Color(0.22f, 0.85f, 1f, 0.55f));
            fans = Label("Fans", .879f, .952f, .99f, .992f, new Color(0.05f, 0.04f, 0.14f, 0.88f), new Color(0.78f, 0.88f, 1f), new Color(0.22f, 0.85f, 1f, 0.55f));

            // Authentic Capcom Mega Man X / X4 Segmented Vertical Energy Bars + Hero Emblem Base on the left
            healthBarFill = CreateVerticalArcadeBar("MMX_EnergyBar", "HP", 0.012f, 0.36f, 0.029f, 0.88f, new Color(0.18f, 0.98f, 0.48f, 0.96f), 20);
            honmoonBarFill = CreateVerticalArcadeBar("MMX_HonmoonBar", "HM", 0.033f, 0.36f, 0.048f, 0.88f, new Color(1f, 0.84f, 0.18f, 0.96f), 20);
            specialBarFill = CreateVerticalArcadeBar("MMX_SpecialBar", "SP", 0.052f, 0.36f, 0.065f, 0.78f, new Color(0.98f, 0.26f, 0.86f, 0.96f), 16);
            CreateCapcomHeroEmblemBadge();
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

            if (heroBadgeLabel != null)
            {
                string raw = state.ActiveCharacter ?? string.Empty;
                string cleanHero = raw.IndexOf("Mira", System.StringComparison.OrdinalIgnoreCase) >= 0 ? "MIRA\n[3]"
                    : raw.IndexOf("Zoey", System.StringComparison.OrdinalIgnoreCase) >= 0 ? "ZOEY\n[2]"
                    : "RUMI\n[1]";
                heroBadgeLabel.text = cleanHero;
                if (heroBadgeBorder != null)
                {
                    heroBadgeBorder.color = raw.IndexOf("Mira", System.StringComparison.OrdinalIgnoreCase) >= 0
                        ? new Color(1f, 0.28f, 0.58f, 0.95f)
                        : raw.IndexOf("Zoey", System.StringComparison.OrdinalIgnoreCase) >= 0
                            ? new Color(0.18f, 0.98f, 0.85f, 0.95f)
                            : new Color(0.25f, 0.88f, 1f, 0.95f);
                }
            }

            if (healthBarFill != null)
            {
                float hpRatio = (state.HealthCurrent >= 0 && state.HealthMaximum > 0)
                    ? Mathf.Clamp01((float)state.HealthCurrent / state.HealthMaximum) : 0f;
                healthBarFill.anchorMax = new Vector2(1f, Mathf.Max(0.02f, hpRatio));
            }
            if (honmoonBarFill != null)
            {
                float hmRatio = state.HonmoonCurrent >= 0 ? Mathf.Clamp01(state.HonmoonCurrent / 100f) : 0f;
                honmoonBarFill.anchorMax = new Vector2(1f, Mathf.Max(0.02f, hmRatio));
            }
            if (specialBarFill != null)
            {
                float spRatio = state.SpecialReady ? 1f : (state.SpecialCharge >= 0 ? Mathf.Clamp01(state.SpecialCharge / 100f) : 0f);
                specialBarFill.anchorMax = new Vector2(1f, Mathf.Max(0.02f, spRatio));
            }
        }

        private void CreateCapcomHeroEmblemBadge()
        {
            var outer = new GameObject("MMX_HeroEmblemOuter", typeof(RectTransform), typeof(Image));
            outer.transform.SetParent(transform, false);
            var outerRect = outer.GetComponent<RectTransform>();
            outerRect.anchorMin = new Vector2(0.011f, 0.255f);
            outerRect.anchorMax = new Vector2(0.066f, 0.352f);
            outerRect.offsetMin = outerRect.offsetMax = Vector2.zero;
            heroBadgeBorder = outer.GetComponent<Image>();
            heroBadgeBorder.color = new Color(0.25f, 0.88f, 1f, 0.95f);
            heroBadgeBorder.raycastTarget = false;

            var inner = new GameObject("MMX_HeroEmblemInner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(outer.transform, false);
            var innerRect = inner.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.offsetMin = new Vector2(2f, 2f);
            innerRect.offsetMax = new Vector2(-2f, -2f);
            var innerImg = inner.GetComponent<Image>();
            innerImg.color = new Color(0.05f, 0.04f, 0.14f, 0.95f);
            innerImg.raycastTarget = false;

            var lblGo = new GameObject("MMX_HeroEmblemText", typeof(RectTransform), typeof(Text));
            lblGo.transform.SetParent(inner.transform, false);
            var lblRect = lblGo.GetComponent<RectTransform>();
            lblRect.anchorMin = Vector2.zero;
            lblRect.anchorMax = Vector2.one;
            lblRect.offsetMin = lblRect.offsetMax = Vector2.zero;
            heroBadgeLabel = lblGo.GetComponent<Text>();
            heroBadgeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            heroBadgeLabel.fontSize = 13;
            heroBadgeLabel.fontStyle = FontStyle.Bold;
            heroBadgeLabel.color = Color.white;
            heroBadgeLabel.alignment = TextAnchor.MiddleCenter;
            heroBadgeLabel.raycastTarget = false;
            heroBadgeLabel.text = "RUMI\n[1]";
        }

        private RectTransform CreateVerticalArcadeBar(string barName, string capTag, float xMin, float yMin, float xMax, float yMax, Color fillColor, int segmentCount)
        {
            var frame = new GameObject(barName + "_Frame", typeof(RectTransform), typeof(Image));
            frame.transform.SetParent(transform, false);
            var frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = new Vector2(xMin, yMin);
            frameRect.anchorMax = new Vector2(xMax, yMax);
            frameRect.offsetMin = frameRect.offsetMax = Vector2.zero;
            var frameImg = frame.GetComponent<Image>();
            frameImg.color = new Color(0.32f, 0.42f, 0.62f, 0.92f);
            frameImg.raycastTarget = false;

            var container = new GameObject(barName, typeof(RectTransform), typeof(Image));
            container.transform.SetParent(frame.transform, false);
            var rect = container.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            var bg = container.GetComponent<Image>();
            bg.color = new Color(0.03f, 0.02f, 0.08f, 0.94f);
            bg.raycastTarget = false;

            var fillObj = new GameObject(barName + "_Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(container.transform, false);
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            var fillImg = fillObj.GetComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            // Authentic Mega Man X horizontal segment divider ticks
            for (int s = 1; s < segmentCount; s++)
            {
                float normY = (float)s / segmentCount;
                var tick = new GameObject(barName + "_Tick_" + s, typeof(RectTransform), typeof(Image));
                tick.transform.SetParent(container.transform, false);
                var tickRect = tick.GetComponent<RectTransform>();
                tickRect.anchorMin = new Vector2(0f, normY);
                tickRect.anchorMax = new Vector2(1f, normY);
                tickRect.offsetMin = new Vector2(0f, -1f);
                tickRect.offsetMax = new Vector2(0f, 1f);
                var tickImg = tick.GetComponent<Image>();
                tickImg.color = new Color(0.03f, 0.02f, 0.08f, 0.88f);
                tickImg.raycastTarget = false;
            }

            var capGo = new GameObject(barName + "_CapLabel", typeof(RectTransform), typeof(Text));
            capGo.transform.SetParent(frame.transform, false);
            var capRect = capGo.GetComponent<RectTransform>();
            capRect.anchorMin = new Vector2(0f, 1f);
            capRect.anchorMax = new Vector2(1f, 1.055f);
            capRect.offsetMin = capRect.offsetMax = Vector2.zero;
            var capText = capGo.GetComponent<Text>();
            capText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            capText.fontSize = 11;
            capText.fontStyle = FontStyle.Bold;
            capText.color = fillColor;
            capText.alignment = TextAnchor.MiddleCenter;
            capText.raycastTarget = false;
            capText.text = capTag;

            return fillRect;
        }

        private Text Label(string name, float x, float y, float xx, float yy, Color bgColor, Color txtColor, Color borderColor)
        {
            var border = new GameObject(name + "_Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(transform, false);
            var borderRect = border.GetComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(x, y);
            borderRect.anchorMax = new Vector2(xx, yy);
            borderRect.offsetMin = borderRect.offsetMax = Vector2.zero;
            var borderImg = border.GetComponent<Image>();
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;

            var root = new GameObject(name + "_Container", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(border.transform, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(1f, 1f);
            rect.offsetMax = new Vector2(-1f, -1f);
            var background = root.GetComponent<Image>();
            background.color = bgColor;
            background.raycastTarget = false;

            var label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(root.transform, false);
            var textRect = label.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 1);
            textRect.offsetMax = new Vector2(-6, -1);
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 13;
            text.fontStyle = FontStyle.Bold;
            text.color = txtColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }
    }
}
