using UnityEngine;
namespace HuntrX.Data
{
    public enum CharacterAnimationTechnique { Rig, Hybrid }
    public enum CharacterVisualPose { Idle, Run, Jump, Dash, Attack, Aerial, Hurt, Parry, Death, Special }
    [CreateAssetMenu(menuName="HUNTR/X/Art/Character rig")]
    public sealed class CharacterRigDefinition : GameDataDefinition
    {
        [SerializeField] private CharacterArtDefinition character;
        [SerializeField] private CharacterAnimationTechnique technique;
        [SerializeField] private float runCycleSeconds=.45f;
        [SerializeField] private float idleCycleSeconds=2f;
        [SerializeField] private float hurtSeconds=.2f;
        [SerializeField] private float specialSeconds=1f;
        public CharacterArtDefinition Character=>character;
        public CharacterAnimationTechnique Technique=>technique;
        public float RunCycleSeconds=>runCycleSeconds;
        public float IdleCycleSeconds=>idleCycleSeconds;
        public float HurtSeconds=>hurtSeconds;
        public float SpecialSeconds=>specialSeconds;
        public bool IsValid=>character!=null&&character.IsValid&&(int)technique>=0&&(int)technique<=1&&Bound(runCycleSeconds,.2f,2)&&Bound(idleCycleSeconds,.5f,10)&&Bound(hurtSeconds,.05f,2)&&Bound(specialSeconds,.1f,10);
        private static bool Bound(float v,float lo,float hi)=>!float.IsNaN(v)&&!float.IsInfinity(v)&&v>=lo&&v<=hi;
    }
}
