using HuntrX.Data;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class CombatComboDefinitionEditModeTests
    {
        private AttackDefinition validAttack;
        private CombatComboDefinition combo;

        [SetUp]
        public void SetUp()
        {
            validAttack = ScriptableObject.CreateInstance<AttackDefinition>();
            SetAttackValues(validAttack, damage: 10f, startup: 0.1f, active: 0.2f, recovery: 0.3f);
            combo = ScriptableObject.CreateInstance<CombatComboDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(combo);
            Object.DestroyImmediate(validAttack);
        }

        [Test]
        public void IsValidAcceptsGroundAndAirSequencesWithAtLeastTwoSteps()
        {
            SetSequence("groundedSteps", CreateStep(validAttack), CreateStep(validAttack));
            SetSequence("aerialSteps", CreateStep(validAttack), CreateStep(validAttack));

            AssertValid();
        }

        [TestCase("groundedSteps")]
        [TestCase("aerialSteps")]
        public void IsValidRejectsEmptySequence(string sequenceField)
        {
            SetSequence("groundedSteps", CreateStep(validAttack), CreateStep(validAttack));
            SetSequence("aerialSteps", CreateStep(validAttack), CreateStep(validAttack));
            SetSequence(sequenceField);

            AssertInvalid();
        }

        [TestCase("groundedSteps")]
        [TestCase("aerialSteps")]
        public void IsValidRejectsSingleStepSequence(string sequenceField)
        {
            SetSequence("groundedSteps", CreateStep(validAttack), CreateStep(validAttack));
            SetSequence("aerialSteps", CreateStep(validAttack), CreateStep(validAttack));
            SetSequence(sequenceField, CreateStep(validAttack));

            AssertInvalid();
        }

        [Test]
        public void IsValidRejectsNullAttackReference()
        {
            SetBothSequences(CreateStep(validAttack), CreateStep(null));

            AssertInvalid();
        }

        [Test]
        public void IsValidRejectsReferencedAttackWithInvalidDefinition()
        {
            var invalidAttack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                SetBothSequences(CreateStep(invalidAttack), CreateStep(validAttack));

                AssertInvalid();
            }
            finally
            {
                Object.DestroyImmediate(invalidAttack);
            }
        }

        [Test]
        public void IsValidRejectsFiniteAttackDurationsWhoseTotalOverflows()
        {
            SetAttackValues(validAttack, damage: 10f, startup: float.MaxValue, active: float.MaxValue, recovery: 0f);
            SetBothSequences(CreateStep(validAttack), CreateStep(validAttack));

            AssertInvalid();
        }

        [TestCase(float.NaN, 0.2f)]
        [TestCase(float.PositiveInfinity, 0.2f)]
        [TestCase(-0.1f, 0.2f)]
        [TestCase(0.2f, float.NaN)]
        [TestCase(0.2f, float.NegativeInfinity)]
        [TestCase(0.2f, float.PositiveInfinity)]
        [TestCase(0.2f, 0.2f)]
        [TestCase(0.3f, 0.2f)]
        [TestCase(0.2f, 0.61f)]
        public void IsValidRejectsMalformedNonFiniteOrOutOfRangeNonFinalWindow(float start, float end)
        {
            SetBothSequences(CreateStep(validAttack, start, end), CreateStep(validAttack));

            AssertInvalid();
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-0.01f)]
        public void IsValidRejectsNonFiniteOrNegativeHitStopDuration(float hitStopDuration)
        {
            SetBothSequences(CreateStep(validAttack, hitStopDuration: hitStopDuration), CreateStep(validAttack));

            AssertInvalid();
        }

        [Test]
        public void IsValidIgnoresMalformedFinalStepWindow()
        {
            SetBothSequences(
                CreateStep(validAttack),
                CreateStep(validAttack, float.NaN, float.PositiveInfinity));

            AssertValid();
        }

        private ComboStep CreateStep(AttackDefinition attack, float chainWindowStart = 0.1f,
            float chainWindowEnd = 0.5f, float hitStopDuration = 0.05f)
        {
            return new ComboStep(attack, chainWindowStart, chainWindowEnd, hitStopDuration);
        }

        private void SetBothSequences(params ComboStep[] steps)
        {
            SetSequence("groundedSteps", steps);
            SetSequence("aerialSteps", steps);
        }

        private void SetSequence(string fieldName, params ComboStep[] steps)
        {
            var serialized = new SerializedObject(combo);
            var property = serialized.FindProperty(fieldName);
            Assert.That(property, Is.Not.Null, "CombatComboDefinition must serialize a field named '" + fieldName + "'.");
            property.arraySize = steps.Length;
            for (var i = 0; i < steps.Length; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                SetSerializedStep(element, steps[i]);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedStep(SerializedProperty element, ComboStep step)
        {
            RequireRelative(element, "attack").objectReferenceValue = step.Attack;
            RequireRelative(element, "chainWindowStart").floatValue = step.ChainWindowStart;
            RequireRelative(element, "chainWindowEnd").floatValue = step.ChainWindowEnd;
            RequireRelative(element, "hitStopDuration").floatValue = step.HitStopDuration;
        }

        private static SerializedProperty RequireRelative(SerializedProperty element, string fieldName)
        {
            var property = element.FindPropertyRelative(fieldName);
            Assert.That(property, Is.Not.Null, "ComboStep must serialize a field named '" + fieldName + "'.");
            return property;
        }

        private static void SetAttackValues(AttackDefinition attack, float damage, float startup,
            float active, float recovery)
        {
            var serialized = new SerializedObject(attack);
            serialized.FindProperty("damage").floatValue = damage;
            serialized.FindProperty("startupDuration").floatValue = startup;
            serialized.FindProperty("activeDuration").floatValue = active;
            serialized.FindProperty("recoveryDuration").floatValue = recovery;
            serialized.FindProperty("hitboxSize").vector2Value = Vector2.one;
            serialized.FindProperty("horizontalKnockbackImpulse").floatValue = 1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssertValid()
        {
            Assert.That(combo.IsValid(out var error), Is.True, error);
            Assert.That(error, Is.Empty);
        }

        private void AssertInvalid()
        {
            Assert.That(combo.IsValid(out var error), Is.False);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
        }
    }
}
