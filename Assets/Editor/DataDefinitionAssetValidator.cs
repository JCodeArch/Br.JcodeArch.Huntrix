using System.Collections.Generic;
using System.Linq;
using HuntrX.Data;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Editor
{
    public static class DataDefinitionAssetValidator
    {
        private const string DataFolder = "Assets/Data";

        [MenuItem("HUNTR/X/Data/Validate Definitions")]
        public static void ValidateFromMenu()
        {
            var issues = ValidateProjectAssets();
            if (issues.Count == 0)
            {
                Debug.Log("HUNTR/X data definition validation passed.");
                return;
            }

            foreach (var issue in issues)
            {
                var path = issue.Definition == null ? DataFolder : AssetDatabase.GetAssetPath(issue.Definition);
                Debug.LogError($"{issue.Code}: {issue.Message} Asset: {path}", issue.Definition);
            }
        }

        public static IReadOnlyList<DataDefinitionValidationIssue> ValidateProjectAssets()
        {
            if (!AssetDatabase.IsValidFolder(DataFolder))
            {
                return System.Array.Empty<DataDefinitionValidationIssue>();
            }

            var definitions = AssetDatabase.FindAssets(string.Empty, new[] { DataFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadMainAssetAtPath)
                .OfType<GameDataDefinition>()
                .ToArray();

            return DataDefinitionValidation.Validate(definitions);
        }
    }
}
