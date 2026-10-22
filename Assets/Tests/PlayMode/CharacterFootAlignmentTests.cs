using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.VerticalSlice;
using HuntrX.Presentation.Art;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CharacterFootAlignmentTests
    {
        private readonly List<Object> owned = new List<Object>();
        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private static void Set(object owner, string field, object value) => owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
        private static T Get<T>(object owner, string field) => (T)owner.GetType()
            .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private static void Late(object owner) => owner.GetType()
            .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);
        [TearDown] public void Cleanup()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FootPivotMatchesSolidBodyBottomAtBindAndAfterMovement(bool sliceHost)
        {
            var actor = Own(new GameObject("Rumi foot alignment"));
            actor.SetActive(false);
            actor.transform.position = new Vector3(4f, 7f, 0f);
            var body = actor.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            var collider = actor.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(.7f, 1.8f);
            collider.offset = new Vector2(.15f, .2f);
            var receiver = actor.AddComponent<DamageReceiver2D>();
            receiver.Configure(100f, CombatFaction2D.HuntrX);
            actor.SetActive(true);
            Physics2D.SyncTransforms();

            SpriteRenderer sprite;
            object lateOwner;
            if (sliceHost)
            {
                var presenter = Own(new GameObject("Slice foot host")).AddComponent<SlicePlayerPresentation2D>();
                Set(presenter, "followCamera", Own(new GameObject("Foot test camera")).AddComponent<Camera>());
                typeof(SlicePlayerPresentation2D).GetMethod("OnActor", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(presenter, new object[] { null, receiver });
                sprite = Get<SpriteRenderer>(presenter, "mainRenderer");
                lateOwner = presenter;
            }
            else
            {
                var art = Own(ScriptableObject.CreateInstance<CharacterArtDefinition>());
                Set(art, "characterName", "Rumi"); Set(art, "height", 1.8f); Set(art, "width", .55f);
                var rig = Own(ScriptableObject.CreateInstance<CharacterRigDefinition>());
                Set(rig, "character", art); Set(rig, "idleCycleSeconds", .6f); Set(rig, "runCycleSeconds", .4f);
                Set(rig, "hurtSeconds", .2f); Set(rig, "specialSeconds", .5f);
                var renderer = actor.AddComponent<CharacterArtRenderer2D>();
                Assert.That(renderer.TryConfigure(art), Is.True);
                var animator = actor.AddComponent<CharacterGameplayAnimator2D>();
                Assert.That(animator.TryConfigure(renderer, rig), Is.True);
                sprite = renderer.SpriteRenderer;
                lateOwner = animator;
            }
            Assert.That(sprite, Is.Not.Null);
            Assert.That(sprite.sprite, Is.Not.Null);
            Assert.That(sprite.sprite.pivot.y, Is.EqualTo(0f).Within(.001f), "Fixture must use imported foot-pivot art.");
            Assert.That(sprite.transform.position.y, Is.EqualTo(collider.bounds.min.y).Within(.001f), "Bind must place the foot pivot on the physical body bottom.");
            foreach (float direction in new[] { -1f, 1f })
            {
                actor.transform.position += new Vector3(direction * 2f, .4f, 0f);
                body.linearVelocity = new Vector2(direction * 3f, 0f);
                Physics2D.SyncTransforms();
                Late(lateOwner);
                Assert.That(sprite.transform.position.y, Is.EqualTo(collider.bounds.min.y).Within(.001f), "LateUpdate/facing must preserve foot alignment after movement.");
            }
        }
    }
}
