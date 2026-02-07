using Kivancalp.Gameplay.Application;
using Kivancalp.Gameplay.Configuration;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class LayoutNavigatorTests
    {
        private static GameConfig CreateConfig(LayoutId defaultId, params BoardLayoutConfig[] layouts)
        {
            return new GameConfig(
                layouts,
                defaultId,
                new ScoringConfig(100, 25, 15),
                flipDurationSeconds: 0.2f,
                compareDelaySeconds: 0.5f,
                mismatchRevealSeconds: 1.0f,
                saveDebounceSeconds: 2.0f,
                randomSeed: 42);
        }

        private static BoardLayoutConfig Layout(int id, int rows, int cols)
        {
            return new BoardLayoutConfig(new LayoutId(id), id + "x", rows, cols, 0f, 0f);
        }

        [Test]
        public void Constructor_DefaultLayoutFound()
        {
            var config = CreateConfig(new LayoutId(2), Layout(1, 2, 2), Layout(2, 4, 4), Layout(3, 2, 4));
            var navigator = new LayoutNavigator(config);

            Assert.AreEqual(1, navigator.CurrentLayoutIndex);
            Assert.AreEqual(new LayoutId(2), navigator.CurrentLayout.Id);
        }

        [Test]
        public void Constructor_InvalidDefaultLayoutId_FallsBackToIndex0()
        {
            var config = CreateConfig(new LayoutId(999), Layout(1, 2, 2), Layout(2, 4, 4));
            var navigator = new LayoutNavigator(config);

            Assert.AreEqual(0, navigator.CurrentLayoutIndex);
        }

        [Test]
        public void ResolveLayoutIndex_ValidId_ReturnsCorrectIndex()
        {
            var config = CreateConfig(new LayoutId(1), Layout(1, 2, 2), Layout(2, 4, 4), Layout(3, 2, 4));
            var navigator = new LayoutNavigator(config);

            int index = navigator.ResolveLayoutIndex(new LayoutId(3));

            Assert.AreEqual(2, index);
        }

        [Test]
        public void ResolveLayoutIndex_InvalidId_FallsBackToDefault()
        {
            var config = CreateConfig(new LayoutId(1), Layout(1, 2, 2), Layout(2, 4, 4));
            var navigator = new LayoutNavigator(config);

            int index = navigator.ResolveLayoutIndex(new LayoutId(999));

            Assert.AreEqual(0, index);
        }

        [Test]
        public void SetLayout_UpdatesCurrentLayout()
        {
            var config = CreateConfig(new LayoutId(1), Layout(1, 2, 2), Layout(2, 4, 4), Layout(3, 2, 4));
            var navigator = new LayoutNavigator(config);

            navigator.SetLayout(2);

            Assert.AreEqual(2, navigator.CurrentLayoutIndex);
            Assert.AreEqual(new LayoutId(3), navigator.CurrentLayout.Id);
        }

        [Test]
        public void CalculateOffsetIndex_WrapsAroundCorrectly()
        {
            var config = CreateConfig(new LayoutId(1), Layout(1, 2, 2), Layout(2, 4, 4), Layout(3, 2, 4));
            var navigator = new LayoutNavigator(config);
            navigator.SetLayout(2);

            Assert.AreEqual(0, navigator.CalculateOffsetIndex(1), "Forward wrap");
            Assert.AreEqual(1, navigator.CalculateOffsetIndex(-1), "Backward from index 2");

            navigator.SetLayout(0);
            Assert.AreEqual(2, navigator.CalculateOffsetIndex(-1), "Backward wrap from index 0");
        }
    }
}
