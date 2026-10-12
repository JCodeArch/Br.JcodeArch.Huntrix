using System;
using System.IO;
using HuntrX.Gameplay.Special;
using UnityEngine;
namespace HuntrX.Gameplay.VerticalSlice
{
    [Serializable]
    public sealed class SlicePlaytestReport
    {
        public string StartedAtUtc;
        public float DurationSeconds;
        public int JumpInputs, DashInputs, AttackInputs, SwitchInputs, RescueInputs, SpecialAccepted, VisitedBeats, ActualRescues, Attempts;
        public float PerformancePoints;
        public bool CompleteWindow;
        public int HumanFunRating = -1, HumanDifficultyRating = -1;
        public string HumanComments = string.Empty;
    }
    [DisallowMultipleComponent]
    public sealed class SlicePlaytestRecorder2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private float observationSeconds = 60f;
        private readonly bool[] visited = new bool[16];
        private HuntrXSpecialController2D special;
        private bool observing;
        public SlicePlaytestReport Report { get; private set; }
        public event Action<SlicePlaytestReport> ObservationCompleted;
        private void OnEnable()
        {
            if (session == null) return;
            session.BeatChanged += Beat; session.InputObserved += Input;
            if (Report != null && !Report.CompleteWindow) observing = true;
            if (session.IsRunning && session.BeatIndex >= 0) Beat(session.BeatIndex);
        }
        private void Beat(int index)
        {
            if (Report == null)
            {
                if (float.IsNaN(observationSeconds) || float.IsInfinity(observationSeconds) || observationSeconds <= 0f || observationSeconds > 600f) return;
                Report = new SlicePlaytestReport { StartedAtUtc = DateTime.UtcNow.ToString("O") }; observing = true;
            }
            if (index >= 0 && index < visited.Length && !visited[index]) { visited[index] = true; Report.VisitedBeats++; }
            if (special != session.Special) { if (special != null) special.Completed -= Special; special = session.Special; if (special != null) special.Completed += Special; }
        }
        private void Input(SliceInputFrame frame)
        {
            if (!observing || Report == null) return;
            if (frame.JumpPressed) Report.JumpInputs++; if (frame.Dash) Report.DashInputs++; if (frame.Attack) Report.AttackInputs++;
            if (frame.Switch) Report.SwitchInputs++; if (frame.Interact) Report.RescueInputs++;
        }
        private void Special(bool cancelled) { if (observing && !cancelled && Report != null) Report.SpecialAccepted++; }
        private void Update()
        {
            if (!observing || Report == null || session == null || (!session.IsRunning && !session.IsComplete) || Time.timeScale <= 0f) return;
            Report.DurationSeconds = Mathf.Min(observationSeconds, Report.DurationSeconds + Time.unscaledDeltaTime);
            if (session.Performance != null) { Report.PerformancePoints = session.Performance.Snapshot.Points; Report.ActualRescues = session.Performance.Snapshot.RescuedFans; }
            if (session.CheckpointFlow != null) Report.Attempts = session.CheckpointFlow.AttemptNumber;
            if (Report.DurationSeconds < observationSeconds) return;
            observing = false; Report.CompleteWindow = true;
            var handlers = ObservationCompleted;
            if (handlers != null) foreach (Action<SlicePlaytestReport> handler in handlers.GetInvocationList())
                try { handler(Report); } catch (Exception exception) { Debug.LogException(exception, this); }
        }
        public bool TrySubmitFeedback(int fun, int difficulty, string comments)
        {
            if (Report == null || fun < 1 || fun > 5 || difficulty < 1 || difficulty > 5 || comments == null || comments.Length > 1000) return false;
            Report.HumanFunRating = fun; Report.HumanDifficultyRating = difficulty; Report.HumanComments = comments;
            return true;
        }
        public bool TrySaveReport(out string path)
        {
            path = string.Empty; if (Report == null) return false;
            try { string directory = Path.Combine(Application.persistentDataPath,"Playtests"); Directory.CreateDirectory(directory); path = Path.Combine(directory,"slice-"+Guid.NewGuid().ToString("N")+".json"); File.WriteAllText(path,JsonUtility.ToJson(Report,true)); return true; }
            catch(Exception exception) { path=string.Empty;Debug.LogException(exception,this);return false; }
        }
        private void OnDisable()
        {
            if (session != null) { session.BeatChanged -= Beat; session.InputObserved -= Input; }
            if (special != null) special.Completed -= Special; special=null; observing=false;
        }
    }
}
