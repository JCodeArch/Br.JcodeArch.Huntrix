using System.Collections.Generic;
using UnityEngine;

namespace HuntrX.Gameplay.Levels
{
    /// <summary>
    /// Animates subtle living background movement in every stage (parallax depth, airplane cabin turbulence sway,
    /// sweeping concert stage lasers, pulsing Golden Honmoon rings, and drifting Seoul night spirit embers).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LivingStageBackdropAnimator2D : MonoBehaviour
    {
        private string stageKey = "show";
        private Vector3 baseLocalPos;
        private Vector3 baseLocalScale;
        private Camera activeCamera;
        private Vector3 initialCameraPos;
        private bool cameraSampled;
        private float clock;

        private readonly List<Transform> laserBeams = new List<Transform>(6);
        private readonly List<SpriteRenderer> laserRenderers = new List<SpriteRenderer>(6);
        private readonly List<Transform> floatingEmbers = new List<Transform>(16);
        private readonly List<Vector3> emberBasePositions = new List<Vector3>(16);
        private readonly List<Transform> windowFlightStreaks = new List<Transform>(32);
        private readonly List<float> windowStreakSpeeds = new List<float>(32);
        private readonly List<SpriteRenderer> cockpitBeaconLights = new List<SpriteRenderer>(4);
        private Transform honmoonPulseOverlay;
        private SpriteRenderer honmoonPulseRenderer;
        private Texture2D softGlowTex;
        private Sprite softGlowSprite;
        private Texture2D solidBlockTex;
        private Sprite solidBlockSprite;

        public void Configure(string resolvedStageKey, SpriteRenderer sr)
        {
            Configure(resolvedStageKey, transform.localPosition, transform.localScale);
        }

        public void Configure(string resolvedStageKey, Vector3 centerLocalPos, Vector3 scale)
        {
            stageKey = string.IsNullOrEmpty(resolvedStageKey) ? "show" : resolvedStageKey;
            baseLocalPos = centerLocalPos;
            baseLocalScale = scale;
            BuildLivingAmbientElements();
        }

        private void BuildLivingAmbientElements()
        {
            softGlowTex = CreateSoftGlowTexture();
            softGlowSprite = Sprite.Create(softGlowTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            solidBlockTex = CreateSolidBlockTexture();
            solidBlockSprite = Sprite.Create(solidBlockTex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);

            // 1. Subtle breathing Golden/Violet Honmoon sky aura overlay
            var pulseGo = new GameObject("Living_HonmoonSkyPulse");
            pulseGo.transform.SetParent(transform, false);
            pulseGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            pulseGo.transform.localScale = new Vector3(1.1f, 0.65f, 1f);
            honmoonPulseOverlay = pulseGo.transform;
            honmoonPulseRenderer = pulseGo.AddComponent<SpriteRenderer>();
            honmoonPulseRenderer.sprite = softGlowSprite;
            honmoonPulseRenderer.sortingOrder = -23;
            honmoonPulseRenderer.color = stageKey == "airplane"
                ? new Color(0.65f, 0.25f, 1f, 0.14f)
                : new Color(1f, 0.85f, 0.25f, 0.14f);

            // 2. Sweeping Concert / Skyline Neon Light Beams
            int beamCount = (stageKey == "show" || stageKey == "awards" || stageKey == "coex") ? 5 : 3;
            for (int i = 0; i < beamCount; i++)
            {
                var beamGo = new GameObject("Living_NeonBeam_" + i);
                beamGo.transform.SetParent(transform, false);
                float normX = -0.36f + (i * (0.72f / Mathf.Max(1, beamCount - 1)));
                beamGo.transform.localPosition = new Vector3(normX, -0.35f, 0f);
                beamGo.transform.localScale = new Vector3(0.045f, 1.35f, 1f);
                var sr = beamGo.AddComponent<SpriteRenderer>();
                sr.sprite = softGlowSprite;
                sr.sortingOrder = -22;
                sr.color = (i % 2 == 0)
                    ? new Color(0.15f, 0.92f, 1f, 0.16f)
                    : new Color(1f, 0.22f, 0.78f, 0.15f);
                laserBeams.Add(beamGo.transform);
                laserRenderers.Add(sr);
            }

            // 3. Drifting Honmoon Spirit Embers / Clouds
            for (int i = 0; i < 12; i++)
            {
                var emberGo = new GameObject("Living_SpiritEmber_" + i);
                emberGo.transform.SetParent(transform, false);
                float ex = -0.45f + (i * 0.082f);
                float ey = -0.30f + ((i * 37) % 100) * 0.006f;
                Vector3 pos = new Vector3(ex, ey, 0f);
                emberGo.transform.localPosition = pos;
                float sz = 0.018f + (i % 3) * 0.009f;
                emberGo.transform.localScale = new Vector3(sz, sz * 1.8f, 1f);
                var sr = emberGo.AddComponent<SpriteRenderer>();
                sr.sprite = softGlowSprite;
                sr.sortingOrder = -21;
                sr.color = (i % 3 == 0)
                    ? new Color(1f, 0.88f, 0.35f, 0.42f)
                    : new Color(0.35f, 0.95f, 1f, 0.36f);
                floatingEmbers.Add(emberGo.transform);
                emberBasePositions.Add(pos);
            }

            // 4. Airplane Window High-Speed Flight Clouds & Honmoon Jet-Stream Streaks + Right-End Cockpit Boss Bulkhead!
            if (stageKey == "airplane")
            {
                BuildAirplaneWindowFlightMotionAndCockpitDoor();
            }
        }

        private void BuildAirplaneWindowFlightMotionAndCockpitDoor()
        {
            // High-speed scrolling clouds, city lights & Honmoon streaks moving right-to-left across the window band (y in [-0.2f .. +1.25f])
            for (int i = 0; i < 28; i++)
            {
                var streakGo = new GameObject("Airplane_WindowFlightStreak_" + i);
                streakGo.transform.SetParent(transform, false);
                float startX = -38f + (i * 2.8f);
                float winY = -0.15f + ((i * 29) % 100) * 0.013f;
                streakGo.transform.localPosition = new Vector3(startX, winY, 0f);

                bool isCloud = (i % 3) == 0;
                float sx = isCloud ? (1.8f + (i % 4) * 0.45f) : (2.4f + (i % 3) * 0.85f);
                float sy = isCloud ? 0.48f : 0.11f;
                streakGo.transform.localScale = new Vector3(sx, sy, 1f);

                var sr = streakGo.AddComponent<SpriteRenderer>();
                sr.sprite = softGlowSprite;
                sr.sortingOrder = -24; // Renders across the window glass band behind characters and platforms
                sr.color = isCloud
                    ? new Color(0.55f, 0.42f, 0.98f, 0.38f)
                    : ((i % 2 == 0) ? new Color(0.20f, 0.95f, 1f, 0.52f) : new Color(1f, 0.30f, 0.85f, 0.46f));

                windowFlightStreaks.Add(streakGo.transform);
                windowStreakSpeeds.Add(isCloud ? (8.5f + (i % 4) * 2.2f) : (18f + (i % 5) * 4.5f));
            }

            // Right-end Cockpit Bulkhead Boss Door anchored in stage space (not drifting with backdrop parallax)
            Transform stageAnchorParent = transform.parent != null ? transform.parent : transform;
            var bulkheadRoot = new GameObject("Airplane_CockpitBossBulkhead");
            bulkheadRoot.transform.SetParent(stageAnchorParent, false);
            bulkheadRoot.transform.localPosition = stageAnchorParent != transform
                ? new Vector3(baseLocalPos.x + 22.2f, baseLocalPos.y - 0.2f, 0f)
                : new Vector3(22.2f, -0.2f, 0f);

            var doorFrame = new GameObject("Cockpit_ArmorDoorFrame");
            doorFrame.transform.SetParent(bulkheadRoot.transform, false);
            doorFrame.transform.localScale = new Vector3(4.2f, 7.2f, 1f);
            var frameSr = doorFrame.AddComponent<SpriteRenderer>();
            frameSr.sprite = solidBlockSprite;
            frameSr.sortingOrder = -18;
            frameSr.color = new Color(0.11f, 0.13f, 0.24f, 0.98f);

            var innerDoor = new GameObject("Cockpit_BlastDoor");
            innerDoor.transform.SetParent(bulkheadRoot.transform, false);
            innerDoor.transform.localScale = new Vector3(3.2f, 6.2f, 1f);
            var innerSr = innerDoor.AddComponent<SpriteRenderer>();
            innerSr.sprite = solidBlockSprite;
            innerSr.sortingOrder = -17;
            innerSr.color = new Color(0.05f, 0.06f, 0.13f, 0.98f);

            // Glowing red/amber Capcom Boss Warning Stripes & Radar Beacons on the Cockpit Door
            for (int b = 0; b < 3; b++)
            {
                var beacon = new GameObject("Cockpit_WarningBeacon_" + b);
                beacon.transform.SetParent(bulkheadRoot.transform, false);
                beacon.transform.localPosition = new Vector3(0f, 2.2f - b * 1.4f, 0f);
                beacon.transform.localScale = new Vector3(2.6f, 0.28f, 1f);
                var bSr = beacon.AddComponent<SpriteRenderer>();
                bSr.sprite = solidBlockSprite;
                bSr.sortingOrder = -16;
                bSr.color = new Color(1f, 0.18f, 0.28f, 0.92f);
                cockpitBeaconLights.Add(bSr);
            }
        }

        private void LateUpdate()
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            clock = (clock + dt) % 10000f;

            if (activeCamera == null) activeCamera = Camera.main;
            Vector3 parallaxOffset = Vector3.zero;
            if (activeCamera != null)
            {
                if (!cameraSampled)
                {
                    initialCameraPos = activeCamera.transform.position;
                    cameraSampled = true;
                }
                Vector3 camDelta = activeCamera.transform.position - initialCameraPos;
                if (!float.IsNaN(camDelta.x) && !float.IsInfinity(camDelta.x))
                {
                    // Smooth parallax tracking so backdrop moves subtly with the camera
                    parallaxOffset = new Vector3(camDelta.x * 0.14f, camDelta.y * 0.08f, 0f);
                }
            }

            // Subtle living background motion (gentle breathing zoom + stage-specific sway)
            float swayX = Mathf.Sin(clock * 0.85f) * 0.15f;
            float swayY = Mathf.Cos(clock * 1.15f) * 0.04f;
            float rollDeg = 0f;
            if (stageKey == "airplane")
            {
                // High-altitude private jet subtle vibration without detaching the cabin floor line
                swayY = Mathf.Sin(clock * 2.2f) * 0.03f;
                rollDeg = Mathf.Sin(clock * 1.25f) * 0.18f;
                parallaxOffset.y *= 0.25f;
            }

            float breathScale = 1f + Mathf.Sin(clock * 0.9f) * 0.005f;
            transform.localPosition = baseLocalPos + parallaxOffset + new Vector3(swayX, swayY, 0f);
            transform.localScale = new Vector3(baseLocalScale.x * breathScale, baseLocalScale.y * breathScale, 1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rollDeg);

            // Animate high-speed airplane window flight clouds & streaks moving right-to-left!
            for (int i = 0; i < windowFlightStreaks.Count; i++)
            {
                Transform st = windowFlightStreaks[i];
                if (st == null) continue;
                Vector3 p = st.localPosition;
                p.x -= windowStreakSpeeds[i] * dt;
                if (p.x < -42f) p.x += 84f;
                st.localPosition = p;
            }

            // Animate Cockpit Boss Door Warning Beacons
            for (int i = 0; i < cockpitBeaconLights.Count; i++)
            {
                SpriteRenderer bSr = cockpitBeaconLights[i];
                if (bSr == null) continue;
                float flash = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(clock * 5.5f + i * 0.9f));
                bSr.color = new Color(1f, 0.18f + 0.35f * flash, 0.22f, flash);
            }

            // Animate sweeping concert lasers / neon beams
            for (int i = 0; i < laserBeams.Count; i++)
            {
                if (laserBeams[i] == null) continue;
                float angle = Mathf.Sin(clock * (1.1f + i * 0.25f) + i * 1.3f) * 24f;
                laserBeams[i].localRotation = Quaternion.Euler(0f, 0f, angle);
                if (laserRenderers[i] != null)
                {
                    Color c = laserRenderers[i].color;
                    c.a = 0.09f + 0.09f * Mathf.Abs(Mathf.Sin(clock * 2.2f + i));
                    laserRenderers[i].color = c;
                }
            }

            // Animate drifting spirit embers / clouds
            for (int i = 0; i < floatingEmbers.Count; i++)
            {
                if (floatingEmbers[i] == null) continue;
                Vector3 basePos = emberBasePositions[i];
                float driftY = ((clock * (0.04f + (i % 3) * 0.015f) + i * 0.17f) % 0.75f) - 0.35f;
                float driftX = basePos.x + Mathf.Sin(clock * 1.5f + i) * 0.03f;
                floatingEmbers[i].localPosition = new Vector3(driftX, driftY, 0f);
            }

            if (honmoonPulseOverlay != null && honmoonPulseRenderer != null)
            {
                float pulse = 1f + Mathf.Sin(clock * 1.8f) * 0.08f;
                honmoonPulseOverlay.localScale = new Vector3(1.1f * pulse, 0.65f * pulse, 1f);
                Color c = honmoonPulseRenderer.color;
                c.a = 0.08f + 0.07f * (0.5f + 0.5f * Mathf.Sin(clock * 2.0f));
                honmoonPulseRenderer.color = c;
            }
        }

        private static Texture2D CreateSoftGlowTexture()
        {
            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dx = (x - 15.5f) / 15.5f;
                    float dy = (y - 15.5f) / 15.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSolidBlockTexture()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool border = x == 0 || x == 15 || y == 0 || y == 15;
                    tex.SetPixel(x, y, border ? new Color(0.35f, 0.85f, 1f, 1f) : Color.white);
                }
            }
            tex.Apply();
            return tex;
        }

        private void OnDestroy()
        {
            if (softGlowSprite != null) Destroy(softGlowSprite);
            if (softGlowTex != null) Destroy(softGlowTex);
            if (solidBlockSprite != null) Destroy(solidBlockSprite);
            if (solidBlockTex != null) Destroy(solidBlockTex);
        }
    }
}
