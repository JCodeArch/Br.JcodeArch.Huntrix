using System;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Editor;
using HuntrX.Gameplay;
using HuntrX.Gameplay.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class CharacterDefinitionTests
    {
        private const string MovementDefinitionTypeName = "HuntrX.Data.CharacterMovementDefinition, HuntrX.Runtime";

        [Test]
        public void RumiPrototypeAssetsAreValidAndUseSharedControllers()
        {
            var issues = DataDefinitionAssetValidator.ValidateProjectAssets();
            Assert.That(issues, Is.Empty, string.Join("; ", System.Linq.Enumerable.Select(issues, issue => issue.Message)));

            CharacterDefinition definition = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(
                "Assets/Data/Characters/Rumi/Rumi.asset");
            Assert.That(definition, Is.Not.Null);
            Assert.That(definition.IsValid(out string definitionError), Is.True, definitionError);
            Assert.That(definition.CombatCombo.GroundedSteps.Count, Is.EqualTo(3));
            Assert.That(definition.CombatCombo.AerialSteps.Count, Is.EqualTo(2));

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Resources/Characters/Rumi_Prototype.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<CharacterDefinitionApplier2D>().Definition, Is.SameAs(definition));
            Assert.That(prefab.GetComponent<AttackController2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<ParryController2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponentInChildren<BoxCollider2D>(true).isTrigger, Is.True);
        }

        [Test]
        public void CharacterMovementDefinitionAcceptsValidPrototypeSettings()
        {
            Type definitionType = Type.GetType(MovementDefinitionTypeName);
            Assert.That(definitionType, Is.Not.Null, "CharacterMovementDefinition must exist in HuntrX.Runtime.");
            var movement = (ScriptableObject)ScriptableObject.CreateInstance(definitionType);

            try
            {
                SetMovementValues(movement);

                AssertDefinitionValid(movement);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(movement);
            }
        }

        [TestCase("maxHorizontalSpeed", float.NaN)]
        [TestCase("acceleration", float.PositiveInfinity)]
        [TestCase("wallJumpHorizontalSpeed", -1f)]
        [TestCase("jumpCutMultiplier", 1f)]
        [TestCase("airDashDuration", 0f)]
        public void CharacterMovementDefinitionRejectsInvalidSettings(string fieldName, float value)
        {
            Type definitionType = Type.GetType(MovementDefinitionTypeName);
            Assert.That(definitionType, Is.Not.Null, "CharacterMovementDefinition must exist in HuntrX.Runtime.");
            var movement = (ScriptableObject)ScriptableObject.CreateInstance(definitionType);

            try
            {
                SetMovementValues(movement);
                var serialized = new SerializedObject(movement);
                SetFloat(serialized, fieldName, value);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                AssertDefinitionInvalid(movement);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(movement);
            }
        }

        [Test]
        public void CharacterDefinitionRequiresNameAndCompleteValidStaticKit()
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            var movementType = Type.GetType(MovementDefinitionTypeName);
            Assert.That(movementType, Is.Not.Null, "CharacterMovementDefinition must exist in HuntrX.Runtime.");
            var movement = (ScriptableObject)ScriptableObject.CreateInstance(movementType);
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            var combo = ScriptableObject.CreateInstance<CombatComboDefinition>();
            var parry = ScriptableObject.CreateInstance<ParryDefinition>();

            try
            {
                SetMovementValues(movement);
                SetAttackValues(attack);
                SetComboSequences(combo, attack);
                SetSerializedFloat(parry, "windowDuration", 0.12f);
                SetCharacterReferences(character, "Rumi", movement, combo, parry);

                AssertDefinitionValid(character);

                SetCharacterReferences(character, " ", movement, combo, parry);
                AssertDefinitionInvalid(character);

                SetCharacterReferences(character, "Rumi", null, combo, parry);
                AssertDefinitionInvalid(character);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(character);
                UnityEngine.Object.DestroyImmediate(movement);
                UnityEngine.Object.DestroyImmediate(attack);
                UnityEngine.Object.DestroyImmediate(combo);
                UnityEngine.Object.DestroyImmediate(parry);
            }
        }

        private static void SetMovementValues(ScriptableObject movement)
        {
            var serialized = new SerializedObject(movement);
            SetFloat(serialized, "maxHorizontalSpeed", 4f);
            SetFloat(serialized, "acceleration", 8f);
            SetFloat(serialized, "deceleration", 12f);
            SetFloat(serialized, "jumpVelocity", 8f);
            SetFloat(serialized, "wallJumpHorizontalSpeed", 6f);
            SetFloat(serialized, "gravityScale", 2f);
            SetFloat(serialized, "coyoteTime", 0.1f);
            SetFloat(serialized, "jumpBufferTime", 0.1f);
            SetFloat(serialized, "jumpCutMultiplier", 0.5f);
            SetFloat(serialized, "dashSpeed", 10f);
            SetFloat(serialized, "dashDuration", 0.2f);
            SetFloat(serialized, "airDashSpeed", 10f);
            SetFloat(serialized, "airDashDuration", 0.15f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetAttackValues(AttackDefinition attack)
        {
            var serialized = new SerializedObject(attack);
            SetFloat(serialized, "damage", 10f);
            SetFloat(serialized, "startupDuration", 0.1f);
            SetFloat(serialized, "activeDuration", 0.2f);
            SetFloat(serialized, "recoveryDuration", 0.3f);
            serialized.FindProperty("hitboxSize").vector2Value = Vector2.one;
            serialized.FindProperty("hitboxOffset").vector2Value = Vector2.zero;
            SetFloat(serialized, "horizontalKnockbackImpulse", 1f);
            SetFloat(serialized, "upwardKnockbackImpulse", 0f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetComboSequences(CombatComboDefinition combo, AttackDefinition attack)
        {
            var serialized = new SerializedObject(combo);
            SetSequence(serialized.FindProperty("groundedSteps"), attack);
            SetSequence(serialized.FindProperty("aerialSteps"), attack);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSequence(SerializedProperty sequence, AttackDefinition attack)
        {
            sequence.arraySize = 2;
            for (var index = 0; index < sequence.arraySize; index++)
            {
                var step = sequence.GetArrayElementAtIndex(index);
                step.FindPropertyRelative("attack").objectReferenceValue = attack;
                step.FindPropertyRelative("chainWindowStart").floatValue = 0.1f;
                step.FindPropertyRelative("chainWindowEnd").floatValue = 0.5f;
                step.FindPropertyRelative("hitStopDuration").floatValue = 0f;
            }
        }

        private static void SetCharacterReferences(CharacterDefinition character, string displayName,
            ScriptableObject movement, CombatComboDefinition combo, ParryDefinition parry)
        {
            var serialized = new SerializedObject(character);
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("movement").objectReferenceValue = movement;
            serialized.FindProperty("combatCombo").objectReferenceValue = combo;
            serialized.FindProperty("parry").objectReferenceValue = parry;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedFloat(ScriptableObject target, string fieldName, float value)
        {
            var serialized = new SerializedObject(target);
            SetFloat(serialized, fieldName, value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(SerializedObject serialized, string fieldName, float value)
        {
            var property = serialized.FindProperty(fieldName);
            Assert.That(property, Is.Not.Null, "Serialized field '" + fieldName + "' must exist.");
            property.floatValue = value;
        }

        private static void AssertDefinitionValid(ScriptableObject definition)
        {
            MethodInfo method = definition.GetType().GetMethod("IsValid", new[] { typeof(string).MakeByRefType() });
            Assert.That(method, Is.Not.Null, definition.GetType().Name + " must expose IsValid(out string error).");
            object[] arguments = { string.Empty };
            Assert.That(method.Invoke(definition, arguments), Is.EqualTo(true), arguments[0]?.ToString());
            Assert.That(arguments[0]?.ToString(), Is.Empty);
        }

        private static void AssertDefinitionInvalid(ScriptableObject definition)
        {
            MethodInfo method = definition.GetType().GetMethod("IsValid", new[] { typeof(string).MakeByRefType() });
            Assert.That(method, Is.Not.Null, definition.GetType().Name + " must expose IsValid(out string error).");
            object[] arguments = { string.Empty };
            Assert.That(method.Invoke(definition, arguments), Is.EqualTo(false));
            Assert.That(arguments[0]?.ToString(), Is.Not.Null.And.Not.Empty);
        }
    }
}
