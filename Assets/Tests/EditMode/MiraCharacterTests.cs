using System;
using HuntrX.Data;
using HuntrX.Gameplay;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Protection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace HuntrX.Tests.EditMode
{
    public sealed class MiraCharacterTests
    {
        [Test] public void MiraProfileHasOwnIdentityAndValidProvisionalSharedBasicKit()
        {
            var mira = AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/Data/Characters/Mira/Mira.asset");
            var rumi = AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/Data/Characters/Rumi/Rumi.asset");
            Assert.That(mira, Is.Not.Null, "Mira authored profile is required.");
            Assert.That(mira.IsValid(out string error), Is.True, error);
            Assert.That(Guid.TryParseExact(mira.Id, "N", out _), Is.True);
            Assert.That(mira.Id, Is.Not.EqualTo(rumi.Id));
            Assert.That(mira.DisplayName, Is.EqualTo("Mira"));
            Assert.That(mira.Movement, Is.SameAs(rumi.Movement));
            Assert.That(mira.CombatCombo, Is.SameAs(rumi.CombatCombo));
            Assert.That(mira.Parry, Is.SameAs(rumi.Parry));
        }
        [Test] public void MiraResourcesPrefabComposesSeparateValidFieldAndIndependentChildTrigger()
        {
            var prefab = Resources.Load<GameObject>("Characters/Mira_Prototype");
            Assert.That(prefab, Is.Not.Null, "Actual Mira Resources prefab is required.");
            var profile = prefab.GetComponent<CharacterDefinitionApplier2D>().Definition;
            Assert.That(profile, Is.SameAs(AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/Data/Characters/Mira/Mira.asset")));
            Assert.That(prefab.GetComponent<AttackController2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<ParryController2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<Hurtbox2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<DamageReceiver2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<DamageProtection2D>(), Is.Not.Null);
            var field = prefab.GetComponent<MiraProtectionField2D>(); Assert.That(field, Is.Not.Null);
            Assert.That(field.Definition, Is.SameAs(AssetDatabase.LoadAssetAtPath<MiraProtectionDefinition>("Assets/Data/Characters/Mira/Mira_Protection_Prototype.asset")));
            Assert.That(field.Definition.IsValid(out string error), Is.True, error);
            Assert.That(Guid.TryParseExact(field.Definition.Id, "N", out _), Is.True);
            Assert.That(field.Definition.Id, Is.Not.EqualTo(profile.Id));
            var circle = new SerializedObject(field).FindProperty("fieldCollider").objectReferenceValue as CircleCollider2D;
            Assert.That(circle, Is.Not.Null);
            Assert.That(circle.transform.parent, Is.SameAs(prefab.transform));
            Assert.That(circle.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(circle.offset, Is.EqualTo(Vector2.zero));
            Assert.That(circle.isTrigger, Is.True); Assert.That(circle.enabled, Is.False);
            Assert.That(circle.attachedRigidbody, Is.SameAs(prefab.GetComponent<Rigidbody2D>()));
            Assert.That(prefab.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Dynamic));
            Assert.That(prefab.GetComponentInChildren<BoxCollider2D>(), Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Rigidbody2D>(true).Length, Is.EqualTo(1));
            Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab), Is.Zero);
        }
    }
}
