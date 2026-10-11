using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Gameplay.Levels
{
    [DisallowMultipleComponent, RequireComponent(typeof(StageBlockoutBuilder2D))]
    public sealed class StageBlockoutHost2D : MonoBehaviour
    {
        [SerializeField] private StageBlockoutDefinition definition;
        private void Start()
        {
            if (!GetComponent<StageBlockoutBuilder2D>().TryBuild(definition)) Debug.LogError("Invalid stage grammar host.", this);
        }
    }
}
