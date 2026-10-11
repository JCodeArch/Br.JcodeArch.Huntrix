using UnityEngine;
namespace HuntrX.Data
{
    [CreateAssetMenu(fileName = "StageRouteDefinition", menuName = "HUNTR/X/Data/Stage Route")]
    public sealed class StageRouteDefinition : GameDataDefinition
    {
        [SerializeField] private StageBlockoutDefinition[] stages = new StageBlockoutDefinition[0];
        public int StageCount => stages?.Length ?? 0;
        public StageBlockoutDefinition GetStage(int index) => index >= 0 && index < StageCount ? stages[index] : null;
        public bool IsValid(out string error)
        {
            if (stages == null || stages.Length == 0 || stages.Length > 32)
            { error = "Stage route requires 1..32 authored stages."; return false; }
            for (int i = 0; i < stages.Length; i++)
            {
                if (stages[i] == null || !stages[i].IsValid(out error)) { error = "Stage route contains an invalid stage."; return false; }
                for (int j = 0; j < i; j++) if (stages[j] == stages[i] || stages[j].StageId == stages[i].StageId)
                { error = "Stage route identities must be unique."; return false; }
            }
            error = string.Empty; return true;
        }
    }
}
