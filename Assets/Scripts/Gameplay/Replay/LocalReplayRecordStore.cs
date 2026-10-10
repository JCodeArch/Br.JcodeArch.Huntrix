using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace HuntrX.Gameplay.Replay
{
    /// <summary>Local summaries only. Versioned bounded files never write campaign progress.</summary>
    public sealed class LocalReplayRecordStore
    {
        [Serializable] private sealed class Envelope
        {
            public int schemaVersion;
            public string profileId;
            public LocalReplayRecord[] records;
        }
        private const int MaximumRecords=64;
        private const int MaximumBytes=262144;
        private readonly string directory;
        public bool LastLoadRecovered {get;private set;}
        public LocalReplayRecordStore(string storageDirectory)
        {
            if(string.IsNullOrWhiteSpace(storageDirectory))throw new ArgumentException("Local replay storage directory is required.");
            directory=storageDirectory;
        }
        public bool TryLoad(string profile,out LocalReplayRecord[] records,out string error)
        {
            records=new LocalReplayRecord[0];error=string.Empty;LastLoadRecovered=false;
            if(!LocalReplayRecord.ValidProfileId(profile)){error="An opaque lowercase hexadecimal profile ID is required.";return false;}
            try
            {
                string path=PathFor(profile);
                if(!File.Exists(path)&&!File.Exists(path+".backup"))return true;
                if(TryRead(path,profile,out records))return true;
                if(TryRead(path+".backup",profile,out records)){LastLoadRecovered=true;return true;}
                error="Replay records are invalid or use an unsupported schema; existing data was preserved.";return false;
            }
            catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException)
            {error="Replay records could not be read; existing data was preserved.";return false;}
        }
        public bool TryAppend(LocalReplayRecord record,out string error)
        {
            error=string.Empty;
            if(record==null||!record.IsValid(record.profileId)){error="A valid completed run summary is required.";return false;}
            if(!TryLoad(record.profileId,out LocalReplayRecord[] existing,out error))return false;
            foreach(LocalReplayRecord item in existing)if(item.recordId==record.recordId)return true;
            int count=Math.Min(MaximumRecords-1,existing.Length);
            LocalReplayRecord[] next=new LocalReplayRecord[count+1];
            Array.Copy(existing,Math.Max(0,existing.Length-count),next,0,count);next[count]=record.Copy();
            return TryWrite(new Envelope{schemaVersion=1,profileId=record.profileId,records=next},out error);
        }
        public bool TryReadRankings(string profile,string chapter,out LocalReplayRecord[] ranked,out string error)
        {
            ranked=new LocalReplayRecord[0];
            if(string.IsNullOrWhiteSpace(chapter)||chapter.Length>128){error="A chapter ID is required.";return false;}
            if(!TryLoad(profile,out LocalReplayRecord[] records,out error))return false;
            List<LocalReplayRecord> matching=new List<LocalReplayRecord>();
            foreach(LocalReplayRecord item in records)if(item.chapterId==chapter)matching.Add(item.Copy());
            matching.Sort(Compare);ranked=matching.ToArray();return true;
        }
        public bool TryReset(string profile,out string error)
        {
            error=string.Empty;
            if(!LocalReplayRecord.ValidProfileId(profile)){error="An opaque profile ID is required.";return false;}
            // Explicitly resets this profile's replay history, never campaign/equipment files.
            if(!TryWrite(new Envelope{schemaVersion=1,profileId=profile,records=new LocalReplayRecord[0]},out error))return false;
            try { string backup=PathFor(profile)+".backup";if(File.Exists(backup))File.Delete(backup);return true; }
            catch(Exception e)when(e is IOException||e is UnauthorizedAccessException)
            {error="Active replay history was cleared, but removal of its old backup failed.";return false;}
        }
        private bool TryWrite(Envelope envelope,out string error)
        {
            error=string.Empty;string temp=null;
            try
            {
                Directory.CreateDirectory(directory);string path=PathFor(envelope.profileId);temp=path+".pending";
                byte[] bytes=Encoding.UTF8.GetBytes(JsonUtility.ToJson(envelope));
                if(bytes.Length>MaximumBytes){error="Replay storage limit exceeded.";return false;}
                using(FileStream stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
                {stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
                // When recovering from backup, never replace that valid backup with an invalid main file.
                if(File.Exists(path))
                {
                    if(TryRead(path,envelope.profileId,out _))File.Replace(temp,path,path+".backup",true);
                    else File.Replace(temp,path,null,true);
                }
                else File.Move(temp,path);
                return true;
            }
            catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is NotSupportedException||e is ArgumentException)
            {error="Replay records were not saved; the previous valid file/backup was preserved.";return false;}
            finally
            {
                if(temp!=null)try{if(File.Exists(temp))File.Delete(temp);}catch(IOException){}catch(UnauthorizedAccessException){}
            }
        }
        private bool TryRead(string path,string profile,out LocalReplayRecord[] records)
        {
            records=new LocalReplayRecord[0];
            if(!File.Exists(path)||new FileInfo(path).Length>MaximumBytes)return false;
            Envelope envelope;
            try{envelope=JsonUtility.FromJson<Envelope>(File.ReadAllText(path,Encoding.UTF8));}
            catch(ArgumentException){return false;}
            if(envelope==null||envelope.schemaVersion!=1||envelope.profileId!=profile||envelope.records==null||envelope.records.Length>MaximumRecords)return false;
            HashSet<string> seen=new HashSet<string>();
            foreach(LocalReplayRecord item in envelope.records)if(item==null||!item.IsValid(profile)||!seen.Add(item.recordId))return false;
            records=new LocalReplayRecord[envelope.records.Length];
            for(int i=0;i<records.Length;i++)records[i]=envelope.records[i].Copy();return true;
        }
        private string PathFor(string profile)=>Path.Combine(directory,profile+".replay.json");
        private static int Compare(LocalReplayRecord a,LocalReplayRecord b)
        {
            int c=b.rescuedFans.CompareTo(a.rescuedFans);if(c!=0)return c;
            c=b.performanceNormalized.CompareTo(a.performanceNormalized);if(c!=0)return c;
            c=b.maximumCombo.CompareTo(a.maximumCombo);if(c!=0)return c;
            c=b.noDamage.CompareTo(a.noDamage);if(c!=0)return c;
            c=a.elapsedSeconds.CompareTo(b.elapsedSeconds);if(c!=0)return c;
            c=b.completedUtcTicks.CompareTo(a.completedUtcTicks);return c!=0?c:string.CompareOrdinal(a.recordId,b.recordId);
        }
    }
}
