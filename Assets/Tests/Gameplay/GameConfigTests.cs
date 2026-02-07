using System;
using Kivancalp.Gameplay.Configuration;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class GameConfigTests
    {
        private static GameConfig CreateConfig(params BoardLayoutConfig[] layouts)
        {
            var defaultId = layouts.Length > 0 ? layouts[0].Id : new LayoutId(0);
            return new GameConfig(layouts, defaultId, new ScoringConfig(100, 25, 15), 0.2f, 0.5f, 1.0f, 2.0f, 42);
        }

        [Test]
        public void Constructor_NullLayouts_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GameConfig(null, new LayoutId(1), new ScoringConfig(100, 25, 15), 0.2f, 0.5f, 1.0f, 2.0f, 42));
        }

        [Test]
        public void Constructor_EmptyLayouts_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new GameConfig(new BoardLayoutConfig[0], new LayoutId(1), new ScoringConfig(100, 25, 15), 0.2f, 0.5f, 1.0f, 2.0f, 42));
        }

        [Test]
        public void FindLayoutIndex_ExistingId_ReturnsCorrectIndex()
        {
            var config = CreateConfig(
                new BoardLayoutConfig(new LayoutId(10), "A", 2, 2, 0, 0),
                new BoardLayoutConfig(new LayoutId(20), "B", 4, 4, 0, 0));

            Assert.AreEqual(0, config.FindLayoutIndex(new LayoutId(10)));
            Assert.AreEqual(1, config.FindLayoutIndex(new LayoutId(20)));
        }

        [Test]
        public void FindLayoutIndex_NonExistingId_ReturnsNegativeOne()
        {
            var config = CreateConfig(
                new BoardLayoutConfig(new LayoutId(10), "A", 2, 2, 0, 0));

            Assert.AreEqual(-1, config.FindLayoutIndex(new LayoutId(999)));
        }

        [Test]
        public void GetLayout_NonExistingId_ThrowsArgumentOutOfRangeException()
        {
            var config = CreateConfig(
                new BoardLayoutConfig(new LayoutId(10), "A", 2, 2, 0, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => config.GetLayout(new LayoutId(999)));
        }

        [Test]
        public void GetMaxCardCount_ReturnsLargestLayoutCardCount()
        {
            var config = CreateConfig(
                new BoardLayoutConfig(new LayoutId(1), "Small", 2, 2, 0, 0),
                new BoardLayoutConfig(new LayoutId(2), "Big", 4, 6, 0, 0),
                new BoardLayoutConfig(new LayoutId(3), "Medium", 2, 4, 0, 0));

            Assert.AreEqual(24, config.GetMaxCardCount());
        }
    }
}
