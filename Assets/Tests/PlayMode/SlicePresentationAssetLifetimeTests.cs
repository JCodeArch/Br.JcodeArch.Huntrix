using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SlicePresentationAssetLifetimeTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        private GameObject Host(string name)
        {
            var go = new GameObject(name);
            owned.Add(go);
            return go;
        }

        private static T Private<T>(object owner, string name) => (T)owner.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);

        private static void ChangeActor(SlicePlayerPresentation2D presenter, DamageReceiver2D actor) =>
            typeof(SlicePlayerPresentation2D).GetMethod("OnActor", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(presenter, new object[] { null, actor });

        [UnityTest]
        public IEnumerator RebindingAndDisablingReleaseActorAssetsButKeepReusableVfx()
        {
            var actor = Host("Rumi lifetime test");
            actor.SetActive(false);
            actor.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var receiver = actor.AddComponent<DamageReceiver2D>();
            receiver.Configure(100f, CombatFaction2D.HuntrX);
            actor.SetActive(true);
            var presenter = Host("Presentation lifetime test").AddComponent<SlicePlayerPresentation2D>();
            Sprite vfx = Private<Sprite>(presenter, "saberCyanSprite");
            Assert.That(vfx != null, Is.True, "Fixture requires the existing reusable slash VFX.");

            ChangeActor(presenter, receiver);
            for (int i = 0; i < 3; i++)
            {
                CharacterSpriteSet2D previous = presenter.SpriteSet;
                Assert.That(previous != null, Is.True);
                Assert.That(previous.TryGetClip("Idle", out SpriteAnimationClip2D clip), Is.True);
                Sprite previousFrame = clip.FrameAt(0f);
                Texture2D sharedTexture = previousFrame.texture;

                // Rebinding the same actor also occurs on enable/reconfigure, without a scene teardown.
                ChangeActor(presenter, receiver);
                yield return null;
                Assert.That(previous == null, Is.True, "Old runtime catalog survived an actor rebind.");
                Assert.That(previousFrame == null, Is.True, "Old runtime sprite survived an actor rebind.");
                Assert.That(sharedTexture != null, Is.True, "Resources-owned source textures must remain alive.");
                Assert.That(vfx != null, Is.True, "Actor cleanup destroyed reusable VFX.");
                Assert.That(presenter.Playback.CurrentState, Is.EqualTo("Idle"));
            }

            CharacterSpriteSet2D lastSet = presenter.SpriteSet;
            lastSet.TryGetClip("Idle", out SpriteAnimationClip2D lastClip);
            Sprite lastFrame = lastClip.FrameAt(0f);
            presenter.enabled = false;
            yield return null;
            Assert.That(lastSet == null, Is.True, "Unbind must release the final actor catalog.");
            Assert.That(lastFrame == null, Is.True, "Unbind must release the final actor sprite.");
            Assert.That(vfx != null, Is.True, "Disabling must preserve VFX for the next enable.");
        }
    }
}
