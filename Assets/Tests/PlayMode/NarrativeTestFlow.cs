using System.Collections;
using HuntrX.Gameplay.Narrative;
using NUnit.Framework;
using UnityEngine;
namespace HuntrX.Tests.PlayMode
{
    internal static class NarrativeTestFlow
    {
        public static IEnumerator CompleteCurrentDialogue()
        {
            // Let the composition create the authored cue, then advance it as the player would.
            yield return null;
            var composition = Object.FindAnyObjectByType<SliceNarrativeComposition2D>();
            if (composition == null) yield break;
            var presenter = composition.Presenter;
            if (presenter == null) yield break;
            for (int i = 0; i < 64 && presenter.IsPresenting; i++) presenter.Advance();
            Assert.That(presenter.IsPresenting, Is.False, "Dialogue must finish within the bounded explicit advances.");
            yield return null;
        }
    }
}
