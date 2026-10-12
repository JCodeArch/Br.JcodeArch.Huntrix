using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Special;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class SpecialSequenceIntegrityTests
    {
        private readonly List<Object> owned = new List<Object>();
        private float previousScale;
        [SetUp] public void SetUp() { previousScale = Time.timeScale; Time.timeScale = 1f; }
        [TearDown] public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
            owned.Clear(); Time.timeScale = previousScale;
        }
        private GameObject Spawn(string path, float x = 0f)
        {
            GameObject prefab = UnityEngine.Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            GameObject result = Object.Instantiate(prefab, new Vector3(x, 0f, 0f), Quaternion.identity);
            owned.Add(result);
            Rigidbody2D body = result.GetComponent<Rigidbody2D>();
            if (body != null) { body.gravityScale = 0f; body.constraints = RigidbodyConstraints2D.FreezeAll; }
            return result;
        }
        private HuntrXSpecialController2D Configure(DamageReceiver2D owner, DamageReceiver2D[] targets)
        {
            var special = Spawn("Special/HuntrXSpecial_Prototype").GetComponent<HuntrXSpecialController2D>();
            var definition = Object.Instantiate(special.Definition); owned.Add(definition);
            typeof(HuntrXSpecialDefinition).GetField("stageDuration", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(definition, .02f);
            Assert.That(special.TryConfigure(definition, owner, targets, out string error), Is.True, error);
            Assert.That(special.TryAddCharge(definition.MaximumCharge), Is.True);
            return special;
        }
        [Test] public void SelectionUsesEligibleCountAndNearestOrderWithoutHurtingAlliesOrFans()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var roster = new List<DamageReceiver2D>();
            for (int x = 5; x >= 1; x--) roster.Add(Spawn("Enemies/GroundDemon_Prototype", x).GetComponent<DamageReceiver2D>());
            var ally = Spawn("Characters/Mira_Prototype", .25f).GetComponent<DamageReceiver2D>(); roster.Add(ally);
            var fan = Spawn("Enemies/GroundDemon_Prototype", .5f).GetComponent<DamageReceiver2D>(); fan.gameObject.AddComponent<FanActor2D>(); roster.Add(fan);
            var special = Configure(owner, roster.ToArray());
            float allyHealth = ally.CurrentHealth, fanHealth = fan.CurrentHealth;
            Assert.That(special.TryActivate(), Is.True);
            Assert.That(special.SelectedCount, Is.EqualTo(2)); // ceil(5 valid demons * .4), not seven roster entries.
            Assert.That(special.GetSelectedTarget(0), Is.SameAs(roster[4]));
            Assert.That(special.GetSelectedTarget(1), Is.SameAs(roster[3]));
            Assert.That(ally.CurrentHealth, Is.EqualTo(allyHealth)); Assert.That(fan.CurrentHealth, Is.EqualTo(fanHealth));
            for (int i = 0; i < 3; i++) Assert.That(roster[i].CurrentHealth, Is.EqualTo(roster[i].MaximumHealth));
        }
        [UnityTest] public IEnumerator FourStagesCommitDistinctAttacksThenCompleteOnce()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var target = Spawn("Enemies/GroundDemon_Prototype", 1f).GetComponent<DamageReceiver2D>();
            var special = Configure(owner, new[] { target });
            var stages = new List<SpecialStage2D>(); var attacks = new List<AttackDefinition>();
            int deaths = 0, completions = 0; bool cancelled = true;
            special.StageChanged += stages.Add; target.DamageReceived += hit => attacks.Add(hit.AttackDefinition);
            target.Died += actor => deaths++; special.Completed += value => { completions++; cancelled = value; };
            Assert.That(special.TryActivate(), Is.True);
            float deadline = Time.realtimeSinceStartup + 5f;
            while (special.IsActive && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(special.IsActive, Is.False, "Sequence failed to finish within the bounded runtime deadline.");
            CollectionAssert.AreEqual(new[] { SpecialStage2D.Rumi, SpecialStage2D.Mira, SpecialStage2D.Zoey, SpecialStage2D.Combined }, stages);
            CollectionAssert.AreEqual(new[] { special.Definition.RumiAttack, special.Definition.MiraAttack, special.Definition.ZoeyAttack, special.Definition.CombinedAttack }, attacks);
            Assert.That(target.CurrentHealth, Is.Zero); Assert.That(deaths, Is.EqualTo(1));
            Assert.That(completions, Is.EqualTo(1)); Assert.That(cancelled, Is.False);
            Assert.That(special.Charge, Is.Zero, "Kills during an active Special must not refund its reserved charge.");
            yield return null; Assert.That(completions, Is.EqualTo(1));
        }
        [Test] public void StageCallbackInvalidatingOwnerCancelsBeforeAnyDamage()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var target = Spawn("Enemies/GroundDemon_Prototype", 1f).GetComponent<DamageReceiver2D>();
            var special = Configure(owner, new[] { target });
            int completions = 0; bool cancelled = false;
            special.StageChanged += stage => owner.gameObject.SetActive(false);
            special.Completed += value => { completions++; cancelled = value; };
            Assert.That(special.TryActivate(), Is.False);
            Assert.That(target.CurrentHealth, Is.EqualTo(target.MaximumHealth));
            Assert.That(special.Charge, Is.Zero); Assert.That(special.SelectedCount, Is.Zero);
            Assert.That(completions, Is.EqualTo(1)); Assert.That(cancelled, Is.True);
        }
        [Test] public void CommittedHitInvalidatingOwnerStopsLaterTargetsWithoutUndoingFirstHit()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var first = Spawn("Enemies/GroundDemon_Prototype", 1f).GetComponent<DamageReceiver2D>();
            var second = Spawn("Enemies/GroundDemon_Prototype", 2f).GetComponent<DamageReceiver2D>();
            var special = Configure(owner, new[] { first, second,
                Spawn("Enemies/GroundDemon_Prototype", 3f).GetComponent<DamageReceiver2D>() });
            first.DamageReceived += hit => owner.gameObject.SetActive(false);
            int completions = 0; special.Completed += cancelled => completions++;
            Assert.That(special.TryActivate(), Is.False);
            Assert.That(first.CurrentHealth, Is.EqualTo(first.MaximumHealth - special.Definition.RumiAttack.Damage));
            Assert.That(second.CurrentHealth, Is.EqualTo(second.MaximumHealth));
            Assert.That(completions, Is.EqualTo(1)); Assert.That(special.IsActive, Is.False);
        }
        [Test] public void ChargeReservationRejectsNestedActivationAndDisableCancelsExactlyOnce()
        {
            var owner = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var target = Spawn("Enemies/GroundDemon_Prototype", 1f).GetComponent<DamageReceiver2D>();
            var special = Configure(owner, new[] { target });
            bool nestedActivate = true, nestedCharge = true, observedReservation = false;
            special.ChargeChanged += charge => {
                if (charge != 0f) return;
                observedReservation = special.IsActive && special.SelectedCount == 1;
                nestedActivate = special.TryActivate(); nestedCharge = special.TryAddCharge(special.Definition.MaximumCharge);
            };
            int cancelled = 0; special.Completed += value => { if (value) cancelled++; };
            Assert.That(special.TryActivate(), Is.True);
            Assert.That(observedReservation, Is.True); Assert.That(nestedActivate, Is.False); Assert.That(nestedCharge, Is.False);
            special.enabled = false; special.Cancel();
            Assert.That(cancelled, Is.EqualTo(1)); Assert.That(special.Charge, Is.Zero); Assert.That(special.SelectedCount, Is.Zero);
            Assert.That(special.TryActivate(), Is.False);
        }
    }
}
