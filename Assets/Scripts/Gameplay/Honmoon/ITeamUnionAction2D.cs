namespace HuntrX.Gameplay.Honmoon
{
    /// <summary>An explicit assigned action; true means a real team action was accepted.</summary>
    public interface ITeamUnionAction2D
    {
        bool TryExecuteTeamUnion();
    }
}
