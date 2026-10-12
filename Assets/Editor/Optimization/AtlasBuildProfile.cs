using UnityEngine;
namespace HuntrX.Editor.Optimization
{
    [CreateAssetMenu(fileName="AtlasBuildProfile",menuName="HUNTR/X/Editor/Atlas Build Profile")]
    public sealed class AtlasBuildProfile : ScriptableObject
    {
        public string sourceFolder="Assets/Resources/Art";
        public string outputAtlas="Assets/Resources/Art/HuntrX.spriteatlas";
        public int maximumTextureSize=2048;
        public int padding=4;
        public FilterMode filterMode=FilterMode.Bilinear;
    }
}
