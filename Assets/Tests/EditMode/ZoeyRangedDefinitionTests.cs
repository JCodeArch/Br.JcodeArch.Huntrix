using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class ZoeyRangedDefinitionTests
    {
        private ZoeyRangedDefinition definition;
        private AttackDefinition attack;
        [SetUp] public void SetUp()
        {
            attack = ScriptableObject.CreateInstance<AttackDefinition>();
            Set(attack, "damage", 10f); Set(attack, "activeDuration", .1f);
            Set(attack, "hitboxSize", Vector2.one); Set(attack, "horizontalKnockbackImpulse", 1f);
            definition = ScriptableObject.CreateInstance<ZoeyRangedDefinition>();
            Set(definition, "attack", attack); Set(definition, "range", 8f);
            Set(definition, "cooldown", .25f); Set(definition, "muzzleOffset", Vector2.zero);
        }
        [TearDown] public void TearDown() { Object.DestroyImmediate(definition); Object.DestroyImmediate(attack); }
        [Test] public void AcceptsValidAuthoredAttackAndRange()
        {
            Assert.That(definition.IsValid(out var error), Is.True, error);
            Assert.That(definition.Attack, Is.SameAs(attack)); Assert.That(definition.Range, Is.EqualTo(8f));
        }
        [TestCase(0f)] [TestCase(-1f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void RejectsInvalidRange(float value)
        {
            Set(definition, "range", value); Assert.That(definition.IsValid(out var error), Is.False); Assert.That(error, Is.Not.Empty);
        }
        [TestCase(0f)] [TestCase(-1f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void RejectsInvalidCooldown(float value)
        {
            Set(definition, "cooldown", value); Assert.That(definition.IsValid(out var error), Is.False); Assert.That(error, Is.Not.Empty);
        }
        [Test] public void RejectsMissingOrInvalidAttack()
        {
            Set(definition, "attack", null); Assert.That(definition.IsValid(out _), Is.False);
            Set(definition, "attack", attack); Set(attack, "damage", 0f); Assert.That(definition.IsValid(out _), Is.False);
        }
        [Test] public void RejectsNonfiniteMuzzleOffset()
        {
            Set(definition, "muzzleOffset", new Vector2(float.NaN, 0f)); Assert.That(definition.IsValid(out _), Is.False);
            Set(definition, "muzzleOffset", new Vector2(0f, float.PositiveInfinity)); Assert.That(definition.IsValid(out _), Is.False);
        }
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
