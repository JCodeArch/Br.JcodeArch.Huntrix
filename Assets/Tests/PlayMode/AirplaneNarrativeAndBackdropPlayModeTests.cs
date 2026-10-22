using System.Collections;
using System.Collections.Generic;
using HuntrX.Gameplay.Levels;
using HuntrX.Gameplay.Narrative;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AirplaneNarrativeAndBackdropPlayModeTests
    {
        private Scene slice;

        [UnityTearDown]
        public IEnumerator UnloadSlice()
        {
            if (!slice.IsValid() || !slice.isLoaded) yield break;
            SceneManager.SetActiveScene(SceneManager.CreateScene("AirplaneNarrativeCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }

        [UnityTest]
        public IEnumerator VerticalSliceSingleCharacterPresenterAndVipIntroFourFrameProgression()
        {
            SceneManager.LoadScene("VerticalSlice");
            yield return null;
            yield return null;
            slice = SceneManager.GetActiveScene();

            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            Assert.That(session, Is.Not.Null, "Expected VerticalSliceSession2D in VerticalSlice scene.");
            Assert.That(session.Stage, Is.Not.Null);
            Assert.That(session.Stage.Definition, Is.Not.Null);
            Assert.That(session.Stage.Definition.StageId, Is.EqualTo("airplane"));

            var narrative = session.GetComponent<SliceNarrativeComposition2D>();
            if (narrative != null && narrative.Presenter != null)
            {
                int safety = 0;
                while (narrative.Presenter.IsPresenting && safety++ < 16)
                    narrative.Presenter.Advance();
            }

            yield return null;

            var activeHero = session.Manager != null ? session.Manager.ActiveCharacter : null;
            Assert.That(activeHero, Is.Not.Null, "Expected active heroine on VerticalSliceSession2D.");

            // Contract 1: While VIP_RearLounge_EatingScene is active at the start of the stage,
            // no standing character SpriteRenderer on the active heroine may be visible over the seated VIP intro!
            var vipRoot = session.Stage.RuntimeRoot.transform.Find("VIP_RearLounge_EatingScene");
            Assert.That(vipRoot, Is.Not.Null, "Expected VIP_RearLounge_EatingScene in airplane stage.");

            var heroRenderersWhileSeated = GetEnabledHeroCharacterRenderers(activeHero.transform);
            Assert.That(heroRenderersWhileSeated.Count, Is.EqualTo(0),
                "Expected 0 enabled standing hero SpriteRenderers while seated in VIP_RearLounge_EatingScene, but found: " +
                string.Join(", ", heroRenderersWhileSeated));

            // Contract 2: VIP intro progresses through all 4 authored narrative frames (0+1 seated, 2 alert stand-up, 3 sword-slash leap)
            // before releasing control to the single standing gameplay presenter.
            var rumiSeat = vipRoot.Find("VIP_Seat_Rumi_EatingRamyeon")?.GetComponent<SpriteRenderer>();
            var zoeyMiraSeat = vipRoot.Find("VIP_Table_ZoeyAndMira_Feasting")?.GetComponent<SpriteRenderer>();
            Assert.That(rumiSeat, Is.Not.Null);
            Assert.That(zoeyMiraSeat, Is.Not.Null);
            Assert.That(rumiSeat.enabled, Is.True);
            Assert.That(zoeyMiraSeat.enabled, Is.True);

            var frame0 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/0");
            var frame2 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/2");
            var frame3 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/3");
            Assert.That(frame0, Is.Not.Null);
            Assert.That(frame2, Is.Not.Null);
            Assert.That(frame3, Is.Not.Null);
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame0), "Expected frame 0 (slurping ramyeon) at initial beat clock.");

            var beat = session.GetComponent<SliceAirplaneBeat2D>();
            Assert.That(beat, Is.Not.Null);

            beat.Tick(1.6f);
            Assert.That(rumiSeat.enabled, Is.True);
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame2), "Expected frame 2 (alert stand-up) around 1.6s.");

            beat.Tick(0.85f);
            Assert.That(rumiSeat.enabled, Is.True, "Expected VIP intro to remain visible during frame 3 sword-slash leap (~2.45s).");
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame3), "Expected frame 3 (drawing Saingeom sword leap) around 2.45s.");

            beat.Tick(0.85f);
            yield return null;

            Assert.That(rumiSeat.enabled, Is.False, "Expected Rumi VIP intro renderer disabled after stand-up completes (~3.3s).");
            Assert.That(zoeyMiraSeat.enabled, Is.False, "Expected Zoey/Mira VIP booth renderer disabled after stand-up completes (~3.3s).");

            var heroRenderersAfterStandUp = GetEnabledHeroCharacterRenderers(activeHero.transform);
            Assert.That(heroRenderersAfterStandUp.Count, Is.EqualTo(1),
                "Expected exactly 1 enabled hero character SpriteRenderer after VIP intro stand-up, but found: " +
                string.Join(", ", heroRenderersAfterStandUp));
            Assert.That(heroRenderersAfterStandUp[0], Does.Contain("Player prototype silhouette"));
        }

        [UnityTest]
        public IEnumerator AirplaneStageBuildsSingleUndistortedTiledBackdropAndAnchoredCockpitDoor()
        {
            SceneManager.LoadScene("VerticalSlice");
            yield return null;
            yield return null;
            slice = SceneManager.GetActiveScene();

            var allTransforms = Object.FindObjectsByType<Transform>();
            var backdropRoots = new List<Transform>();
            Transform cockpitDoor = null;

            foreach (var tr in allTransforms)
            {
                if (tr == null) continue;
                if (tr.name == "HD_StageBackdrop_airplane")
                    backdropRoots.Add(tr);
                else if (tr.name == "Airplane_CockpitBossBulkhead")
                    cockpitDoor = tr;
            }

            Assert.That(backdropRoots.Count, Is.EqualTo(1),
                "Expected exactly 1 HD_StageBackdrop_airplane root (no duplicate 64x18 stretched quad from StageEnvironmentRenderer2D).");
            Assert.That(backdropRoots[0].lossyScale.x, Is.EqualTo(1f).Within(0.01f),
                "Expected HD_StageBackdrop_airplane root to have unstretched unit lossyScale.");

            var animators = Object.FindObjectsByType<LivingStageBackdropAnimator2D>();
            Assert.That(animators.Length, Is.EqualTo(1), "Expected exactly 1 LivingStageBackdropAnimator2D in the airplane stage.");

            Assert.That(cockpitDoor, Is.Not.Null, "Expected Airplane_CockpitBossBulkhead at the right end of the stage.");

            var cam = Camera.main;
            Assert.That(cam, Is.Not.Null);

            cam.transform.position = new Vector3(8f, 1.8f, -10f);
            yield return null;
            float doorXNearStart = cockpitDoor.position.x;

            cam.transform.position = new Vector3(42f, 1.8f, -10f);
            yield return null;
            float doorXNearCockpit = cockpitDoor.position.x;

            Assert.That(doorXNearStart, Is.InRange(44.5f, 48.5f),
                "Expected Airplane_CockpitBossBulkhead to be anchored near Stage_End (x ~ 46.2), not multiplied by parent scale.");
            Assert.That(Mathf.Abs(doorXNearCockpit - doorXNearStart), Is.LessThanOrEqualTo(0.25f),
                "Expected Airplane_CockpitBossBulkhead world X to stay anchored at the cockpit end instead of drifting with backdrop parallax.");
        }

        [Test]
        public void DialoguePresenterResolvesDedicatedPortraitTexturesBeforeFullBodySheets()
        {
            var go = new GameObject("DialoguePresenterPortraitTest");
            try
            {
                var presenter = go.AddComponent<DialoguePresenter2D>();
                var method = typeof(DialoguePresenter2D).GetMethod(
                    "ResolveSpeakerPortrait",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null, "Expected ResolveSpeakerPortrait on DialoguePresenter2D.");

                var expectedRumiPortrait = Resources.Load<Texture2D>("Art/Portraits/Rumi_Portrait");
                Assert.That(expectedRumiPortrait, Is.Not.Null, "Expected Art/Portraits/Rumi_Portrait texture in Resources.");

                var resolved = method.Invoke(null, new object[] { "RUMI" }) as Texture2D;
                Assert.That(resolved, Is.EqualTo(expectedRumiPortrait),
                    "Expected DialoguePresenter2D to resolve Art/Portraits/Rumi_Portrait instead of full-body character sheet.");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static List<string> GetEnabledHeroCharacterRenderers(Transform heroRoot)
        {
            var result = new List<string>();
            if (heroRoot == null) return result;
            foreach (var sr in heroRoot.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr == null || !sr.enabled || sr.sprite == null) continue;
                if (sr.name == "Player prototype silhouette" || sr.name == "HD_CharacterSprite")
                    result.Add(sr.name);
            }
            return result;
        }
    }
}
