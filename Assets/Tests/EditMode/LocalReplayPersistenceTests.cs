using System;
using System.IO;
using HuntrX.Gameplay.Replay;
using NUnit.Framework;
namespace HuntrX.Tests.EditMode
{
    public sealed class LocalReplayPersistenceTests
    {
        private string directory;
        private const string Profile="0123456789abcdef0123456789abcdef";
        [SetUp] public void SetUp()=>directory=Path.Combine(Path.GetTempPath(),"huntrx-replay-test-"+Guid.NewGuid().ToString("N"));
        [TearDown] public void TearDown(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        private static LocalReplayRecord Record(int fans=0)=>new LocalReplayRecord
        {
            recordId=Guid.NewGuid().ToString("N"),profileId=Profile,chapterId="prototype",rescuedFans=fans,
            performancePoints=20,performanceNormalized=.2f,elapsedSeconds=30,maximumCombo=2,noDamage=true,
            completedUtcTicks=DateTime.UtcNow.Ticks
        };
        [Test] public void AppendIsIdempotentAndReturnsCopies()
        {
            var store=new LocalReplayRecordStore(directory);var record=Record();
            Assert.That(store.TryAppend(record,out _),Is.True);Assert.That(store.TryAppend(record,out _),Is.True);
            Assert.That(store.TryLoad(Profile,out var records,out _),Is.True);Assert.That(records.Length,Is.EqualTo(1));
            records[0].rescuedFans=99;Assert.That(store.TryLoad(Profile,out records,out _),Is.True);Assert.That(records[0].rescuedFans,Is.Zero);
        }
        [Test] public void HistoryIsBoundedAndRankingsPrioritizeRescuedFans()
        {
            var store=new LocalReplayRecordStore(directory);
            for(int i=0;i<70;i++)Assert.That(store.TryAppend(Record(i),out _),Is.True);
            Assert.That(store.TryReadRankings(Profile,"prototype",out var records,out _),Is.True);
            Assert.That(records.Length,Is.EqualTo(64));Assert.That(records[0].rescuedFans,Is.EqualTo(69));
        }
        [Test] public void InvalidSchemaIsPreservedAndCannotBeSilentlyOverwritten()
        {
            Directory.CreateDirectory(directory);string path=Path.Combine(directory,Profile+".replay.json");
            const string invalid="{\"schemaVersion\":999}";File.WriteAllText(path,invalid);
            var store=new LocalReplayRecordStore(directory);Assert.That(store.TryAppend(Record(),out _),Is.False);
            Assert.That(File.ReadAllText(path),Is.EqualTo(invalid));
        }
        [Test] public void InvalidProfileCannotEscapeStorageDirectory()
        {
            var store=new LocalReplayRecordStore(directory);Assert.That(store.TryLoad("../campaign",out _,out _),Is.False);
            Assert.That(Directory.Exists(directory),Is.False);
        }
        [Test] public void ExplicitResetClearsOnlyReplayHistory()
        {
            var store=new LocalReplayRecordStore(directory);Assert.That(store.TryAppend(Record(),out _),Is.True);
            string campaign=Path.Combine(directory,"campaign.json");File.WriteAllText(campaign,"untouched");
            Assert.That(store.TryReset(Profile,out _),Is.True);Assert.That(store.TryLoad(Profile,out var records,out _),Is.True);
            Assert.That(records,Is.Empty);Assert.That(File.ReadAllText(campaign),Is.EqualTo("untouched"));
        }
    }
}
