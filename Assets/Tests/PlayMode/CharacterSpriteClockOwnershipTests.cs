using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    // Observe the real callbacks: after playback.Update, then after animator.LateUpdate.
    [DefaultExecutionOrder(30000)]
    public sealed class SpriteClockCallbackProbe : MonoBehaviour
    {
        public CharacterSpritePlayback2D Playback;
        public int AttackObservations;
        public int AttackClockDrifts;
        public float LargestDrift;
        private bool hadLateAttackSample;
        private float lastLateElapsed;

        private void Update()
        {
            if (Playback == null || !hadLateAttackSample || Playback.CurrentState != "Attack" || Time.deltaTime <= 0f)
                return;
            AttackObservations++;
            float drift = Mathf.Abs(Playback.Elapsed - lastLateElapsed);
            if (drift > .000001f) AttackClockDrifts++;
            LargestDrift = Mathf.Max(LargestDrift, drift);
        }

        private void LateUpdate()
        {
            hadLateAttackSample = Playback != null && Playback.CurrentState == "Attack";
            if (hadLateAttackSample) lastLateElapsed = Playback.Elapsed;
        }
    }

    public sealed class CharacterSpriteClockOwnershipTests
    {
        private readonly List<Object> owned = new List<Object>();
        private float priorTimeScale;

        [SetUp]
        public void SetUp()
        {
            priorTimeScale = Time.timeScale;
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = priorTimeScale;
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        private T Definition<T>() where T : ScriptableObject
        {
            T value = ScriptableObject.CreateInstance<T>();
            owned.Add(value);
            return value;
        }

        private static void Set(object owner, string field, object value)
        {
            FieldInfo info = owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            info.SetValue(owner, value);
        }

        [UnityTest]
        public IEnumerator ExternalSamplingRejectsInvalidInputAndSameStateOrRebindRestoresAutomaticTime()
        {
            var texture = new Texture2D(2, 2);
            owned.Add(texture);
            Sprite frame = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero, 1f);
            owned.Add(frame);
            var catalog = Definition<CharacterSpriteSet2D>();
            Set(catalog, "characterName", "Clock API actor");
            Set(catalog, "pixelsPerUnit", 1f);
            Set(catalog, "footAnchor", Vector2.zero);
            var clip = new SpriteAnimationClip2D(new[] { frame }, new[] { 10f }, true);
            Set(catalog, "states", new[] { new SpriteAnimationState2D("Idle", clip), new SpriteAnimationState2D("Attack", clip) });
            var host = new GameObject("Clock API host");
            owned.Add(host);
            var renderer = host.AddComponent<SpriteRenderer>();
            var playback = host.AddComponent<CharacterSpritePlayback2D>();
            Assert.That(playback.TryBind(renderer, catalog), Is.True);
            Assert.That(playback.SampleStateAt("Attack", .2f), Is.True);
            Assert.That(playback.SampleStateAt("Unknown", 1f), Is.False);
            foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity })
                Assert.That(playback.SampleStateAt("Attack", invalid), Is.False);
            playback.Tick(1f);
            yield return null;
            yield return null;
            Assert.That(playback.CurrentState, Is.EqualTo("Attack"));
            Assert.That(playback.Elapsed, Is.EqualTo(.2f), "Only a valid absolute sample can advance the external clock.");

            Assert.That(playback.SetState("Attack", false), Is.True);
            Assert.That(playback.Elapsed, Is.EqualTo(.2f), "Same-state release must preserve time rather than restart.");
            yield return null;
            yield return null;
            Assert.That(playback.Elapsed, Is.GreaterThan(.2f), "Same-state SetState must restore automatic Update.");

            Assert.That(playback.SampleStateAt("Attack", .2f), Is.True);
            playback.Unbind();
            Assert.That(playback.TryBind(renderer, catalog), Is.True);
            Assert.That(playback.Elapsed, Is.Zero);
            yield return null;
            yield return null;
            Assert.That(playback.Elapsed, Is.GreaterThan(0f), "A new binding must not inherit external clock ownership.");
        }

        [UnityTest]
        public IEnumerator AttackSamplesRemainStableThroughUpdateAndIdleResumesAutomaticClock()
        {
            var texture = new Texture2D(2, 2);
            owned.Add(texture);
            Sprite frame = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero, 1f);
            owned.Add(frame);
            var catalog = Definition<CharacterSpriteSet2D>();
            Set(catalog, "characterName", "Clock actor");
            Set(catalog, "pixelsPerUnit", 1f);
            Set(catalog, "footAnchor", Vector2.zero);
            Set(catalog, "states", new[] {
                new SpriteAnimationState2D("Idle", new SpriteAnimationClip2D(new[] { frame }, new[] { 10f }, true)),
                new SpriteAnimationState2D("Attack", new SpriteAnimationClip2D(new[] { frame }, new[] { 10f }, false))
            });
            Assert.That(catalog.IsValid, Is.True);

            var attackDefinition = Definition<AttackDefinition>();
            Set(attackDefinition, "damage", 10f);
            Set(attackDefinition, "startupDuration", 10f);
            Set(attackDefinition, "activeDuration", 1f);
            Set(attackDefinition, "recoveryDuration", 1f);
            Set(attackDefinition, "hitboxSize", Vector2.one);
            Set(attackDefinition, "horizontalKnockbackImpulse", 1f);
            Assert.That(attackDefinition.IsValid(out string attackError), Is.True, attackError);
            var combo = Definition<CombatComboDefinition>();
            var steps = new[] { new ComboStep(attackDefinition, 0f, 1f, 0f), new ComboStep(attackDefinition, 0f, 1f, 0f) };
            Set(combo, "groundedSteps", steps);
            Set(combo, "aerialSteps", steps);
            Assert.That(combo.IsValid(out string comboError), Is.True, comboError);
            var art = Definition<CharacterArtDefinition>();
            Set(art, "characterName", "Clock actor");
            var rig = Definition<CharacterRigDefinition>();
            Set(rig, "character", art);

            var actor = new GameObject("Clock ownership actor");
            owned.Add(actor);
            actor.SetActive(false);
            actor.AddComponent<Rigidbody2D>().gravityScale = 0f;
            actor.AddComponent<BoxCollider2D>();
            var receiver = actor.AddComponent<DamageReceiver2D>();
            Set(receiver, "maximumHealth", 100f);
            Set(receiver, "faction", CombatFaction2D.HuntrX);
            var child = new GameObject("Hitbox");
            child.transform.SetParent(actor.transform, false);
            var trigger = child.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            var hitbox = actor.AddComponent<AttackHitbox2D>();
            Set(hitbox, "hitboxCollider", trigger);
            var attack = actor.AddComponent<AttackController2D>();
            Set(attack, "comboDefinition", combo);
            var renderer = actor.AddComponent<CharacterArtRenderer2D>();
            Set(renderer, "definition", art);
            Set(renderer, "spriteSet", catalog);
            var animator = actor.AddComponent<CharacterGameplayAnimator2D>();
            Set(animator, "renderer2D", renderer);
            Set(animator, "rig", rig);
            var probe = actor.AddComponent<SpriteClockCallbackProbe>();
            actor.SetActive(true);
            Assert.That(animator.TryConfigure(renderer, rig), Is.True);
            probe.Playback = renderer.Playback;
            Assert.That(attack.TryStartAttack(1f, true), Is.True);

            // No manual Tick, Update, LateUpdate or FixedUpdate invocation: run actual player-loop frames.
            for (int i = 0; i < 6; i++) yield return null;
            Assert.That(probe.AttackObservations, Is.GreaterThanOrEqualTo(3));
            Assert.That(probe.AttackClockDrifts, Is.Zero,
                "Automatic Update advanced an externally sampled attack before LateUpdate rewound it. Largest drift: " + probe.LargestDrift);

            // OnDisable cancels the combat state; the next animator callback selects Idle again.
            attack.enabled = false;
            yield return null;
            yield return null;
            Assert.That(renderer.Playback.CurrentState, Is.EqualTo("Idle"));
            float idleElapsed = renderer.Playback.Elapsed;
            for (int i = 0; i < 3; i++) yield return null;
            Assert.That(renderer.Playback.Elapsed, Is.GreaterThan(idleElapsed),
                "Leaving external attack sampling must restore automatic playback for Idle.");
        }
    }
}
