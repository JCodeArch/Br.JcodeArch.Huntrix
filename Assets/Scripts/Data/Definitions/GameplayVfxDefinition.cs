using UnityEngine;
namespace HuntrX.Data
{
    public enum GameplayVfxKind { Impact, Energy, Honmoon, Special, SoulDrain, Telegraph }
    [CreateAssetMenu(menuName="HUNTR/X/Art/Gameplay VFX")]
    public sealed class GameplayVfxDefinition : GameDataDefinition
    {
        [SerializeField] private int capacity=32;
        [SerializeField] private float lifetime=.55f;
        [SerializeField] private float radius=.45f;
        [SerializeField] private float lineWidth=.055f;
        [SerializeField] private Color impact=new Color(1,.75f,.2f);
        [SerializeField] private Color energy=new Color(.3f,.8f,1);
        [SerializeField] private Color honmoon=new Color(.55f,.4f,1);
        [SerializeField] private Color special=new Color(1,.6f,.8f);
        [SerializeField] private Color soulDrain=new Color(.55f,.9f,.7f);
        [SerializeField] private Color telegraph=new Color(1,.35f,.3f);
        public int Capacity=>capacity;
        public float Lifetime=>lifetime;
        public float Radius=>radius;
        public float LineWidth=>lineWidth;
        public Color ColorFor(GameplayVfxKind kind)=>kind==GameplayVfxKind.Impact?impact:kind==GameplayVfxKind.Energy?energy:kind==GameplayVfxKind.Honmoon?honmoon:kind==GameplayVfxKind.Special?special:kind==GameplayVfxKind.SoulDrain?soulDrain:telegraph;
        public bool IsValid=>capacity>=1&&capacity<=64&&Bound(lifetime,.1f,3)&&Bound(radius,.1f,3)&&Bound(lineWidth,.01f,.2f)&&Valid(impact)&&Valid(energy)&&Valid(honmoon)&&Valid(special)&&Valid(soulDrain)&&Valid(telegraph);
        private static bool Bound(float v,float a,float b)=>!float.IsNaN(v)&&!float.IsInfinity(v)&&v>=a&&v<=b;
        private static bool Valid(Color c)=>Bound(c.r,0,1)&&Bound(c.g,0,1)&&Bound(c.b,0,1)&&Bound(c.a,.1f,1);
    }
}
