using UnityEngine;

namespace HuntrX.Presentation.UI
{
    /// <summary>
    /// Visual design tokens based on the Figma specifications (AoNgky0oNVLU31UvML5xns).
    /// </summary>
    public static class SliceUiTokens
    {
        // Reference Layout
        public static readonly Vector2 ReferenceResolution = new Vector2(1280f, 720f);
        public const float MinimumTouchTargetSize = 64f;

        // Typography Sizes (Reference Units)
        public const int TitleFontSize = 36;
        public const int ActionFontSize = 24;
        public const int BodyFontSize = 20;

        // Colors (Figma exact hex codes)
        public static readonly Color TextPrimary = HexToColor("203047");
        public static readonly Color TextSecondary = HexToColor("53657C");
        public static readonly Color Background = HexToColor("F5F8FC");
        public static readonly Color Surface = HexToColor("FFFFFF");
        public static readonly Color PrimaryAction = HexToColor("2463D4");
        public static readonly Color Protection = HexToColor("C5EEDF");
        public static readonly Color Special = HexToColor("7653B7");

        // Character Identity Accents
        public static readonly Color IdentityRumi = HexToColor("F6CADA");
        public static readonly Color IdentityMira = HexToColor("F8D987");
        public static readonly Color IdentityZoey = HexToColor("D5C6F2");

        public static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
