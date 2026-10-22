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

        [UnityTest]
        public IEnumerator OpeningDialogueAndVipIntroAreSynchronizedAndFramedInsideCameraViewport()
        {
            SceneManager.LoadScene("VerticalSlice");
            yield return null;
            yield return null;
            slice = SceneManager.GetActiveScene();

            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            Assert.That(session, Is.Not.Null);
            var narrative = session.GetComponent<SliceNarrativeComposition2D>();
            Assert.That(narrative, Is.Not.Null);
            Assert.That(narrative.Presenter, Is.Not.Null, "Expected DialoguePresenter2D to be active on stage entry.");
            Assert.That(narrative.Presenter.IsPresenting, Is.True, "Expected opening airplane dialogue to be presenting.");

            var vipRoot = session.Stage.RuntimeRoot.transform.Find("VIP_RearLounge_EatingScene");
            Assert.That(vipRoot, Is.Not.Null);
            var rumiSeat = vipRoot.Find("VIP_Seat_Rumi_EatingRamyeon")?.GetComponent<SpriteRenderer>();
            var zoeyMiraSeat = vipRoot.Find("VIP_Table_ZoeyAndMira_Feasting")?.GetComponent<SpriteRenderer>();
            Assert.That(rumiSeat, Is.Not.Null);
            Assert.That(zoeyMiraSeat, Is.Not.Null);

            var cam = Camera.main;
            Assert.That(cam, Is.Not.Null);
            float halfH = cam.orthographicSize;
            float halfW = halfH * (16f / 9f);
            Rect stageBounds;
            Vector2 camCenter = session.Stage.Definition.TryGetWorldBounds(out stageBounds)
                ? SliceCameraSolver2D.Solve(stageBounds, new Vector2(2.55f, 1.75f), halfH, 16f / 9f)
                : (Vector2)cam.transform.position;
            float camLeft = camCenter.x - halfW;
            float camRight = camCenter.x + halfW;

            // Contract A: Both Rumi's VIP chair and Zoey/Mira's VIP booth table must be framed well inside the 16:9 opening camera viewport
            Assert.That(rumiSeat.bounds.min.x, Is.GreaterThanOrEqualTo(camLeft + 0.5f),
                "Rumi's VIP seat must not be clipped at the left camera edge.");
            Assert.That(zoeyMiraSeat.bounds.min.x, Is.GreaterThanOrEqualTo(camLeft + 0.5f),
                "Zoey & Mira's VIP table must be inside the opening camera viewport (not clipped off-screen to the left).");
            Assert.That(zoeyMiraSeat.bounds.max.x, Is.LessThanOrEqualTo(camRight - 1.5f),
                "Zoey & Mira's VIP table must stay well inside the opening camera viewport.");

            // Contract B: The Anime_01_AirplaneFight cutscene inset must not occlude the center playfield over the VIP tableau
            Rect cutsceneInset = DialoguePresenter2D.GetCutsceneInsetViewportRect(1280f, 720f);
            Rect dialogueBox = DialoguePresenter2D.GetDialoguePanelViewportRect(1280f, 720f);
            Assert.That(cutsceneInset.width, Is.LessThanOrEqualTo(1280f * 0.32f),
                "Cutscene inset must be a compact picture-in-picture card, not a full-screen 72% blocker over the VIP lounge.");
            Assert.That(cutsceneInset.xMin, Is.GreaterThanOrEqualTo(1280f * 0.62f),
                "Cutscene inset must stay in the top-right corner clear of the left/center VIP lounge tableau.");
            Assert.That(dialogueBox.yMin, Is.GreaterThanOrEqualTo(720f * 0.70f),
                "Bottom dialogue panel must leave the upper 70% of the 1280x720 viewport open for the VIP lounge scene.");

            // Contract C: Ticking the airplane beat while dialogue is open must NOT consume or finish the VIP intro clock
            var frame0 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/0");
            var frame2 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/2");
            var frame3 = Resources.Load<Sprite>("Art/Props/VipLoungeIntro/3");
            var beat = session.GetComponent<SliceAirplaneBeat2D>();
            Assert.That(beat, Is.Not.Null);

            beat.Tick(2.5f);
            Assert.That(rumiSeat.enabled, Is.True, "VIP intro must not finish behind the open dialogue box.");
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame0),
                "VIP intro must hold Frame 0 (slurping ramyeon) while opening dialogue is still being read.");

            // Advance through both opening dialogue lines (Rumi -> Zoey)
            int safety = 0;
            while (narrative.Presenter.IsPresenting && safety++ < 16)
                narrative.Presenter.Advance();
            Assert.That(narrative.Presenter.IsPresenting, Is.False);

            // Immediately after closing the second dialogue line, the VIP intro must still be at Frame 0 so the player sees the full sequence!
            Assert.That(rumiSeat.enabled, Is.True, "VIP intro must still be visible immediately after closing the dialogue.");
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame0), "VIP intro must begin from Frame 0 after dialogue closes.");

            beat.Tick(1.6f);
            Assert.That(rumiSeat.enabled, Is.True);
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame2), "Expected Frame 2 (alert stand-up) after dialogue completion.");

            beat.Tick(0.85f);
            Assert.That(rumiSeat.enabled, Is.True);
            Assert.That(rumiSeat.sprite, Is.EqualTo(frame3), "Expected Frame 3 (Saingeom sword-slash leap) after Frame 2.");

            beat.Tick(0.85f);
            Assert.That(rumiSeat.enabled, Is.False, "Expected VIP intro to hand off to combat after Frame 3 completes.");
            Assert.That(zoeyMiraSeat.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator AirplaneStageHasTrueAlphaCutoutWindowsAndMultiPlaneParallaxAtDistinctSpeeds()
        {
            SceneManager.LoadScene("VerticalSlice");
            yield return null;
            yield return null;
            slice = SceneManager.GetActiveScene();

            var bgRoot = GameObject.Find("HD_StageBackdrop_airplane");
            Assert.That(bgRoot, Is.Not.Null);
            Assert.That(bgRoot.transform.lossyScale.x, Is.EqualTo(1f).Within(0.01f));

            var farSky = bgRoot.transform.Find("Parallax_Far_NightSkyClouds");
            var midWing = bgRoot.transform.Find("Parallax_Mid_JetWing");
            var nearFuselageTile = bgRoot.transform.Find("Backdrop_Tile_0");
            var cockpitDoor = GameObject.Find("Airplane_CockpitBossBulkhead");

            Assert.That(farSky, Is.Not.Null, "Expected Parallax_Far_NightSkyClouds layer behind the airplane windows.");
            Assert.That(midWing, Is.Not.Null, "Expected Parallax_Mid_JetWing layer outside the airplane windows.");
            Assert.That(nearFuselageTile, Is.Not.Null, "Expected Backdrop_Tile_0 cabin fuselage layer.");
            Assert.That(cockpitDoor, Is.Not.Null);

            var farSr = farSky.GetComponentInChildren<SpriteRenderer>();
            var midSr = midWing.GetComponentInChildren<SpriteRenderer>();
            var nearSr = nearFuselageTile.GetComponent<SpriteRenderer>();
            Assert.That(farSr, Is.Not.Null);
            Assert.That(midSr, Is.Not.Null);
            Assert.That(nearSr, Is.Not.Null);

            // Depth sorting order: Far Sky (< Mid Wing) < Mid Wing (< Near Fuselage) < Gameplay
            Assert.That(farSr.sortingOrder, Is.LessThan(midSr.sortingOrder),
                "Far sky/clouds must sort behind the mid-plane jet wing.");
            Assert.That(midSr.sortingOrder, Is.LessThan(nearSr.sortingOrder),
                "Mid-plane jet wing must sort behind the cabin fuselage window cutouts.");

            // Verify the cabin fuselage sprite has true alpha-cutout window panes (alpha < 0.15 at window centers)
            // while keeping opaque metallic fuselage ribs between windows (alpha > 0.95).
            Texture2D fuselageTex = nearSr.sprite.texture;
            Assert.That(fuselageTex, Is.Not.Null);
            int winCenterY = fuselageTex.height - 375;
            Color windowGlassPixel1 = fuselageTex.GetPixel(346, winCenterY);
            Color windowGlassPixel2 = fuselageTex.GetPixel(697, winCenterY);
            Color windowGlassPixel3 = fuselageTex.GetPixel(1051, winCenterY);
            Color fuselageWallPixel = fuselageTex.GetPixel(520, winCenterY);

            Assert.That(windowGlassPixel1.a, Is.LessThan(0.15f), "Window 1 center must be alpha-cutout transparent.");
            Assert.That(windowGlassPixel2.a, Is.LessThan(0.15f), "Window 2 center must be alpha-cutout transparent.");
            Assert.That(windowGlassPixel3.a, Is.LessThan(0.15f), "Window 3 center must be alpha-cutout transparent.");
            Assert.That(fuselageWallPixel.a, Is.GreaterThan(0.95f), "Cabin wall between windows must remain opaque.");

            // Move camera horizontally and verify the 3 planes shift by 3 strictly distinct world-X amounts while Cockpit Door stays anchored!
            var cam = Camera.main;
            Assert.That(cam, Is.Not.Null);
            var director = Object.FindAnyObjectByType<SliceCameraDirector2D>();
            if (director != null) director.enabled = false;

            cam.transform.position = new Vector3(8f, 1.8f, -10f);
            yield return null;
            float farX0 = farSky.position.x;
            float midX0 = midWing.position.x;
            float nearX0 = nearFuselageTile.position.x;
            float doorX0 = cockpitDoor.transform.position.x;

            cam.transform.position = new Vector3(38f, 1.8f, -10f);
            yield return null;
            float deltaFar = Mathf.Abs(farSky.position.x - farX0);
            float deltaMid = Mathf.Abs(midWing.position.x - midX0);
            float deltaNear = Mathf.Abs(nearFuselageTile.position.x - nearX0);
            float deltaDoor = Mathf.Abs(cockpitDoor.transform.position.x - doorX0);

            Assert.That(deltaNear, Is.GreaterThan(0.5f), "Near fuselage plane should shift with parallax.");
            Assert.That(deltaMid, Is.GreaterThan(deltaNear + 0.5f),
                $"Mid wing plane ({deltaMid:F2}) must shift differently from near fuselage ({deltaNear:F2}) to create real window parallax.");
            Assert.That(deltaFar, Is.GreaterThan(deltaMid + 0.5f),
                $"Far sky plane ({deltaFar:F2}) must shift more with the camera (slower screen-relative motion) than mid wing ({deltaMid:F2}).");
            Assert.That(deltaDoor, Is.LessThanOrEqualTo(0.25f),
                "Cockpit boss bulkhead must remain anchored in stage coordinates.");
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
