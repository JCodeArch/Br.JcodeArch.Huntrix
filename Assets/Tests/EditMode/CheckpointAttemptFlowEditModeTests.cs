using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.EditMode
{
    public sealed class CheckpointAttemptFlowEditModeTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void AnchorUsesConfiguredRespawnPointAndReturnsStableIdentity()
        {
            var anchorObject = new GameObject("checkpoint");
            var pointObject = new GameObject("checkpoint respawn point");
            try
            {
                anchorObject.transform.position = new Vector3(1f, 2f, 0f);
                anchorObject.transform.rotation = Quaternion.Euler(0f, 0f, 20f);
                pointObject.transform.position = new Vector3(8f, 3f, 0f);
                pointObject.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                CheckpointAnchor2D anchor = anchorObject.AddComponent<CheckpointAnchor2D>();
                SetPrivateField(anchor, "checkpointId", "chapter-1-bridge");
                SetPrivateField(anchor, "respawnPoint", pointObject.transform);

                Assert.That(anchor.TryGetCheckpoint(out string id, out Vector3 position, out Quaternion rotation), Is.True);
                Assert.That(id, Is.EqualTo("chapter-1-bridge"));
                Assert.That(position, Is.EqualTo(pointObject.transform.position));
                Assert.That(rotation, Is.EqualTo(pointObject.transform.rotation));
            }
            finally
            {
                Object.DestroyImmediate(pointObject);
                Object.DestroyImmediate(anchorObject);
            }
        }

        [TestCase("")]
        [TestCase("   ")]
        public void AnchorRejectsMissingIdentity(string id)
        {
            var anchorObject = new GameObject("invalid checkpoint");
            try
            {
                CheckpointAnchor2D anchor = anchorObject.AddComponent<CheckpointAnchor2D>();
                SetPrivateField(anchor, "checkpointId", id);

                Assert.That(anchor.TryGetCheckpoint(out _, out _, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(anchorObject);
            }
        }

        [Test]
        public void FlowStartsAtConfiguredDestinationAndLatestActivationWins()
        {
            var startObject = new GameObject("stage start");
            var firstObject = new GameObject("first checkpoint");
            var latestObject = new GameObject("latest checkpoint");
            var flowObject = new GameObject("attempt flow");
            var actorObject = new GameObject("controlled actor");
            try
            {
                actorObject.SetActive(false);
                actorObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                DamageReceiver2D actor = actorObject.AddComponent<DamageReceiver2D>();
                SetPrivateField(actor, "<IsConfigurationValid>k__BackingField", true);
                SetPrivateField(actor, "<CurrentHealth>k__BackingField", 100f);
                startObject.transform.position = new Vector3(-2f, 1f, 0f);
                firstObject.transform.position = new Vector3(4f, 0f, 0f);
                latestObject.transform.position = new Vector3(12f, 5f, 0f);
                CheckpointAttemptFlow2D flow = flowObject.AddComponent<CheckpointAttemptFlow2D>();
                CheckpointAnchor2D first = CreateAnchor(firstObject, "checkpoint-a");
                CheckpointAnchor2D latest = CreateAnchor(latestObject, "checkpoint-b");

                Assert.That(flow.ConfigureStart("chapter-start", startObject.transform), Is.True);
                Assert.That(flow.BindActor(actor), Is.True);
                Assert.That(flow.StartAttempt(), Is.True);
                Assert.That(flow.ActiveCheckpointId, Is.EqualTo("chapter-start"));
                Assert.That(flow.ActiveCheckpointPosition, Is.EqualTo(startObject.transform.position));
                var activations = new List<string>();
                flow.CheckpointActivated += activations.Add;

                Assert.That(flow.ActivateCheckpoint(first), Is.True);
                Assert.That(flow.ActivateCheckpoint(latest), Is.True);
                Assert.That(flow.ActiveCheckpointId, Is.EqualTo("checkpoint-b"));
                Assert.That(flow.ActiveCheckpointPosition, Is.EqualTo(latestObject.transform.position));
                Assert.That(activations, Is.EqualTo(new[] { "checkpoint-a", "checkpoint-b" }));
            }
            finally
            {
                Object.DestroyImmediate(flowObject);
                Object.DestroyImmediate(actorObject);
                Object.DestroyImmediate(latestObject);
                Object.DestroyImmediate(firstObject);
                Object.DestroyImmediate(startObject);
            }
        }

        private static CheckpointAnchor2D CreateAnchor(GameObject target, string id)
        {
            CheckpointAnchor2D anchor = target.AddComponent<CheckpointAnchor2D>();
            SetPrivateField(anchor, "checkpointId", id);
            return anchor;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected serialized field '{fieldName}'.");
            field.SetValue(instance, value);
        }

[Test]
        public void InvalidCheckpointActivationPreservesLastValidSelection()
        {
            var startObject = new GameObject("stage start");
            var validObject = new GameObject("valid checkpoint");
            var invalidObject = new GameObject("invalid checkpoint");
            var flowObject = new GameObject("attempt flow");
            var actorObject = new GameObject("controlled actor");
            try
            {
                actorObject.SetActive(false);
                actorObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                DamageReceiver2D actor = actorObject.AddComponent<DamageReceiver2D>();
                SetPrivateField(actor, "<IsConfigurationValid>k__BackingField", true);
                SetPrivateField(actor, "<CurrentHealth>k__BackingField", 100f);
                CheckpointAttemptFlow2D flow = flowObject.AddComponent<CheckpointAttemptFlow2D>();
                CheckpointAnchor2D valid = CreateAnchor(validObject, "valid-id");
                CheckpointAnchor2D invalid = CreateAnchor(invalidObject, " ");
                Assert.That(flow.ConfigureStart("start", startObject.transform), Is.True);
                Assert.That(flow.BindActor(actor), Is.True);
                Assert.That(flow.StartAttempt(), Is.True);
                Assert.That(flow.ActivateCheckpoint(valid), Is.True);
                Assert.That(flow.ActivateCheckpoint(invalid), Is.False);
                Assert.That(flow.ActiveCheckpointId, Is.EqualTo("valid-id"));
            }
            finally
            {
                Object.DestroyImmediate(flowObject);
                Object.DestroyImmediate(actorObject);
                Object.DestroyImmediate(invalidObject);
                Object.DestroyImmediate(validObject);
                Object.DestroyImmediate(startObject);
            }
        }
        [Test]
        public void ThrowingCheckpointObserverDoesNotBlockLaterObservers()
        {
            var startObject = new GameObject("stage start");
            var checkpointObject = new GameObject("checkpoint");
            var flowObject = new GameObject("attempt flow");
            var actorObject = new GameObject("controlled actor");
            try
            {
                actorObject.SetActive(false);
                actorObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                DamageReceiver2D actor = actorObject.AddComponent<DamageReceiver2D>();
                SetPrivateField(actor, "<IsConfigurationValid>k__BackingField", true);
                SetPrivateField(actor, "<CurrentHealth>k__BackingField", 100f);
                CheckpointAttemptFlow2D flow = flowObject.AddComponent<CheckpointAttemptFlow2D>();
                CheckpointAnchor2D checkpoint = CreateAnchor(checkpointObject, "checkpoint-id");
                Assert.That(flow.ConfigureStart("start", startObject.transform), Is.True);
                Assert.That(flow.BindActor(actor), Is.True);
                Assert.That(flow.StartAttempt(), Is.True);
                int laterObservers = 0;
                flow.CheckpointActivated += _ => throw new System.InvalidOperationException("checkpoint subscriber failed");
                flow.CheckpointActivated += _ => laterObservers++;
                LogAssert.Expect(LogType.Exception, new Regex("checkpoint subscriber failed"));
                Assert.That(flow.ActivateCheckpoint(checkpoint), Is.True);
                Assert.That(laterObservers, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(flowObject);
                Object.DestroyImmediate(actorObject);
                Object.DestroyImmediate(checkpointObject);
                Object.DestroyImmediate(startObject);
            }
        }

        [Test]
        public void AnchorRejectsDisabledAnchorAndInactiveAlternateDestination()
        {
            var anchorObject = new GameObject("checkpoint");
            var pointObject = new GameObject("inactive destination");
            try
            {
                CheckpointAnchor2D anchor = anchorObject.AddComponent<CheckpointAnchor2D>();
                SetPrivateField(anchor, "checkpointId", "checkpoint-id");
                anchor.enabled = false;
                Assert.That(anchor.TryGetCheckpoint(out _, out _, out _), Is.False);
                anchor.enabled = true;
                pointObject.SetActive(false);
                SetPrivateField(anchor, "respawnPoint", pointObject.transform);
                Assert.That(anchor.TryGetCheckpoint(out _, out _, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(pointObject);
                Object.DestroyImmediate(anchorObject);
            }
        }

    }
}
