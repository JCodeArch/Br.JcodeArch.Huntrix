using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Performance;
using HuntrX.Gameplay.Replay;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HuntrX.Tests.PlayMode
{
    public sealed class LocalReplayPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<Object> disposables = new List<Object>();
        private string tempStorageDir;

        private sealed class TestChapterAccess : IReplayChapterAccess
        {
            public bool Allowed = true;
            public bool IsReplayAllowed(string opaqueProfileId, string chapterId) => Allowed;
        }

        [SetUp]
        public void SetUp()
        {
            tempStorageDir = Path.Combine(Path.GetTempPath(), "HuntrX_Replay_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempStorageDir);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = disposables.Count - 1; i >= 0; i--)
            {
                if (disposables[i] != null)
                {
                    Object.DestroyImmediate(disposables[i]);
                }
            }
            disposables.Clear();

            if (!string.IsNullOrEmpty(tempStorageDir) && Directory.Exists(tempStorageDir))
            {
                try { Directory.Delete(tempStorageDir, true); } catch { }
            }
        }

        private static string GenerateProfileId() => Guid.NewGuid().ToString("N").ToLowerInvariant();

        private static LocalReplayRecord CreateRecord(
            string profileId,
            string chapterId,
            int rescuedFans,
            float performancePoints,
            float performanceNormalized,
            int maximumCombo,
            bool noDamage,
            float elapsedSeconds)
        {
            return new LocalReplayRecord
            {
                recordId = Guid.NewGuid().ToString("N"),
                profileId = profileId,
                chapterId = chapterId,
                rescuedFans = rescuedFans,
                performancePoints = performancePoints,
                performanceNormalized = performanceNormalized,
                maximumCombo = maximumCombo,
                noDamage = noDamage,
                elapsedSeconds = elapsedSeconds,
                completedUtcTicks = DateTime.UtcNow.Ticks
            };
        }

        [Test]
        public void LocalReplayRecord_ValidatesProfileIdAndRanges()
        {
            string profile = GenerateProfileId();
            Assert.That(LocalReplayRecord.ValidProfileId(profile), Is.True);
            Assert.That(LocalReplayRecord.ValidProfileId(null), Is.False);
            Assert.That(LocalReplayRecord.ValidProfileId("invalid_len"), Is.False);
            Assert.That(LocalReplayRecord.ValidProfileId(profile.ToUpperInvariant()), Is.False);

            LocalReplayRecord record = CreateRecord(profile, "ch_01", 5, 1200f, 0.6f, 15, true, 45f);
            Assert.That(record.IsValid(profile), Is.True);

            LocalReplayRecord mismatchedProfile = record.Copy();
            mismatchedProfile.profileId = GenerateProfileId();
            Assert.That(mismatchedProfile.IsValid(profile), Is.False);

            LocalReplayRecord negativeFans = record.Copy();
            negativeFans.rescuedFans = -1;
            Assert.That(negativeFans.IsValid(profile), Is.False);

            LocalReplayRecord nanPoints = record.Copy();
            nanPoints.performancePoints = float.NaN;
            Assert.That(nanPoints.IsValid(profile), Is.False);

            LocalReplayRecord negativeNormalized = record.Copy();
            negativeNormalized.performanceNormalized = -0.1f;
            Assert.That(negativeNormalized.IsValid(profile), Is.False);

            LocalReplayRecord excessiveNormalized = record.Copy();
            excessiveNormalized.performanceNormalized = 1.1f;
            Assert.That(excessiveNormalized.IsValid(profile), Is.False);

            LocalReplayRecord excessiveElapsed = record.Copy();
            excessiveElapsed.elapsedSeconds = 90000f;
            Assert.That(excessiveElapsed.IsValid(profile), Is.False);
        }

        [Test]
        public void LocalReplayRecordStore_AppendAndRankingsSortingContract()
        {
            var store = new LocalReplayRecordStore(tempStorageDir);
            string profile = GenerateProfileId();
            string chapter = "chapter_neon_city";

            LocalReplayRecord recA = CreateRecord(profile, chapter, 3, 500f, 0.4f, 10, false, 80f);
            LocalReplayRecord recB = CreateRecord(profile, chapter, 5, 800f, 0.7f, 25, true, 90f);
            LocalReplayRecord recC = CreateRecord(profile, chapter, 5, 950f, 0.85f, 30, true, 75f);
            LocalReplayRecord otherChapter = CreateRecord(profile, "chapter_other", 10, 1000f, 1f, 50, true, 30f);

            Assert.That(store.TryAppend(recA, out string errA), Is.True, errA);
            Assert.That(store.TryAppend(recB, out string errB), Is.True, errB);
            Assert.That(store.TryAppend(recC, out string errC), Is.True, errC);
            Assert.That(store.TryAppend(otherChapter, out string errOther), Is.True, errOther);

            // Appending duplicate recordId should be idempotent
            Assert.That(store.TryAppend(recC, out string errDup), Is.True, errDup);

            bool loaded = store.TryLoad(profile, out LocalReplayRecord[] allRecords, out string loadErr);
            Assert.That(loaded, Is.True, loadErr);
            Assert.That(allRecords.Length, Is.EqualTo(4));

            bool ranked = store.TryReadRankings(profile, chapter, out LocalReplayRecord[] rankings, out string rankErr);
            Assert.That(ranked, Is.True, rankErr);
            Assert.That(rankings.Length, Is.EqualTo(3));

            // Ranking criteria: rescuedFans desc, then normalized desc, then combo desc, then noDamage desc, then elapsed asc
            Assert.That(rankings[0].recordId, Is.EqualTo(recC.recordId), "Top rank: 5 fans, 0.85 normalized");
            Assert.That(rankings[1].recordId, Is.EqualTo(recB.recordId), "Second rank: 5 fans, 0.70 normalized");
            Assert.That(rankings[2].recordId, Is.EqualTo(recA.recordId), "Third rank: 3 fans");
        }

        [Test]
        public void LocalReplayRecordStore_RecoversBackupWhenMainCorrupted()
        {
            var store = new LocalReplayRecordStore(tempStorageDir);
            string profile = GenerateProfileId();

            LocalReplayRecord rec1 = CreateRecord(profile, "ch_01", 4, 600f, 0.5f, 12, true, 60f);
            Assert.That(store.TryAppend(rec1, out _), Is.True);

            LocalReplayRecord rec2 = CreateRecord(profile, "ch_01", 6, 900f, 0.8f, 20, true, 55f);
            Assert.That(store.TryAppend(rec2, out _), Is.True);

            string mainPath = Path.Combine(tempStorageDir, profile + ".replay.json");
            string backupPath = mainPath + ".backup";
            Assert.That(File.Exists(mainPath), Is.True);
            Assert.That(File.Exists(backupPath), Is.True);

            // Corrupt main file
            File.WriteAllText(mainPath, "{ corrupted json payload");

            bool recovered = store.TryLoad(profile, out LocalReplayRecord[] records, out string error);
            Assert.That(recovered, Is.True, error);
            Assert.That(store.LastLoadRecovered, Is.True);
            Assert.That(records.Length, Is.EqualTo(1));
            Assert.That(records[0].recordId, Is.EqualTo(rec1.recordId));
        }

        [Test]
        public void LocalReplayRecordStore_ResetClearsHistoryAndRemovesBackup()
        {
            var store = new LocalReplayRecordStore(tempStorageDir);
            string profile = GenerateProfileId();

            LocalReplayRecord rec1 = CreateRecord(profile, "ch_01", 2, 300f, 0.3f, 5, false, 70f);
            Assert.That(store.TryAppend(rec1, out _), Is.True);
            LocalReplayRecord rec2 = CreateRecord(profile, "ch_01", 4, 600f, 0.5f, 10, true, 65f);
            Assert.That(store.TryAppend(rec2, out _), Is.True);

            Assert.That(store.TryReset(profile, out string resetErr), Is.True, resetErr);

            string backupPath = Path.Combine(tempStorageDir, profile + ".replay.json.backup");
            Assert.That(File.Exists(backupPath), Is.False);

            Assert.That(store.TryLoad(profile, out LocalReplayRecord[] records, out string loadErr), Is.True, loadErr);
            Assert.That(records.Length, Is.EqualTo(0));
        }

        [Test]
        public void LocalReplayController_ConfigureAndRunLifecycle()
        {
            var slotPrefab = Resources.Load<GameObject>("Characters/HuntrXPlayerSlot_Prototype");
            Assert.That(slotPrefab, Is.Not.Null);
            GameObject slotInstance = Object.Instantiate(slotPrefab);
            disposables.Add(slotInstance);

            var manager = slotInstance.GetComponent<CharacterManager2D>();
            var flow = slotInstance.GetComponent<CheckpointAttemptFlow2D>();
            var rumiPrefab = Resources.Load<GameObject>("Characters/Rumi_Prototype");
            Assert.That(manager.Configure(rumiPrefab, flow, slotInstance.transform.Find("StageStart"), "start"), Is.True);
            Assert.That(manager.TrySpawn(), Is.True);

            var perfPrefab = Resources.Load<GameObject>("Performance/PerformanceRun_Prototype");
            Assert.That(perfPrefab, Is.Not.Null);
            GameObject perfInstance = Object.Instantiate(perfPrefab);
            disposables.Add(perfInstance);

            var performance = perfInstance.GetComponent<CombatPerformanceController2D>();
            var definition = (CombatPerformanceDefinition)typeof(CombatPerformanceController2D)
                .GetField("definition", PrivateInstance)
                .GetValue(performance);
            Assert.That(performance.Configure(definition, manager, new FanRescueController2D[0]), Is.True);

            GameObject controllerGo = new GameObject("ReplayControllerHost");
            disposables.Add(controllerGo);
            var controller = controllerGo.AddComponent<LocalReplayController2D>();

            var access = new TestChapterAccess();
            var store = new LocalReplayRecordStore(tempStorageDir);
            string profile = GenerateProfileId();
            string chapter = "chapter_show_opening";

            // Configure validation
            Assert.That(controller.Configure("invalid_profile", chapter, performance, access, store), Is.False);
            Assert.That(controller.Configure(profile, null, performance, access, store), Is.False);
            Assert.That(controller.Configure(profile, chapter, null, access, store), Is.False);
            Assert.That(controller.Configure(profile, chapter, performance, access, store), Is.True);

            // Replay access gate
            access.Allowed = false;
            Assert.That(controller.TryRequestReplay(out string deniedRequestErr), Is.False);
            Assert.That(controller.TryBeginRun(true, out string deniedBeginErr), Is.False);

            access.Allowed = true;
            bool replayRequested = false;
            controller.ReplayRequested += ch => replayRequested = true;
            Assert.That(controller.TryRequestReplay(out string allowedRequestErr), Is.True, allowedRequestErr);
            Assert.That(replayRequested, Is.True);

            // Begin Run
            Assert.That(controller.TryBeginRun(true, out string beginErr), Is.True, beginErr);
            Assert.That(controller.IsRecording, Is.True);
            Assert.That(performance.IsRunning, Is.True);

            // Complete Run
            LocalReplayRecord savedRecord = null;
            controller.RunSaved += r => savedRecord = r;

            Assert.That(controller.TryCompleteRun(out string completeErr), Is.True, completeErr);
            Assert.That(controller.IsRecording, Is.False);
            Assert.That(controller.HasUnsavedRecord, Is.False);
            Assert.That(savedRecord, Is.Not.Null);
            Assert.That(savedRecord.chapterId, Is.EqualTo(chapter));
            Assert.That(savedRecord.profileId, Is.EqualTo(profile));

            // Verify written ranking
            Assert.That(controller.TryReadRankings(out LocalReplayRecord[] chapterRankings, out string rankErr), Is.True, rankErr);
            Assert.That(chapterRankings.Length, Is.EqualTo(1));
            Assert.That(chapterRankings[0].recordId, Is.EqualTo(savedRecord.recordId));
        }
    }
}
