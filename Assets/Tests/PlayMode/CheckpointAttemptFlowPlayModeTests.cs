using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class CheckpointAttemptFlowPlayModeTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly System.Collections.Generic.List<Object> createdObjects =
            new System.Collections.Generic.List<Object>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.Destroy(createdObjects[i]);
                }
            }
            createdObjects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator LethalDamagePublishesOneRespawnRequestForLatestCheckpoint()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Transform start = CreateTransform("stage start", new Vector3(-4f, 0f, 0f));
            CheckpointAnchor2D checkpoint = CreateAnchor("active checkpoint", "cp-2", new Vector3(7f, 3f, 0f));
            Assert.That(flow.ConfigureStart("stage-start", start), Is.True);
            Vector2 velocityAtDeath = Vector2.zero;
            actor.Died += _ => velocityAtDeath = actor.GetComponent<Rigidbody2D>().linearVelocity;
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.ActivateCheckpoint(checkpoint), Is.True);

            RespawnRequest received = default;
            int requests = 0;
            flow.RespawnRequested += request =>
            {
                received = request;
                requests++;
            };

            AttackDefinition lethal = CreateLethalDefinition();
            AttackController2D controller = CreateAttacker(attacker, lethal);
            Physics2D.SyncTransforms();

            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(actor.IsAlive, Is.False);
            Assert.That(velocityAtDeath.sqrMagnitude, Is.GreaterThan(0f), "The death event must run after accepted knockback.");
            Assert.That(requests, Is.EqualTo(1));
            Assert.That(received.AttemptNumber, Is.EqualTo(1));
            Assert.That(received.CheckpointId, Is.EqualTo("cp-2"));
            Assert.That(received.Position, Is.EqualTo(new Vector3(7f, 3f, 0f)));
            Assert.That(received.Rotation, Is.EqualTo(checkpoint.transform.rotation));
        }

        [UnityTest]
        public IEnumerator DisablingFlowUnsubscribesFromActorDeath()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Transform start = CreateTransform("stage start", Vector3.zero);
            Assert.That(flow.ConfigureStart("stage-start", start), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            int requests = 0;
            flow.RespawnRequested += _ => requests++;

            flow.enabled = false;
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && actor.IsAlive; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(actor.IsAlive, Is.False);
            Assert.That(requests, Is.Zero);
        }

        private AttackDefinition CreateLethalDefinition()
        {
            AttackDefinition lethal = ScriptableObject.CreateInstance<AttackDefinition>();
            createdObjects.Add(lethal);
            SetPrivateField(lethal, "damage", 100f);
            SetPrivateField(lethal, "startupDuration", 0f);
            SetPrivateField(lethal, "activeDuration", 0.2f);
            SetPrivateField(lethal, "recoveryDuration", 0f);
            SetPrivateField(lethal, "hitboxSize", new Vector2(1.4f, 0.9f));
            SetPrivateField(lethal, "hitboxOffset", new Vector2(0.8f, 0.1f));
            SetPrivateField(lethal, "horizontalKnockbackImpulse", 5f);
            SetPrivateField(lethal, "upwardKnockbackImpulse", 2f);
            return lethal;
        }

        private DamageReceiver2D CreateReceiver(string name, CombatFaction2D faction, Vector2 position)
        {
            var root = new GameObject(name);
            root.SetActive(false);
            root.transform.position = position;
            createdObjects.Add(root);
            root.AddComponent<Rigidbody2D>().gravityScale = 0f;
            root.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
            root.AddComponent<Hurtbox2D>();
            DamageReceiver2D receiver = root.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 100f);
            SetPrivateField(receiver, "faction", faction);
            root.SetActive(true);
            return receiver;
        }

        private Transform CreateTransform(string name, Vector3 position)
        {
            var point = new GameObject(name);
            point.transform.position = position;
            createdObjects.Add(point);
            return point.transform;
        }

        private CheckpointAnchor2D CreateAnchor(string name, string id, Vector3 position)
        {
            Transform point = CreateTransform(name, position);
            CheckpointAnchor2D anchor = point.gameObject.AddComponent<CheckpointAnchor2D>();
            SetPrivateField(anchor, "checkpointId", id);
            return anchor;
        }

        private AttackController2D CreateAttacker(DamageReceiver2D receiver, AttackDefinition definition)
        {
            var root = receiver.gameObject;
            root.SetActive(false);
            var hitboxObject = new GameObject("attack hitbox");
            hitboxObject.transform.SetParent(root.transform, false);
            BoxCollider2D hitboxCollider = hitboxObject.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.size = new Vector2(1.4f, 0.9f);
            AttackHitbox2D hitbox = root.AddComponent<AttackHitbox2D>();
            SetPrivateField(hitbox, "hitboxCollider", hitboxCollider);
            createdObjects.Add(hitboxObject);
            CombatComboDefinition combo = ScriptableObject.CreateInstance<CombatComboDefinition>();
            createdObjects.Add(combo);
            ComboStep link = new ComboStep(definition, 0f, 0.2f, 0f);
            ComboStep final = new ComboStep(definition, 0f, 0f, 0f);
            SetPrivateField(combo, "groundedSteps", new[] { link, final });
            SetPrivateField(combo, "aerialSteps", new[] { link, final });
            AttackController2D controller = root.AddComponent<AttackController2D>();
            SetPrivateField(controller, "comboDefinition", combo);
            root.SetActive(true);
            return controller;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected serialized field '{fieldName}'.");
            field.SetValue(instance, value);
        }

[UnityTest]
        public IEnumerator RebindingActorKeepsAttemptAndIgnoresOldActorsDeath()
        {
            DamageReceiver2D oldActor = CreateReceiver("old actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D newActor = CreateReceiver("new actor", CombatFaction2D.HuntrX, new Vector2(4f, 0f));
            DamageReceiver2D oldAttacker = CreateReceiver("old attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            DamageReceiver2D newAttacker = CreateReceiver("new attacker", CombatFaction2D.Demon, new Vector2(3.2f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(oldActor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.BindActor(newActor), Is.True);
            Assert.That(flow.AttemptNumber, Is.EqualTo(1));
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            AttackController2D oldController = CreateAttacker(oldAttacker, CreateLethalDefinition());
            AttackController2D newController = CreateAttacker(newAttacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();

            Assert.That(oldController.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && oldActor.IsAlive; i++) yield return new WaitForFixedUpdate();
            Assert.That(oldActor.IsAlive, Is.False);
            Assert.That(requests, Is.Zero);
            Assert.That(newController.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
            Assert.That(flow.AttemptNumber, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator InvalidActorBindingPreservesCurrentBinding()
        {
            DamageReceiver2D actor = CreateReceiver("valid actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            var invalidRoot = new GameObject("invalid actor");
            invalidRoot.SetActive(false);
            invalidRoot.transform.position = new Vector2(6f, 0f);
            createdObjects.Add(invalidRoot);
            invalidRoot.AddComponent<Rigidbody2D>().gravityScale = 0f;
            invalidRoot.AddComponent<BoxCollider2D>().size = new Vector2(0.7f, 0.7f);
            invalidRoot.AddComponent<Hurtbox2D>();
            DamageReceiver2D invalidActor = invalidRoot.AddComponent<DamageReceiver2D>();
            SetPrivateField(invalidActor, "maximumHealth", 0f);
            SetPrivateField(invalidActor, "faction", CombatFaction2D.HuntrX);
            LogAssert.Expect(LogType.Error, new Regex("finite positive maximum health"));
            invalidRoot.SetActive(true);
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.BindActor(invalidActor), Is.False);
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisableEnableRebindStartsNextMonotonicAttemptWithoutDuplicateHandlers()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            flow.enabled = false;
            flow.enabled = true;
            SetPrivateField(actor, "<CurrentHealth>k__BackingField", actor.MaximumHealth);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.AttemptNumber, Is.EqualTo(2));
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DestroyedActiveCheckpointFallsBackToStageStart()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Transform start = CreateTransform("stage start", new Vector3(-5f, 2f, 0f));
            CheckpointAnchor2D checkpoint = CreateAnchor("active checkpoint", "cp-destroyed", new Vector3(7f, 3f, 0f));
            Assert.That(flow.ConfigureStart("stage-start", start), Is.True);
            Vector2 velocityAtDeath = Vector2.zero;
            actor.Died += _ => velocityAtDeath = actor.GetComponent<Rigidbody2D>().linearVelocity;
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.ActivateCheckpoint(checkpoint), Is.True);
            Object.Destroy(checkpoint.gameObject);
            int requests = 0;
            RespawnRequest received = default;
            flow.RespawnRequested += request => { received = request; requests++; };
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
            Assert.That(received.CheckpointId, Is.EqualTo("stage-start"));
            Assert.That(received.Position, Is.EqualTo(start.position));
        }
        [UnityTest]
        public IEnumerator ThrowingRespawnSubscriberDoesNotBlockLaterSubscribers()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            int laterSubscribers = 0;
            flow.RespawnRequested += _ => throw new System.InvalidOperationException("respawn subscriber failed");
            flow.RespawnRequested += _ => laterSubscribers++;
            LogAssert.Expect(LogType.Exception, new Regex("respawn subscriber failed"));
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && laterSubscribers == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(laterSubscribers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ThrowingDeathSubscriberDoesNotBlockLaterSubscribersOrRespawnFlow()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            int respawns = 0;
            int laterDeathSubscribers = 0;
            flow.RespawnRequested += _ => respawns++;
            actor.Died += _ => throw new System.InvalidOperationException("death subscriber failed");
            actor.Died += _ => laterDeathSubscribers++;
            LogAssert.Expect(LogType.Exception, new Regex("death subscriber failed"));
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && laterDeathSubscribers == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(respawns, Is.EqualTo(1));
            Assert.That(laterDeathSubscribers, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DeadActorBindingIsRejectedAndCurrentActorRemainsBound()
        {
            DamageReceiver2D actor = CreateReceiver("current actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            DamageReceiver2D deadCandidate = CreateReceiver("dead candidate", CombatFaction2D.HuntrX, new Vector2(5f, 0f));
            SetPrivateField(deadCandidate, "<CurrentHealth>k__BackingField", 0f);
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.BindActor(deadCandidate), Is.False);
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator SubsequentAttemptUsesPreviouslyActivatedCheckpoint()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D firstAttacker = CreateReceiver("first attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            DamageReceiver2D secondAttacker = CreateReceiver("second attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Transform start = CreateTransform("stage start", new Vector3(-5f, 0f, 0f));
            CheckpointAnchor2D checkpoint = CreateAnchor("checkpoint", "cp-continued", new Vector3(8f, 4f, 0f));
            Assert.That(flow.ConfigureStart("stage-start", start), Is.True);
            Vector2 velocityAtDeath = Vector2.zero;
            actor.Died += _ => velocityAtDeath = actor.GetComponent<Rigidbody2D>().linearVelocity;
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.ActivateCheckpoint(checkpoint), Is.True);
            var requests = new System.Collections.Generic.List<RespawnRequest>();
            flow.RespawnRequested += requests.Add;
            AttackController2D firstController = CreateAttacker(firstAttacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(firstController.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests.Count == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests.Count, Is.EqualTo(1));
            SetPrivateField(actor, "<CurrentHealth>k__BackingField", actor.MaximumHealth);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.AttemptNumber, Is.EqualTo(2));
            AttackController2D secondController = CreateAttacker(secondAttacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(secondController.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests.Count < 2; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests.Count, Is.EqualTo(2));
            Assert.That(requests[1].AttemptNumber, Is.EqualTo(2));
            Assert.That(requests[1].CheckpointId, Is.EqualTo("cp-continued"));
            Assert.That(requests[1].Position, Is.EqualTo(checkpoint.transform.position));
        }

        [UnityTest]
        public IEnumerator NoValidDestinationPublishesNoRespawnRequest()
        {
            DamageReceiver2D actor = CreateReceiver("controlled actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Transform start = CreateTransform("stage start", Vector3.zero);
            CheckpointAnchor2D checkpoint = CreateAnchor("checkpoint", "cp-invalidated", new Vector3(5f, 1f, 0f));
            Assert.That(flow.ConfigureStart("stage-start", start), Is.True);
            Vector2 velocityAtDeath = Vector2.zero;
            actor.Died += _ => velocityAtDeath = actor.GetComponent<Rigidbody2D>().linearVelocity;
            Assert.That(flow.BindActor(actor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            Assert.That(flow.ActivateCheckpoint(checkpoint), Is.True);
            Object.Destroy(start.gameObject);
            Object.Destroy(checkpoint.gameObject);
            yield return null;
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            LogAssert.Expect(LogType.Error, new Regex("could not resolve an active checkpoint or stage start"));
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && actor.IsAlive; i++) yield return new WaitForFixedUpdate();
            Assert.That(actor.IsAlive, Is.False);
            Assert.That(requests, Is.Zero);
        }

        [UnityTest]
        public IEnumerator DestroyedActorCanBeReboundDuringAttempt()
        {
            DamageReceiver2D oldActor = CreateReceiver("old actor", CombatFaction2D.HuntrX, Vector2.zero);
            CheckpointAttemptFlow2D flow = new GameObject("attempt flow").AddComponent<CheckpointAttemptFlow2D>();
            createdObjects.Add(flow.gameObject);
            Assert.That(flow.ConfigureStart("stage-start", CreateTransform("stage start", Vector3.zero)), Is.True);
            Assert.That(flow.BindActor(oldActor), Is.True);
            Assert.That(flow.StartAttempt(), Is.True);
            int requests = 0;
            flow.RespawnRequested += _ => requests++;
            Object.Destroy(oldActor.gameObject);
            yield return null;
            Assert.That(requests, Is.Zero);
            DamageReceiver2D newActor = CreateReceiver("replacement actor", CombatFaction2D.HuntrX, Vector2.zero);
            DamageReceiver2D attacker = CreateReceiver("attacker", CombatFaction2D.Demon, new Vector2(-0.8f, 0f));
            Assert.That(flow.BindActor(newActor), Is.True);
            AttackController2D controller = CreateAttacker(attacker, CreateLethalDefinition());
            Physics2D.SyncTransforms();
            Assert.That(controller.TryStartAttack(1f), Is.True);
            for (int i = 0; i < 8 && requests == 0; i++) yield return new WaitForFixedUpdate();
            Assert.That(requests, Is.EqualTo(1));
        }

    }
}
