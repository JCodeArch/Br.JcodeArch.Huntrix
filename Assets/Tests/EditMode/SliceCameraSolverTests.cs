using HuntrX.Data;
using HuntrX.Gameplay.VerticalSlice;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HuntrX.Tests.EditMode
{
    public sealed class SliceCameraSolverTests
    {
        private static readonly Rect Stage = Rect.MinMaxRect(-1f, -1.5f, 49f, 12f);

        [Test] public void FocusInsideStageIsKept()
        {
            Vector2 c = SliceCameraSolver2D.Solve(Stage, new Vector2(20f, 4f), 3.7f, 16f / 9f);
            Assert.That(c.x, Is.EqualTo(20f).Within(1e-4f));
            Assert.That(c.y, Is.EqualTo(4f).Within(1e-4f));
        }

        [TestCase(16f, 9f)]
        [TestCase(20f, 9f)]
        public void ViewportNeverLeavesStageAtAnyEdge(float w, float h)
        {
            float aspect = w / h, half = 3.7f;
            foreach (var focus in new[] { new Vector2(-50f, -50f), new Vector2(500f, 500f), new Vector2(-50f, 500f), new Vector2(500f, -50f) })
            {
                Vector2 c = SliceCameraSolver2D.Solve(Stage, focus, half, aspect);
                Assert.That(c.x - half * aspect, Is.GreaterThanOrEqualTo(Stage.xMin - 1e-4f));
                Assert.That(c.x + half * aspect, Is.LessThanOrEqualTo(Stage.xMax + 1e-4f));
                Assert.That(c.y - half, Is.GreaterThanOrEqualTo(Stage.yMin - 1e-4f));
                Assert.That(c.y + half, Is.LessThanOrEqualTo(Stage.yMax + 1e-4f));
            }
        }

        [Test] public void StageNarrowerThanViewportCentersHorizontallyButStillFollowsVertically()
        {
            var narrow = Rect.MinMaxRect(0f, 0f, 4f, 30f);
            Vector2 c = SliceCameraSolver2D.Solve(narrow, new Vector2(100f, 15f), 3.7f, 16f / 9f);
            Assert.That(c.x, Is.EqualTo(2f).Within(1e-4f));
            Assert.That(c.y, Is.EqualTo(15f).Within(1e-4f));
        }

        [Test] public void StageShorterThanViewportCentersVerticallyButStillFollowsHorizontally()
        {
            var flat = Rect.MinMaxRect(0f, 0f, 100f, 3f);
            Vector2 c = SliceCameraSolver2D.Solve(flat, new Vector2(40f, 50f), 3.7f, 16f / 9f);
            Assert.That(c.x, Is.EqualTo(40f).Within(1e-4f));
            Assert.That(c.y, Is.EqualTo(1.5f).Within(1e-4f));
        }

        [Test] public void StageSmallerOnBothAxesCentersOnStage()
        {
            var tiny = Rect.MinMaxRect(10f, 20f, 12f, 22f);
            Vector2 c = SliceCameraSolver2D.Solve(tiny, new Vector2(-9f, 99f), 3.7f, 20f / 9f);
            Assert.That(c, Is.EqualTo(new Vector2(11f, 21f)));
        }

        [Test] public void WiderAspectNeverMovesCameraCloserToTheEdgeThanItsHalfWidth()
        {
            Vector2 narrowAspect = SliceCameraSolver2D.Solve(Stage, new Vector2(-50f, 4f), 3.7f, 16f / 9f);
            Vector2 wideAspect = SliceCameraSolver2D.Solve(Stage, new Vector2(-50f, 4f), 3.7f, 20f / 9f);
            Assert.That(wideAspect.x, Is.GreaterThan(narrowAspect.x));
        }

        [Test] public void AirplaneStageBoundsAreDerivedFromItsGeometry()
        {
            var stage = AssetDatabase.LoadAssetAtPath<StageBlockoutDefinition>("Assets/Data/Stages/Aviao_Blockout.asset");
            Assert.That(stage.TryGetWorldBounds(out Rect b), Is.True);
            Assert.That(b.xMin, Is.EqualTo(-1f).Within(1e-4f));
            Assert.That(b.xMax, Is.EqualTo(49f).Within(1e-4f));
            Assert.That(b.yMin, Is.EqualTo(-1.5f).Within(1e-4f));
            Assert.That(b.yMax, Is.EqualTo(5.25f).Within(1e-4f));
        }

        [Test] public void InvalidStageHasNoBounds()
        {
            var stage = ScriptableObject.CreateInstance<StageBlockoutDefinition>();
            try { Assert.That(stage.TryGetWorldBounds(out _), Is.False); }
            finally { Object.DestroyImmediate(stage); }
        }
    }
}
