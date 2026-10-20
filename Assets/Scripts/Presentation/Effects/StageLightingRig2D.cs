using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HuntrX.Presentation.Effects
{
    /// <summary>Owns only the post-processing settings and volume it adds to one gameplay camera.</summary>
    [DisallowMultipleComponent]
    public sealed class StageLightingRig2D : MonoBehaviour
    {
        private const int VolumeLayer = 30;
        private Camera target;
        private UniversalAdditionalCameraData data;
        private GameObject volumeObject;
        private VolumeProfile profile;
        private bool priorHdr, priorPost;
        private LayerMask priorMask;
        private int appliedMask;

        public bool TryBind(Camera camera, VolumeProfile value)
        {
            if(!isActiveAndEnabled || camera == null || value == null ||
                !value.TryGet<Bloom>(out var bloom) || !bloom.active ||
                !bloom.intensity.overrideState || !bloom.IsActive()) return false;
            if(target == camera && profile == value && volumeObject != null) return true;
            Clear();
            target = camera;
            profile = value;
            data = camera.GetUniversalAdditionalCameraData();
            priorHdr = camera.allowHDR;
            priorPost = data.renderPostProcessing;
            priorMask = data.volumeLayerMask;
            appliedMask = priorMask.value | (1 << VolumeLayer);
            camera.allowHDR = true;
            data.renderPostProcessing = true;
            data.volumeLayerMask = appliedMask;
            volumeObject = new GameObject("HUNTRX dedicated bloom volume");
            volumeObject.transform.SetParent(transform, false);
            volumeObject.layer = VolumeLayer;
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.weight = 1f;
            volume.sharedProfile = profile;
            return true;
        }

        public void Clear()
        {
            if(target != null && target.allowHDR) target.allowHDR = priorHdr;
            if(data != null)
            {
                if(data.renderPostProcessing) data.renderPostProcessing = priorPost;
                if(data.volumeLayerMask.value == appliedMask) data.volumeLayerMask = priorMask;
            }
            if(volumeObject != null)
            {
                volumeObject.SetActive(false);
                Destroy(volumeObject);
            }
            volumeObject = null;
            target = null;
            data = null;
            profile = null;
        }
        private void OnDisable() => Clear();
        private void OnDestroy() => Clear();
    }
}
