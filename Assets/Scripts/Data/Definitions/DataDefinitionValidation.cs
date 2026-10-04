using System.Collections.Generic;

namespace HuntrX.Data
{
    public enum DataDefinitionValidationIssueCode
    {
        NullDefinition,
        MissingId,
        DuplicateId
    }

    public sealed class DataDefinitionValidationIssue
    {
        public DataDefinitionValidationIssue(DataDefinitionValidationIssueCode code, string message, GameDataDefinition definition)
        {
            Code = code;
            Message = message;
            Definition = definition;
        }

        public DataDefinitionValidationIssueCode Code { get; }
        public string Message { get; }
        public GameDataDefinition Definition { get; }
    }

    public static class DataDefinitionValidation
    {
        public static IReadOnlyList<DataDefinitionValidationIssue> Validate(IEnumerable<GameDataDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new System.ArgumentNullException(nameof(definitions));
            }

            var issues = new List<DataDefinitionValidationIssue>();
            var definitionsById = new Dictionary<string, GameDataDefinition>(System.StringComparer.Ordinal);

            foreach (var definition in definitions)
            {
                if (definition == null)
                {
                    issues.Add(new DataDefinitionValidationIssue(
                        DataDefinitionValidationIssueCode.NullDefinition,
                        "A data definition reference is null.",
                        null));
                    continue;
                }

                var id = definition.Id;
                if (string.IsNullOrWhiteSpace(id))
                {
                    issues.Add(new DataDefinitionValidationIssue(
                        DataDefinitionValidationIssueCode.MissingId,
                        $"{definition.GetType().Name} has an empty ID.",
                        definition));
                    continue;
                }

                if (definitionsById.ContainsKey(id))
                {
                    issues.Add(new DataDefinitionValidationIssue(
                        DataDefinitionValidationIssueCode.DuplicateId,
                        $"{definition.GetType().Name} repeats ID '{id}'.",
                        definition));
                    continue;
                }

                definitionsById.Add(id, definition);
            }

            return issues;
        }
    }
}
