using UnityEngine;
namespace HuntrX.Pooling
{
    [DisallowMultipleComponent]
    public sealed class PooledInstance : MonoBehaviour
    {
        private BoundedPrefabPool owner;
        internal void Bind(BoundedPrefabPool value)=>owner=value;
        public bool IsPooled=>owner!=null&&owner.IsManaged(gameObject);
        public bool TryReturn()=>owner!=null&&owner.Return(gameObject);
        private void OnDestroy(){owner?.Forget(gameObject);owner=null;}
    }
}
