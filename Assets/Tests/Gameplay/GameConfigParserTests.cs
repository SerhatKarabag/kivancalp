using System;
using Kivancalp.Gameplay.Configuration;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class GameConfigParserTests
    {
        private static GameConfigDto CreateValidDto()
        {
            return new GameConfigDto
            {
                defaultLayoutId = 1,
                layouts = new[]
                {
                    new GameConfigDto.LayoutDto { id = 1, name = "Small", rows = 2, columns = 2, spacing = 5f, padding = 10f },
                    new GameConfigDto.LayoutDto { id = 2, name = "Medium", rows = 2, columns = 4, spacing = 5f, padding = 10f },
                },
                score = new GameConfigDto.ScoreDto
                {
                    matchScore = 100,
                    mismatchPenalty = 25,
                    comboBonusStep = 15,
                },
                flipDurationSeconds = 0.2f,
                compareDelaySeconds = 0.5f,
                mismatchRevealSeconds = 1.0f,
                saveDebounceSeconds = 2.0f,
                randomSeed = 42,
            };
        }

        [Test]
        public void Parse_ValidDto_ReturnsCorrectGameConfig()
        {
            var dto = CreateValidDto();

            var config = GameConfigParser.Parse(dto);

            Assert.AreEqual(2, config.LayoutCount);
            Assert.AreEqual(new LayoutId(1), config.DefaultLayoutId);
            Assert.AreEqual(100, config.Scoring.MatchScore);
            Assert.AreEqual(25, config.Scoring.MismatchPenalty);
            Assert.AreEqual(15, config.Scoring.ComboBonusStep);
            Assert.AreEqual(0.2f, config.FlipDurationSeconds);
            Assert.AreEqual(42, config.RandomSeed);
        }

        [Test]
        public void Parse_NullDto_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => GameConfigParser.Parse(null));
        }

        [Test]
        public void Parse_EmptyLayouts_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.layouts = new GameConfigDto.LayoutDto[0];

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_NullLayouts_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.layouts = null;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_DuplicateLayoutId_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.layouts[1].id = 1;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_OddCardCount_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.layouts[0].rows = 3;
            dto.layouts[0].columns = 3;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_NegativeMatchScore_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.score.matchScore = -1;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_NegativeFlipDuration_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.flipDurationSeconds = -0.1f;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_DefaultLayoutIdNotInLayouts_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.defaultLayoutId = 999;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_ZeroRows_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.layouts[0].rows = 0;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_NullScore_ThrowsInvalidOperationException()
        {
            var dto = CreateValidDto();
            dto.score = null;

            Assert.Throws<InvalidOperationException>(() => GameConfigParser.Parse(dto));
        }

        [Test]
        public void Parse_LayoutWithoutName_GeneratesDefaultName()
        {
            var dto = CreateValidDto();
            dto.layouts[0].name = null;

            var config = GameConfigParser.Parse(dto);
            var layout = config.GetLayoutByIndex(0);

            Assert.AreEqual("2x2", layout.DisplayName);
        }
    }
}
