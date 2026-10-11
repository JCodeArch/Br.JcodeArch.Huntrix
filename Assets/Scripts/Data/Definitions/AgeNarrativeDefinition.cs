using System;
using UnityEngine;
namespace HuntrX.Data
{
    [Serializable] public sealed class AgeNarrativeVariant
    {
        [SerializeField] private AgeBand band;
        [SerializeField,TextArea] private string text;
        [SerializeField,Range(0f,1f)] private float intensity;
        public AgeBand Band=>band;
        public string Text=>text;
        public float Intensity=>intensity;
    }
    [CreateAssetMenu(fileName="AgeNarrative",menuName="HUNTR/X/Data/Age Narrative")]
    public sealed class AgeNarrativeDefinition : GameDataDefinition
    {
        [SerializeField] private AgeNarrativeVariant[] variants=new AgeNarrativeVariant[0];
        [SerializeField] private float duration=3f;
        public float Duration=>duration;
        public bool TryChoose(AgeProfileData profile,out string text)
        {
            text=null;
            if(profile==null || !profile.IsValid(out _) || variants==null || variants.Length>16 || float.IsNaN(duration) || float.IsInfinity(duration) || duration<=0f || duration>30f) return false;
            int selected=-1;
            foreach(var variant in variants)
            {
                if(variant==null || string.IsNullOrWhiteSpace(variant.Text) || !Enum.IsDefined(typeof(AgeBand),variant.Band) || float.IsNaN(variant.Intensity) || float.IsInfinity(variant.Intensity) || variant.Intensity<0f || variant.Intensity>profile.NarrativeIntensity || (int)variant.Band>(int)profile.Band || (int)variant.Band<=selected) continue;
                selected=(int)variant.Band; text=variant.Text;
            }
            return text!=null;
        }
    }
}
