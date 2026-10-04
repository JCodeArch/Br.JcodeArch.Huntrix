using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class AttackDataAndHealthPlayModeTests
    {
        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ValidAttackDefinitionExposesConfiguredValues()
        {
            AttackDefinition definition = CreateValidAttackDefinition();

            Assert.That(definition.IsValid(out string error), Is.True, error);
            Assert.That(error, Is.Empty);
            Assert.That(definition.Damage, Is.EqualTo(12f));
            Assert.That(definition.StartupDuration, Is.EqualTo(0.08f));
            Assert.That(definition.ActiveDuration, Is.EqualTo(0.12f));
            Assert.That(definition.RecoveryDuration, Is.EqualTo(0.2f));
            Assert.That(definition.HitboxSize, Is.EqualTo(new Vector2(1.4f, 0.9f)));
            Assert.That(definition.HitboxOffset, Is.EqualTo(new Vector2(0.7f, 0.1f)));
            Assert.That(definition.HorizontalKnockbackImpulse, Is.EqualTo(5f));
            Assert.That(definition.UpwardKnockbackImpulse, Is.EqualTo(2f));

            Object.DestroyImmediate(definition);
        }

        [TestCase("damage", 0f)]
        [TestCase("damage", -1f)]
        [TestCase("damage", float.NaN)]
        [TestCase("damage", float.PositiveInfinity)]
        [TestCase("damage", float.NegativeInfinity)]
        [TestCase("startupDuration", -0.01f)]
        [TestCase("startupDuration", float.NaN)]
        [TestCase("startupDuration", float.PositiveInfinity)]
        [TestCase("startupDuration", float.NegativeInfinity)]
        [TestCase("activeDuration", 0f)]
        [TestCase("activeDuration", -0.01f)]
        [TestCase("activeDuration", float.NaN)]
        [TestCase("activeDuration", float.PositiveInfinity)]
        [TestCase("activeDuration", float.NegativeInfinity)]
        [TestCase("recoveryDuration", -0.01f)]
        [TestCase("recoveryDuration", float.NaN)]
        [TestCase("recoveryDuration", float.PositiveInfinity)]
        [TestCase("recoveryDuration", float.NegativeInfinity)]
        [TestCase("horizontalKnockbackImpulse", 0f)]
        [TestCase("horizontalKnockbackImpulse", -1f)]
        [TestCase("horizontalKnockbackImpulse", float.NaN)]
        [TestCase("horizontalKnockbackImpulse", float.PositiveInfinity)]
        [TestCase("horizontalKnockbackImpulse", float.NegativeInfinity)]
        [TestCase("upwardKnockbackImpulse", -0.01f)]
        [TestCase("upwardKnockbackImpulse", float.NaN)]
        [TestCase("upwardKnockbackImpulse", float.PositiveInfinity)]
        [TestCase("upwardKnockbackImpulse", float.NegativeInfinity)]
        public void InvalidAttackScalarIsRejected(string fieldName, float value)
        {
            AttackDefinition definition = CreateValidAttackDefinition();
            SetPrivateField(definition, fieldName, value);

            Assert.That(definition.IsValid(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            Object.DestroyImmediate(definition);
        }

        [TestCase("startupDuration", 0f)]
        [TestCase("recoveryDuration", 0f)]
        [TestCase("upwardKnockbackImpulse", 0f)]
        public void ConfiguredNonnegativeAttackScalarAcceptsZero(string fieldName, float value)
        {
            AttackDefinition definition = CreateValidAttackDefinition();
            SetPrivateField(definition, fieldName, value);

            Assert.That(definition.IsValid(out string error), Is.True, error);
            Assert.That(error, Is.Empty);

            Object.DestroyImmediate(definition);
        }

        [TestCase("damage", 0f)]
        [TestCase("damage", -1f)]
        [TestCase("activeDuration", 0f)]
        [TestCase("activeDuration", -1f)]
        [TestCase("horizontalKnockbackImpulse", 0f)]
        [TestCase("horizontalKnockbackImpulse", -1f)]
        public void ConfiguredPositiveAttackScalarRejectsZeroAndNegative(string fieldName, float value)
        {
            AttackDefinition definition = CreateValidAttackDefinition();
            SetPrivateField(definition, fieldName, value);

            Assert.That(definition.IsValid(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            Object.DestroyImmediate(definition);
        }

        [TestCase("hitboxSize", 0f, 1f)]
        [TestCase("hitboxSize", 1f, 0f)]
        [TestCase("hitboxSize", -1f, 1f)]
        [TestCase("hitboxSize", 1f, -1f)]
        [TestCase("hitboxSize", float.NaN, 1f)]
        [TestCase("hitboxSize", 1f, float.NaN)]
        [TestCase("hitboxSize", float.PositiveInfinity, 1f)]
        [TestCase("hitboxSize", 1f, float.PositiveInfinity)]
        [TestCase("hitboxSize", float.NegativeInfinity, 1f)]
        [TestCase("hitboxSize", 1f, float.NegativeInfinity)]
        [TestCase("hitboxOffset", float.NaN, 0f)]
        [TestCase("hitboxOffset", 0f, float.NaN)]
        [TestCase("hitboxOffset", float.PositiveInfinity, 0f)]
        [TestCase("hitboxOffset", 0f, float.PositiveInfinity)]
        [TestCase("hitboxOffset", float.NegativeInfinity, 0f)]
        [TestCase("hitboxOffset", 0f, float.NegativeInfinity)]
        public void InvalidAttackVectorComponentIsRejected(string fieldName, float x, float y)
        {
            AttackDefinition definition = CreateValidAttackDefinition();
            SetPrivateField(definition, fieldName, new Vector2(x, y));

            Assert.That(definition.IsValid(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FiniteAttackOffsetsHaveNoUnspecifiedRangeLimit()
        {
            AttackDefinition definition = CreateValidAttackDefinition();
            SetPrivateField(definition, "hitboxOffset", new Vector2(float.MaxValue, -float.MaxValue));

            Assert.That(definition.IsValid(out string error), Is.True, error);
            Assert.That(definition.HitboxOffset, Is.EqualTo(new Vector2(float.MaxValue, -float.MaxValue)));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ReceiverInitializesHealthToConfiguredMaximum()
        {
            GameObject target = new GameObject("health receiver test");
            target.SetActive(false);
            DamageReceiver2D receiver = target.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 80f);
            target.SetActive(true);

            Assert.That(receiver.IsConfigurationValid, Is.True);
            Assert.That(receiver.Faction, Is.EqualTo(CombatFaction2D.HuntrX));
            Assert.That(receiver.MaximumHealth, Is.EqualTo(80f));
            Assert.That(receiver.CurrentHealth, Is.EqualTo(80f));
            Assert.That(receiver.IsAlive, Is.True);

            Object.DestroyImmediate(target);
        }

        [TestCase(CombatFaction2D.HuntrX)]
        [TestCase(CombatFaction2D.Demon)]
        public void ReceiverExposesConfiguredFaction(CombatFaction2D faction)
        {
            GameObject target = new GameObject("faction receiver test");
            target.SetActive(false);
            DamageReceiver2D receiver = target.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 80f);
            SetPrivateField(receiver, "faction", faction);
            target.SetActive(true);

            Assert.That(receiver.Faction, Is.EqualTo(faction));

            Object.DestroyImmediate(target);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InvalidMaximumHealthIsRejectedOnce(float maximumHealth)
        {
            GameObject target = new GameObject("invalid health receiver test");
            target.SetActive(false);
            DamageReceiver2D receiver = target.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", maximumHealth);
            LogAssert.Expect(LogType.Error,
                "DamageReceiver2D requires finite positive maximum health.");

            target.SetActive(true);

            Assert.That(receiver.IsConfigurationValid, Is.False);
            Assert.That(receiver.MaximumHealth, Is.EqualTo(maximumHealth));
            Assert.That(receiver.CurrentHealth, Is.Zero);
            Assert.That(receiver.IsAlive, Is.False);
            _ = receiver.IsConfigurationValid;
            _ = receiver.IsAlive;

            Object.DestroyImmediate(target);
        }

        [Test]
        public void InvalidFactionIsRejectedOnce()
        {
            GameObject target = new GameObject("invalid faction receiver test");
            target.SetActive(false);
            DamageReceiver2D receiver = target.AddComponent<DamageReceiver2D>();
            SetPrivateField(receiver, "maximumHealth", 80f);
            SetPrivateField(receiver, "faction", (CombatFaction2D)42);
            LogAssert.Expect(LogType.Error,
                "DamageReceiver2D requires a valid CombatFaction2D value.");

            target.SetActive(true);

            Assert.That(receiver.IsConfigurationValid, Is.False);
            Assert.That(receiver.CurrentHealth, Is.Zero);
            Assert.That(receiver.IsAlive, Is.False);
            _ = receiver.IsConfigurationValid;
            _ = receiver.IsAlive;

            Object.DestroyImmediate(target);
        }

        private static AttackDefinition CreateValidAttackDefinition()
        {
            AttackDefinition definition = ScriptableObject.CreateInstance<AttackDefinition>();
            SetPrivateField(definition, "damage", 12f);
            SetPrivateField(definition, "startupDuration", 0.08f);
            SetPrivateField(definition, "activeDuration", 0.12f);
            SetPrivateField(definition, "recoveryDuration", 0.2f);
            SetPrivateField(definition, "hitboxSize", new Vector2(1.4f, 0.9f));
            SetPrivateField(definition, "hitboxOffset", new Vector2(0.7f, 0.1f));
            SetPrivateField(definition, "horizontalKnockbackImpulse", 5f);
            SetPrivateField(definition, "upwardKnockbackImpulse", 2f);
            return definition;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            FieldInfo field = instance.GetType().GetField(fieldName, PrivateInstance);
            Assert.That(field, Is.Not.Null, $"Expected serialized field '{fieldName}'.");
            field.SetValue(instance, value);
        }
    }
}
