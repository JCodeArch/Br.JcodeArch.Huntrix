using System.Collections;
using HuntrX.Gameplay.Audio;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AirplaneAudioLifecyclePlayModeTests
    {
        private Scene slice;

        [UnityTearDown]
        public IEnumerator UnloadSlice()
        {
            if (!slice.IsValid() || !slice.isLoaded) yield break;
            SceneManager.SetActiveScene(SceneManager.CreateScene("AirplaneAudioCleanup"));
            yield return SceneManager.UnloadSceneAsync(slice);
        }

        [UnityTest]
        public IEnumerator RunningAirplaneSessionStartsMusicAndCanPlayCombatCue()
        {
            SceneManager.LoadScene("VerticalSlice");
            yield return null;
            slice = SceneManager.GetActiveScene();

            var session = Object.FindAnyObjectByType<VerticalSliceSession2D>();
            Assert.That(session, Is.Not.Null);
            Assert.That(session.IsRunning, Is.True);

            var composition = Object.FindAnyObjectByType<SliceAudioComposition2D>();
            Assert.That(composition, Is.Not.Null, "Airplane scene must include the audio composition.");

            for (int i = 0; i < 30 && composition.AudioManager == null; i++) yield return null;
            var manager = composition.AudioManager;
            Assert.That(manager, Is.Not.Null, "Running airplane session must instantiate its audio prefab.");

            yield return new WaitForSecondsRealtime(0.2f);
            var sources = manager.GetComponents<AudioSource>();
            bool musicAudible = false;
            foreach (var source in sources)
                if (source.loop && source.clip != null && source.isPlaying && source.volume > 0.01f)
                    musicAudible = true;
            Assert.That(musicAudible, Is.True, "At least one music layer must actually play with nonzero volume.");

            Assert.That(manager.TryPlay(AudioCue.RumiBladeSlash1, Vector3.zero), Is.True,
                "Rumi's attack cue must be available during airplane gameplay.");
            bool combatCuePlaying = false;
            foreach (var source in sources)
                if (!source.loop && source.clip != null && source.isPlaying && source.clip.name == "Cue_RumiSlash1")
                    combatCuePlaying = true;
            Assert.That(combatCuePlaying, Is.True, "Rumi's attack cue must reach a playing voice.");
        }
    }
}
