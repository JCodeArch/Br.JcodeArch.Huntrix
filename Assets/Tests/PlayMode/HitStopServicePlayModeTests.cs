using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    /// <summary>
    /// These tests own the process-wide Unity time settings. A fixture gate serializes cases;
    /// setup and teardown replace any pre-scene service and restore both global values.
    /// </summary>
    public sealed class HitStopServicePlayModeTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private HitStopService service;
        private static int timeScaleFixtureGate;
        private bool ownsFixtureGate;
        private float restoreTimeScale;
        private float testBaseTimeScale;
        private float baselineFixedDeltaTime;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new WaitUntil(TryAcquireFixtureGate);
            ownsFixtureGate = true;
            yield return DestroyAllServices();

            // Capture only after stale owned freezes/pauses have been released by destruction.
            restoreTimeScale = Time.timeScale;
            baselineFixedDeltaTime = Time.fixedDeltaTime;
            testBaseTimeScale = restoreTimeScale > 0f ? restoreTimeScale : 1f;
            Time.timeScale = testBaseTimeScale;
            service = HitStopService.EnsureInstance();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            try
            {
                foreach (GameObject createdObject in createdObjects)
                {
                    if (createdObject != null)
                    {
                        Object.Destroy(createdObject);
                    }
                }

                createdObjects.Clear();
                yield return DestroyAllServices();
                Time.timeScale = restoreTimeScale;
                Time.fixedDeltaTime = baselineFixedDeltaTime;
                yield return null;
                Assert.That(Time.timeScale, Is.EqualTo(restoreTimeScale).Within(0.001f));
                Assert.That(Time.fixedDeltaTime, Is.EqualTo(baselineFixedDeltaTime).Within(0.000001f));
            }
            finally
            {
                if (ownsFixtureGate)
                {
                    Interlocked.Exchange(ref timeScaleFixtureGate, 0);
                    ownsFixtureGate = false;
                }
            }
        }

        [UnityTest]
        public IEnumerator AcceptedRequestFreezesImmediatelyAndExpiresUsingUnscaledTime()
        {
            service.RequestHitStop(0.18f);

            Assert.That(Time.timeScale, Is.Zero);
            yield return new WaitForSecondsRealtime(0.06f);
            Assert.That(Time.timeScale, Is.Zero);

            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator OverlappingRequestsKeepTheLatestDeadline()
        {
            service.RequestHitStop(0.22f);
            yield return new WaitForSecondsRealtime(0.1f);
            service.RequestHitStop(0.3f);

            yield return new WaitForSecondsRealtime(0.16f);
            Assert.That(Time.timeScale, Is.Zero, "The first deadline must not end the extended stop.");

            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator RestoresTheNonDefaultBaseScaleAfterHitStop()
        {
            yield return RecreateServiceAtScale(0.45f);
            service.RequestHitStop(0.08f);

            Assert.That(Time.timeScale, Is.Zero);
            yield return new WaitForSecondsRealtime(0.14f);
            Assert.That(Time.timeScale, Is.EqualTo(0.45f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator PauseKeepsAnActiveHitStopPendingUntilResume()
        {
            service.RequestHitStop(0.35f);
            service.SetPaused(true);

            Assert.That(Time.timeScale, Is.Zero);
            yield return new WaitForSecondsRealtime(0.08f);
            service.SetPaused(false);

            Assert.That(Time.timeScale, Is.Zero, "Resuming before the deadline must reapply the remaining freeze.");
            yield return new WaitForSecondsRealtime(0.32f);
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator ResumeAfterPendingDeadlineRestoresBaseScaleWithoutRefreezing()
        {
            service.RequestHitStop(0.08f);
            service.SetPaused(true);
            yield return new WaitForSecondsRealtime(0.14f);

            Assert.That(Time.timeScale, Is.Zero);
            service.SetPaused(false);
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
            yield return new WaitForSecondsRealtime(0.04f);
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator RequestMadeWhileAlreadyPausedIsIgnored()
        {
            service.SetPaused(true);
            service.RequestHitStop(0.16f);

            Assert.That(Time.timeScale, Is.Zero);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Time.timeScale, Is.Zero, "An ignored request must not create a pending hit-stop deadline.");

            service.SetPaused(false);
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator RequestDuringPreExistingExternalPauseDoesNotClaimTimeScale()
        {
            Time.timeScale = 0f;
            service.RequestHitStop(0.16f);

            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(Time.timeScale, Is.Zero);

            Time.timeScale = testBaseTimeScale;
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator NonzeroExternalScaleWriteDuringFreezeRelinquishesOwnership()
        {
            service.RequestHitStop(0.4f);
            Time.timeScale = 0.7f;
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(0.7f).Within(0.001f));
            yield return new WaitForSecondsRealtime(0.08f);
            Assert.That(Time.timeScale, Is.EqualTo(0.7f).Within(0.001f),
                "The expired former deadline must not overwrite an external scale after ownership is lost.");
        }

        [UnityTest]
        public IEnumerator NonzeroExternalScaleWriteDuringPauseRelinquishesPauseAndPendingStop()
        {
            service.RequestHitStop(0.4f);
            service.SetPaused(true);
            Time.timeScale = 0.7f;
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(0.7f).Within(0.001f));
            service.SetPaused(false);
            yield return null;
            Assert.That(Time.timeScale, Is.EqualTo(0.7f).Within(0.001f));

            service.RequestHitStop(0.06f);
            Assert.That(Time.timeScale, Is.Zero, "Ownership loss must clear old state but allow a later fresh request.");
        }

        [UnityTest]
        public IEnumerator NeverChangesFixedDeltaTime()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            service.RequestHitStop(0.1f);
            service.SetPaused(true);
            yield return new WaitForSecondsRealtime(0.14f);
            service.SetPaused(false);
            yield return null;

            Assert.That(Time.fixedDeltaTime, Is.EqualTo(fixedDeltaTime).Within(0.000001f));
        }

        [UnityTest]
        public IEnumerator SubsystemResetRebindsSurvivingRunnerAndReleasesOldState()
        {
            service.RequestHitStop(0.5f);
            Assert.That(Time.timeScale, Is.Zero);

            MethodInfo resetHook = typeof(HitStopService).GetMethod(
                "ResetStaticState",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(resetHook, Is.Not.Null);
            resetHook.Invoke(null, null);

            HitStopService rebound = HitStopService.EnsureInstance();
            Assert.That(rebound, Is.SameAs(service));
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
            Assert.That(Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include), Has.Length.EqualTo(1));

            yield return new WaitForSecondsRealtime(0.08f);
            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f),
                "A pre-reset deadline must not re-freeze the surviving runner.");
        }

        [UnityTest]
        public IEnumerator DuplicateRunnerDoesNotReplaceTheExistingService()
        {
            HitStopService original = service;
            GameObject duplicateObject = CreateObject("DuplicateHitStopService");
            duplicateObject.AddComponent<HitStopService>();
            yield return null;

            HitStopService[] services = Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include);
            Assert.That(services, Has.Length.EqualTo(1));
            Assert.That(services[0], Is.SameAs(original));
        }

        [UnityTest]
        public IEnumerator TestFallbackCreatesOnePersistentRunnerOnDemand()
        {
            yield return DestroyAllServices();
            service = HitStopService.EnsureInstance();
            yield return null;

            Assert.That(service, Is.Not.Null);
            Assert.That(service.gameObject.scene.name, Is.EqualTo("DontDestroyOnLoad"));
            Assert.That(Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include), Has.Length.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisablingRunnerDuringOwnedFreezeRestoresBaseScale()
        {
            service.RequestHitStop(0.5f);
            service.enabled = false;
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator DisablingRunnerDuringOwnedPauseRestoresBaseScale()
        {
            service.SetPaused(true);
            service.enabled = false;
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator DestroyingRunnerDuringOwnedFreezeRestoresBaseScale()
        {
            service.RequestHitStop(0.5f);
            Object.Destroy(service.gameObject);
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
            Assert.That(Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include), Is.Empty);
        }

        [UnityTest]
        public IEnumerator DestroyingRunnerDuringOwnedPauseRestoresBaseScale()
        {
            service.SetPaused(true);
            Object.Destroy(service.gameObject);
            yield return null;

            Assert.That(Time.timeScale, Is.EqualTo(testBaseTimeScale).Within(0.001f));
            Assert.That(Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include), Is.Empty);
        }

        private IEnumerator RecreateServiceAtScale(float baseScale)
        {
            yield return DestroyAllServices();
            Time.timeScale = baseScale;
            service = HitStopService.EnsureInstance();
            yield return null;
        }

        private GameObject CreateObject(string objectName)
        {
            GameObject gameObject = new GameObject(objectName);
            createdObjects.Add(gameObject);
            return gameObject;
        }

        private static IEnumerator DestroyAllServices()
        {
            HitStopService[] services = Object.FindObjectsByType<HitStopService>(FindObjectsInactive.Include);
            foreach (HitStopService existing in services)
            {
                if (existing != null)
                {
                    Object.Destroy(existing.gameObject);
                }
            }

            yield return null;
        }

        private static bool TryAcquireFixtureGate() =>
            Interlocked.CompareExchange(ref timeScaleFixtureGate, 1, 0) == 0;
    }
}
