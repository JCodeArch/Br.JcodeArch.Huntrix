using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
namespace HuntrX.Tests.PlayMode
{
    public sealed class PresentationPipelineTests
    {
        private GameObject root;
        private VolumeProfile profile;
        private Component rig;
        private Camera camera;
        [SetUp] public void SetUp()
        {
            root = new GameObject("Lighting ownership test");
            camera = root.AddComponent<Camera>();
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var type = typeof(HuntrX.Gameplay.VerticalSlice.VerticalSliceSession2D).Assembly.GetType("HuntrX.Presentation.Effects.StageLightingRig2D");
            Assert.That(type, Is.Not.Null, "Stage lighting rig is required.");
            rig = root.AddComponent(type);
        }
        [TearDown] public void Cleanup()
        {
            if(root != null) UnityEngine.Object.DestroyImmediate(root);
            if(profile != null) UnityEngine.Object.DestroyImmediate(profile);
        }
        private bool Bind() => (bool)rig.GetType().GetMethod("TryBind").Invoke(rig,new object[]{camera,profile});
        [Test] public void InvalidProfileDoesNotChangeCamera()
        {
            Assert.That(Bind(), Is.False);
            Assert.That(camera.GetComponent<UniversalAdditionalCameraData>(), Is.Null);
            Assert.That(root.GetComponentsInChildren<Volume>(true), Is.Empty);
        }
        [UnityTest] public IEnumerator BloomEnablesCameraAndOwnsOneVolumeThenRestoresOnDisable()
        {
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            data.volumeLayerMask = 1;
            camera.allowHDR = false;
            profile.Add<Bloom>(true).intensity.Override(.35f);
            Assert.That(Bind(), Is.True);
            Assert.That(Bind(), Is.True);
            Assert.That(data.renderPostProcessing, Is.True);
            Assert.That(camera.allowHDR, Is.True);
            var volumes = root.GetComponentsInChildren<Volume>(true);
            Assert.That(volumes.Length, Is.EqualTo(1));
            Assert.That(volumes[0].sharedProfile, Is.SameAs(profile));
            Assert.That(volumes[0].isGlobal, Is.True);
            Assert.That((data.volumeLayerMask.value & (1 << volumes[0].gameObject.layer)), Is.Not.Zero);
            ((Behaviour)rig).enabled = false;
            Assert.That(data.renderPostProcessing, Is.False);
            Assert.That(data.volumeLayerMask.value, Is.EqualTo(1));
            Assert.That(camera.allowHDR, Is.False);
            yield return null;
            Assert.That(root.GetComponentsInChildren<Volume>(true), Is.Empty);
        }
        [Test] public void AuthoredRuntimeProfileContainsActiveBloom()
        {
            var authored = Resources.Load<VolumeProfile>("Presentation/HuntrxBloom");
            Assert.That(authored, Is.Not.Null);
            Assert.That(authored.TryGet<Bloom>(out var bloom), Is.True);
            Assert.That(bloom.IsActive(), Is.True);
            Assert.That(bloom.intensity.value, Is.InRange(.1f,.6f));
        }
    }
}
