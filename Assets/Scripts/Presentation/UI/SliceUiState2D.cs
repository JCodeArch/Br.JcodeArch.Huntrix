using System;
using UnityEngine;

namespace HuntrX.Presentation.UI
{
    /// <summary>
    /// Pure data snapshot representing observable UI state.
    /// Decouples rendering and views from gameplay logic and state mutation.
    /// </summary>
    [Serializable]
    public struct SliceUiState2D : IEquatable<SliceUiState2D>
    {
        public int HealthCurrent;
        public int HealthMaximum;
        public int HonmoonCurrent;
        public float SpecialCharge;
        public bool SpecialReady;
        public int FoodCount;
        public int FansRescued;
        public int FansTotal;
        public string ActiveCharacter;
        public string SelectedAgeProfile;
        public string BeatTitle;
        public bool IsRunning;
        public bool IsComplete;
        public bool IsPaused;

        public static SliceUiState2D CreateDefault()
        {
            return new SliceUiState2D
            {
                HealthCurrent = 100,
                HealthMaximum = 100,
                HonmoonCurrent = 100,
                SpecialCharge = 0f,
                SpecialReady = false,
                FoodCount = 0,
                FansRescued = 0,
                FansTotal = 0,
                ActiveCharacter = "Rumi",
                SelectedAgeProfile = "9-12",
                BeatTitle = "Preparando...",
                IsRunning = false,
                IsComplete = false,
                IsPaused = false
            };
        }

        public bool Equals(SliceUiState2D other)
        {
            return HealthCurrent == other.HealthCurrent &&
                   HealthMaximum == other.HealthMaximum &&
                   HonmoonCurrent == other.HonmoonCurrent &&
                   Mathf.Approximately(SpecialCharge, other.SpecialCharge) &&
                   SpecialReady == other.SpecialReady &&
                   FoodCount == other.FoodCount &&
                   FansRescued == other.FansRescued &&
                   FansTotal == other.FansTotal &&
                   string.Equals(ActiveCharacter, other.ActiveCharacter, StringComparison.Ordinal) &&
                   string.Equals(SelectedAgeProfile, other.SelectedAgeProfile, StringComparison.Ordinal) &&
                   string.Equals(BeatTitle, other.BeatTitle, StringComparison.Ordinal) &&
                   IsRunning == other.IsRunning &&
                   IsComplete == other.IsComplete &&
                   IsPaused == other.IsPaused;
        }

        public override bool Equals(object obj)
        {
            return obj is SliceUiState2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = HealthCurrent;
                hash = (hash * 397) ^ HealthMaximum;
                hash = (hash * 397) ^ HonmoonCurrent;
                hash = (hash * 397) ^ SpecialCharge.GetHashCode();
                hash = (hash * 397) ^ SpecialReady.GetHashCode();
                hash = (hash * 397) ^ FoodCount;
                hash = (hash * 397) ^ FansRescued;
                hash = (hash * 397) ^ FansTotal;
                hash = (hash * 397) ^ (ActiveCharacter != null ? ActiveCharacter.GetHashCode() : 0);
                hash = (hash * 397) ^ (SelectedAgeProfile != null ? SelectedAgeProfile.GetHashCode() : 0);
                hash = (hash * 397) ^ (BeatTitle != null ? BeatTitle.GetHashCode() : 0);
                hash = (hash * 397) ^ IsRunning.GetHashCode();
                hash = (hash * 397) ^ IsComplete.GetHashCode();
                hash = (hash * 397) ^ IsPaused.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(SliceUiState2D left, SliceUiState2D right) => left.Equals(right);
        public static bool operator !=(SliceUiState2D left, SliceUiState2D right) => !left.Equals(right);
    }
}
