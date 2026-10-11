using System;
using UnityEngine;
namespace HuntrX.Data
{
    [Serializable]
    public sealed class NarrativeLine
    {
        [SerializeField] private string speaker;
        [SerializeField,TextArea] private string portuguese;
        [SerializeField,TextArea] private string english;
        [SerializeField] private Color portraitColor=Color.white;
        [SerializeField] private AudioClip voice;
        public string Speaker=>speaker;
        public string Text(bool useEnglish)=>useEnglish?english:portuguese;
        public Color PortraitColor=>portraitColor;
        public AudioClip Voice=>voice;
        public bool IsValid=>!string.IsNullOrWhiteSpace(speaker)&&speaker.Length<=64&&!string.IsNullOrWhiteSpace(portuguese)&&portuguese.Length<=2000&&!string.IsNullOrWhiteSpace(english)&&english.Length<=2000;
    }
    [CreateAssetMenu(menuName="HuntrX/Narrative/Sequence")]
    public sealed class NarrativeSequenceDefinition : ScriptableObject
    {
        [SerializeField] private string cueId;
        [SerializeField,Range(5,120)] private float charactersPerSecond=35;
        [SerializeField] private NarrativeLine[] lines=Array.Empty<NarrativeLine>();
        public string CueId=>cueId;
        public float CharactersPerSecond=>charactersPerSecond;
        public int Count=>lines==null?0:lines.Length;
        public NarrativeLine GetLine(int index)=>index>=0&&index<Count?lines[index]:null;
        public bool IsValid(out string error)
        {
            error=null;if(string.IsNullOrWhiteSpace(cueId)||cueId.Length>128||Count<1||Count>32||float.IsNaN(charactersPerSecond)||float.IsInfinity(charactersPerSecond)||charactersPerSecond<5||charactersPerSecond>120){error="Invalid bounded narrative sequence.";return false;}
            foreach(var line in lines)if(line==null||!line.IsValid){error="Both localized texts and speaker are required.";return false;}return true;
        }
    }
}
