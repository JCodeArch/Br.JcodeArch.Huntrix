using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    public abstract class SliceBeat2D : MonoBehaviour
    {
        public abstract string Title { get; }
        public bool IsComplete { get; protected set; }
        public abstract bool TryBegin(VerticalSliceSession2D session);
        public virtual void Tick(float deltaTime) { }
        public virtual void OnInput(SliceInputFrame frame) { }
        public virtual void Exit() { IsComplete = false; }
    }
}
