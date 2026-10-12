using System;
using System.Collections.Generic;
using UnityEngine;
namespace HuntrX.Pooling
{
    public interface IPooledLifecycle
    {
        void PrepareForRent();
        void PrepareForReturn();
    }
    /// <summary>Explicit owner-local pool; scene-root instances and hard capacity, no hidden singleton.</summary>
    public sealed class BoundedPrefabPool : IDisposable
    {
        private readonly GameObject prefab;
        private readonly int capacity;
        private readonly Stack<GameObject> available;
        private readonly HashSet<GameObject> rented=new HashSet<GameObject>();
        private readonly HashSet<GameObject> all=new HashSet<GameObject>();
        private readonly Dictionary<GameObject,IPooledLifecycle[]> lifecycles=new Dictionary<GameObject,IPooledLifecycle[]>();
        private bool disposed;
        private readonly GameObject staging;
        public BoundedPrefabPool(GameObject template,int maximum,int prewarm=0)
        {
            if(template==null||template.scene.IsValid()||maximum<1||maximum>128||prewarm<0||prewarm>maximum)
                throw new ArgumentException("Pool requires a prefab and bounded capacity/prewarm.");
            prefab=template;capacity=maximum;available=new Stack<GameObject>(maximum);
            staging=new GameObject("HuntrX Pool Inactive Staging");staging.SetActive(false);
            try{for(int i=0;i<prewarm;i++)available.Push(Create());}catch{Dispose();throw;}
        }
        public bool TryRent(Vector3 position,Quaternion rotation,out GameObject instance)
        {
            instance=null;if(disposed||!Finite(position.x)||!Finite(position.y)||!Finite(position.z)||
                !Finite(rotation.x)||!Finite(rotation.y)||!Finite(rotation.z)||!Finite(rotation.w)||
                (rotation.x==0f&&rotation.y==0f&&rotation.z==0f&&rotation.w==0f))return false;
            while(available.Count>0&&instance==null)instance=available.Pop();
            if(instance==null){if(all.Count>=capacity)return false;instance=Create();}
            rented.Add(instance);instance.transform.SetParent(null,false);instance.transform.SetPositionAndRotation(position,rotation);
            try
            {
                foreach(IPooledLifecycle component in lifecycles[instance])component.PrepareForRent();
                if(disposed||instance==null||!rented.Contains(instance)){instance=null;return false;}
                instance.SetActive(true);
                if(disposed||instance==null||!rented.Contains(instance)||!instance.activeInHierarchy){if(instance!=null)Return(instance);instance=null;return false;}
                return true;
            }
            catch(Exception){Return(instance);instance=null;return false;}
        }
        private GameObject Create()
        {
            GameObject instance=UnityEngine.Object.Instantiate(prefab,staging.transform,false);instance.SetActive(false);instance.transform.SetParent(null,false);
            PooledInstance lease=instance.GetComponent<PooledInstance>();if(lease==null)lease=instance.AddComponent<PooledInstance>();
            lease.Bind(this);all.Add(instance);
            List<IPooledLifecycle> hooks=new List<IPooledLifecycle>();
            foreach(MonoBehaviour component in instance.GetComponents<MonoBehaviour>())if(component is IPooledLifecycle hook)hooks.Add(hook);
            lifecycles.Add(instance,hooks.ToArray());return instance;
        }
        internal bool Return(GameObject instance)
        {
            if(disposed||instance==null||!rented.Remove(instance))return false;
            // Relinquish rented ownership before lifecycle callbacks or OnDisable can request return again.
            try
            {
                foreach(IPooledLifecycle component in lifecycles[instance])component.PrepareForReturn();
                instance.SetActive(false);
                if(disposed||instance==null)return false;
                if(instance.activeSelf)
                {all.Remove(instance);lifecycles.Remove(instance);UnityEngine.Object.Destroy(instance);return false;}
                available.Push(instance);return true;
            }
            catch(Exception)
            {
                all.Remove(instance);lifecycles.Remove(instance);
                if(instance!=null){instance.SetActive(false);UnityEngine.Object.Destroy(instance);}return false;
            }
        }
        private static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        internal bool IsManaged(GameObject instance)=>!disposed&&instance!=null&&all.Contains(instance);
        internal void Forget(GameObject instance){all.Remove(instance);rented.Remove(instance);lifecycles.Remove(instance);}
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            GameObject[] snapshot=new GameObject[all.Count];all.CopyTo(snapshot);
            foreach(GameObject instance in snapshot)if(instance!=null){instance.SetActive(false);UnityEngine.Object.Destroy(instance);}
            all.Clear();rented.Clear();available.Clear();lifecycles.Clear();
            if(staging!=null)UnityEngine.Object.Destroy(staging);
        }
    }
}
