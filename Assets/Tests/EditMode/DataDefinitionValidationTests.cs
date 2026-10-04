using System.Linq;
using HuntrX.Data;
using HuntrX.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class DataDefinitionValidationTests
    {
        [Test]
        public void ValidationRejectsBlankId()
        {
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                SetId(definition, "  ");
                var issues = DataDefinitionValidation.Validate(new[] { definition });

                Assert.That(issues.Any(issue => issue.Code == DataDefinitionValidationIssueCode.MissingId), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void ValidationFindsDuplicateIdsAcrossDifferentTypes()
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            try
            {
                SetId(character, "shared-id");
                SetId(enemy, "shared-id");

                var issues = DataDefinitionValidation.Validate(new GameDataDefinition[] { character, enemy });

                Assert.That(issues.Count(issue => issue.Code == DataDefinitionValidationIssueCode.DuplicateId), Is.EqualTo(1));
                Assert.That(issues.Single(issue => issue.Code == DataDefinitionValidationIssueCode.DuplicateId).Definition, Is.SameAs(enemy));
            }
            finally
            {
                Object.DestroyImmediate(character);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void ValidationReportsNullDefinitions()
        {
            var issues = DataDefinitionValidation.Validate(new GameDataDefinition[] { null });

            Assert.That(issues.Count(issue => issue.Code == DataDefinitionValidationIssueCode.NullDefinition), Is.EqualTo(1));
        }

        [Test]
        public void ValidationAcceptsDifferentNonEmptyIds()
        {
            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            try
            {
                SetId(character, "character-id");
                SetId(enemy, "enemy-id");

                Assert.That(DataDefinitionValidation.Validate(new GameDataDefinition[] { character, enemy }), Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(character);
                Object.DestroyImmediate(enemy);
            }
        }

        [Test]
        public void ProjectValidatorReportsDuplicateIdsWithAssetPaths()
        {
            const string testFolder = "Assets/Data/EditModeTestAssets";
            const string firstPath = testFolder + "/Character.asset";
            const string secondPath = testFolder + "/Enemy.asset";
            AssetDatabase.CreateFolder("Assets/Data", "EditModeTestAssets");

            var character = ScriptableObject.CreateInstance<CharacterDefinition>();
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            SetId(character, "shared-project-id");
            SetId(enemy, "shared-project-id");
            AssetDatabase.CreateAsset(character, firstPath);
            AssetDatabase.CreateAsset(enemy, secondPath);
            AssetDatabase.SaveAssets();

            try
            {
                var duplicate = DataDefinitionAssetValidator.ValidateProjectAssets()
                    .Single(issue => issue.Code == DataDefinitionValidationIssueCode.DuplicateId);

                Assert.That(AssetDatabase.GetAssetPath(duplicate.Definition), Is.EqualTo(secondPath));
            }
            finally
            {
                AssetDatabase.DeleteAsset(testFolder);
            }
        }

        [Test]
        public void ValidationRejectsANullCollectionWithArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => DataDefinitionValidation.Validate(null));
        }
        private static void SetId(GameDataDefinition definition, string id)
        {
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
