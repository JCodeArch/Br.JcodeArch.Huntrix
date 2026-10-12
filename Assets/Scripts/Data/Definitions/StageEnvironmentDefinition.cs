using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(menuName="HUNTR/X/Art/Stage environment")]
    public sealed class StageEnvironmentDefinition : GameDataDefinition
    {
        [SerializeField] private string stageId;
        [SerializeField] private StageBlockoutTheme theme;
        [SerializeField] private Color sky=new Color(.2f,.3f,.5f);
        [SerializeField] private Color distant=new Color(.3f,.4f,.5f);
        [SerializeField] private Color near=new Color(.15f,.25f,.35f);
        [SerializeField] private Color accent=new Color(.7f,.8f,.9f);
        public string StageId=>stageId;
        public StageBlockoutTheme Theme=>theme;
        public Color Sky=>sky;
        public Color Distant=>distant;
        public Color Near=>near;
        public Color Accent=>accent;
        public bool IsValid=>!string.IsNullOrWhiteSpace(stageId)&&(int)theme>=0&&(int)theme<=3&&Valid(sky)&&Valid(distant)&&Valid(near)&&Valid(accent);
        private static bool Valid(Color c)=>Finite(c.r)&&Finite(c.g)&&Finite(c.b)&&Finite(c.a)&&c.r>=0&&c.r<=1&&c.g>=0&&c.g<=1&&c.b>=0&&c.b<=1&&c.a>0&&c.a<=1;
        private static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
    }
}
