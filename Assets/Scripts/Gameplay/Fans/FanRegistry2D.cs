using System.Collections.Generic;
using UnityEngine;
namespace HuntrX.Gameplay.Fans
{
    [DisallowMultipleComponent]
    public sealed class FanRegistry2D : MonoBehaviour
    {
        private readonly List<FanActor2D> fans = new List<FanActor2D>(64);
        public int Count => fans.Count;
        public FanActor2D GetAt(int index) => index >= 0 && index < fans.Count ? fans[index] : null;
        public bool Register(FanActor2D fan)
        {
            if (!isActiveAndEnabled || fan == null || !fan.isActiveAndEnabled || !fan.IsConfigurationValid) return false;
            if (fans.Contains(fan)) return true;
            if (fans.Count >= 64) return false;
            fans.Add(fan); return true;
        }
        public void Unregister(FanActor2D fan) => fans.Remove(fan);
        private void OnDestroy() => fans.Clear();
    }
}
