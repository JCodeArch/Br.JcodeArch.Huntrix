using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "JinuEncounter", menuName = "HUNTR/X/Data/Jinu Encounter")]
    public sealed class JinuEncounterDefinition : GameDataDefinition
    {
        [SerializeField] private SajaRivalDefinition opening;
        [SerializeField] private SajaRivalDefinition pressure;
        [SerializeField, Range(.01f,.99f)] private float pressureHealthFraction = .5f;
        [SerializeField] private string encounterCue = "jinu.encounter";
        [SerializeField] private string pressureCue = "jinu.pressure";
        [SerializeField] private string defeatedCue = "jinu.defeated";
        public SajaRivalDefinition Opening => opening;
        public SajaRivalDefinition Pressure => pressure;
        public float PressureHealthFraction => pressureHealthFraction;
        public string EncounterCue => encounterCue;
        public string PressureCue => pressureCue;
        public string DefeatedCue => defeatedCue;
        public bool IsValid(out string error)
        {
            error = "Jinu encounter requires two distinct valid Jinu profiles, a finite health threshold and cue identifiers.";
            if (opening == null || pressure == null || opening == pressure || !opening.IsValid(out _) ||
                !pressure.IsValid(out _) || opening.Identity != SajaIdentity.Jinu || pressure.Identity != SajaIdentity.Jinu ||
                float.IsNaN(pressureHealthFraction) || float.IsInfinity(pressureHealthFraction) ||
                pressureHealthFraction <= 0f || pressureHealthFraction >= 1f ||
                string.IsNullOrWhiteSpace(encounterCue) || string.IsNullOrWhiteSpace(pressureCue) ||
                string.IsNullOrWhiteSpace(defeatedCue)) return false;
            error = string.Empty;
            return true;
        }
    }
}
