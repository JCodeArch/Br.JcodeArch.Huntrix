using System.Reflection;
using HuntrX.Data;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class ParryDefinitionEditModeTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [TestCase(0.1f, true)]
        [TestCase(0f, false)]
        [TestCase(-0.1f, false)]
        [TestCase(float.PositiveInfinity, false)]
        [TestCase(float.NaN, false)]
        public void ValidatesFinitePositiveWindowDuration(float duration, bool expected)
        {
            var definition = ScriptableObject.CreateInstance<ParryDefinition>();
            try
            {
                typeof(ParryDefinition).GetField("windowDuration", PrivateInstance).SetValue(definition, duration);
                Assert.That(definition.IsValid(out _), Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }
    }
}
