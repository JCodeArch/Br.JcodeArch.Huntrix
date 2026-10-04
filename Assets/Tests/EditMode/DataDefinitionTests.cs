using System;
using System.Linq;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class DataDefinitionTests
    {
        private static readonly Type[] DefinitionTypes =
        {
            typeof(CharacterDefinition),
            typeof(EnemyDefinition),
            typeof(StageDefinition),
            typeof(AttackDefinition),
            typeof(FoodDefinition),
            typeof(ProfileDefinition)
        };

        [Test]
        public void DefinitionsAreDistinctScriptableObjectsWithUniqueAssetMenuEntries()
        {
            var attributes = DefinitionTypes
                .Select(type => Attribute.GetCustomAttribute(type, typeof(CreateAssetMenuAttribute)))
                .Cast<CreateAssetMenuAttribute>()
                .ToArray();

            Assert.That(attributes.Select(attribute => attribute.menuName).Distinct().Count(), Is.EqualTo(DefinitionTypes.Length));

            foreach (var type in DefinitionTypes)
            {
                Assert.That(typeof(GameDataDefinition).IsAssignableFrom(type), Is.True);
                Assert.That(typeof(ScriptableObject).IsAssignableFrom(type), Is.True);
                Assert.That(attributes.All(attribute => !string.IsNullOrWhiteSpace(attribute.fileName)), Is.True);

                var definition = (GameDataDefinition)ScriptableObject.CreateInstance(type);
                try
                {
                    Assert.That(definition.Id, Is.Not.Null.And.Not.Empty);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(definition);
                }
            }
        }

        [Test]
        public void StableIdSurvivesAssetSaveAndReload()
        {
            const string testFolder = "Assets/Data/EditModeTestAssets";
            const string assetPath = testFolder + "/Character.asset";
            AssetDatabase.CreateFolder("Assets/Data", "EditModeTestAssets");

            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var expectedId = definition.Id;
            AssetDatabase.CreateAsset(definition, assetPath);
            AssetDatabase.SaveAssets();

            try
            {
                var loaded = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(assetPath);
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded.Id, Is.EqualTo(expectedId));
            }
            finally
            {
                AssetDatabase.DeleteAsset(testFolder);
            }
        }

        [Test]
        public void ProfileDefinitionDoesNotDeclareSerializedUserState()
        {
            var serializedFields = typeof(ProfileDefinition)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                .ToArray();

            Assert.That(serializedFields, Is.Empty);
        }
    }
}
