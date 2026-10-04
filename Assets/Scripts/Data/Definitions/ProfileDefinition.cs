using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>
    /// Static authored profile template identity only. User save state remains separate and mutable.
    /// </summary>
    [CreateAssetMenu(fileName = "ProfileDefinition", menuName = "HUNTR/X/Data/Profile")]
    public sealed class ProfileDefinition : GameDataDefinition
    {
    }
}