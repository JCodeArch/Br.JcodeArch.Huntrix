using HuntrX.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class SafeAreaNormalizationTests
    {
        [TestCase(0, 0, 0, 0)]
        [TestCase(100, 0, -50, 720)]
        [TestCase(0, 0, 1280, -1)]
        [TestCase(1400, 0, 50, 720)]
        [TestCase(0, -100, 1280, 50)]
        public void UnusableSafeAreaFallsBackToFullScreen(float x, float y, float width, float height)
        {
            var anchors = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(x, y, width, height), new Vector2(1280, 720));
            Assert.That(anchors.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(anchors.anchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void PartiallyOutsideSafeAreaClipsToScreenBounds()
        {
            var anchors = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(-20, 36, 1236, 720), new Vector2(1280, 720));
            Assert.That(anchors.anchorMin.x, Is.Zero);
            Assert.That(anchors.anchorMin.y, Is.EqualTo(.05f).Within(.0001f));
            Assert.That(anchors.anchorMax.x, Is.EqualTo(.95f).Within(.0001f));
            Assert.That(anchors.anchorMax.y, Is.EqualTo(1));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteMetricsNeverReachAnchors(float invalid)
        {
            var invalidRect = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(invalid, 0, 1280, 720), new Vector2(1280, 720));
            var invalidScreen = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(0, 0, 1280, 720), new Vector2(1280, invalid));
            Assert.That(invalidRect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(invalidRect.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(invalidScreen.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(invalidScreen.anchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void LandscapeFlipPreservesAsymmetricInsetsOnBothSides()
        {
            var left = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(120, 24, 2160, 1032), new Vector2(2400, 1080));
            var right = SliceSafeAreaFitter.CalculateNormalizedSafeArea(
                new Rect(40, 24, 2160, 1032), new Vector2(2400, 1080));
            Assert.That(left.anchorMin.x, Is.EqualTo(.05f).Within(.0001f));
            Assert.That(left.anchorMax.x, Is.EqualTo(.95f).Within(.0001f));
            Assert.That(right.anchorMin.x, Is.EqualTo(40f / 2400).Within(.0001f));
            Assert.That(right.anchorMax.x, Is.EqualTo(2200f / 2400).Within(.0001f));
        }
    }
}
