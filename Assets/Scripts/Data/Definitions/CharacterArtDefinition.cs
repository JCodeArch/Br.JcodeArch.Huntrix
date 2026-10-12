using UnityEngine;
namespace HuntrX.Data
{
    public enum CharacterWeaponShape { Sword, Polearm, TwinStars, Microphone, Staff, Fans, Horns }
    [CreateAssetMenu(menuName="HUNTR/X/Art/Character profile")]
    public sealed class CharacterArtDefinition : GameDataDefinition
    {
        [SerializeField] private string characterName;
        [SerializeField] private Color primary = Color.magenta;
        [SerializeField] private Color accent = Color.cyan;
        [SerializeField] private Color skin = new Color(.85f,.65f,.5f);
        [SerializeField] private CharacterWeaponShape weapon;
        [SerializeField] private float height = 1.1f;
        [SerializeField] private float width = .45f;
        [SerializeField] private float hairLength = .25f;
        public string CharacterName => characterName;
        public Color Primary => primary;
        public Color Accent => accent;
        public Color Skin => skin;
        public CharacterWeaponShape Weapon => weapon;
        public float Height => height;
        public float Width => width;
        public float HairLength => hairLength;
        public bool IsValid => !string.IsNullOrWhiteSpace(characterName) && Finite(height) && height>=.5f && height<=3f && Finite(width) && width>=.2f && width<=1.5f && Finite(hairLength) && hairLength>=0 && hairLength<=1 && ColorValid(primary) && ColorValid(accent) && ColorValid(skin) && (int)weapon>=0 && (int)weapon<=6;
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool ColorValid(Color c) => Finite(c.r)&&Finite(c.g)&&Finite(c.b)&&Finite(c.a)&&c.r>=0&&c.r<=1&&c.g>=0&&c.g<=1&&c.b>=0&&c.b<=1&&c.a>0&&c.a<=1;
    }
}
