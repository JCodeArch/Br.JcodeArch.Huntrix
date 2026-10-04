using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class MiraProtectionDefinitionTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void ValidatesFinitePositiveDurationAndRadius()
        {
            var definition = ScriptableObject.CreateInstance<MiraProtectionDefinition>();
            try
            {
                SetValue(definition, "durationSeconds", 4f);
                SetValue(definition, "radius", 2.5f);
                Assert.That(definition.IsValid(out var error), Is.True, error);
                Assert.That(definition.DurationSeconds, Is.EqualTo(4f));
                Assert.That(definition.Radius, Is.EqualTo(2.5f));
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void RejectsInvalidDuration(float value)
        {
            var definition = ScriptableObject.CreateInstance<MiraProtectionDefinition>();
            try
            {
                SetValue(definition, "durationSeconds", value);
                SetValue(definition, "radius", 1f);
                Assert.That(definition.IsValid(out var error), Is.False);
                Assert.That(error, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void RejectsInvalidRadius(float value)
        {
            var definition = ScriptableObject.CreateInstance<MiraProtectionDefinition>();
            try
            {
                SetValue(definition, "durationSeconds", 1f);
                SetValue(definition, "radius", value);
                Assert.That(definition.IsValid(out var error), Is.False);
                Assert.That(error, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(definition); }
        }

        private static void SetValue(MiraProtectionDefinition definition, string fieldName, float value)
        {
            typeof(MiraProtectionDefinition).GetField(fieldName, PrivateInstance).SetValue(definition, value);
        }
    }
}
