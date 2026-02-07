using System;
using System.Collections.Generic;

namespace Kivancalp.Gameplay.Configuration
{
    public static class GameConfigParser
    {
        public static GameConfig Parse(GameConfigDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            if (dto.layouts == null || dto.layouts.Length == 0)
            {
                throw new InvalidOperationException("Config must contain at least one layout.");
            }

            if (dto.score == null)
            {
                throw new InvalidOperationException("Config must contain score settings.");
            }

            var layouts = new BoardLayoutConfig[dto.layouts.Length];
            var layoutIds = new HashSet<int>();

            for (int index = 0; index < dto.layouts.Length; index += 1)
            {
                GameConfigDto.LayoutDto sourceLayout = dto.layouts[index];

                if (sourceLayout == null)
                {
                    throw new InvalidOperationException("Config contains an empty layout entry at index " + index + ".");
                }

                string displayName = string.IsNullOrEmpty(sourceLayout.name)
                    ? sourceLayout.rows + "x" + sourceLayout.columns
                    : sourceLayout.name;

                if (sourceLayout.id < 0)
                {
                    throw new InvalidOperationException("Layout id cannot be negative. id=" + sourceLayout.id);
                }

                if (!layoutIds.Add(sourceLayout.id))
                {
                    throw new InvalidOperationException("Layout ids must be unique. Duplicate id=" + sourceLayout.id);
                }

                if (sourceLayout.rows <= 0)
                {
                    throw new InvalidOperationException("Layout rows must be greater than zero. id=" + sourceLayout.id + " rows=" + sourceLayout.rows);
                }

                if (sourceLayout.columns <= 0)
                {
                    throw new InvalidOperationException("Layout columns must be greater than zero. id=" + sourceLayout.id + " columns=" + sourceLayout.columns);
                }

                if ((sourceLayout.rows * sourceLayout.columns) % 2 != 0)
                {
                    throw new InvalidOperationException("Layout card count must be even for pair matching. id=" + sourceLayout.id + " rows=" + sourceLayout.rows + " columns=" + sourceLayout.columns);
                }

                var layout = new BoardLayoutConfig(
                    new LayoutId(sourceLayout.id),
                    displayName,
                    sourceLayout.rows,
                    sourceLayout.columns,
                    Math.Max(0f, sourceLayout.spacing),
                    Math.Max(0f, sourceLayout.padding));

                if (!layout.IsValid)
                {
                    throw new InvalidOperationException("Invalid layout in config. id=" + sourceLayout.id);
                }

                layouts[index] = layout;
            }

            var defaultLayoutId = new LayoutId(dto.defaultLayoutId);

            if (!layoutIds.Contains(defaultLayoutId.Value))
            {
                throw new InvalidOperationException("Default layout id is not present in layout list. id=" + defaultLayoutId.Value);
            }

            if (dto.score.matchScore < 0)
            {
                throw new InvalidOperationException("matchScore cannot be negative. value=" + dto.score.matchScore);
            }

            if (dto.score.mismatchPenalty < 0)
            {
                throw new InvalidOperationException("mismatchPenalty cannot be negative. value=" + dto.score.mismatchPenalty);
            }

            if (dto.score.comboBonusStep < 0)
            {
                throw new InvalidOperationException("comboBonusStep cannot be negative. value=" + dto.score.comboBonusStep);
            }

            if (dto.flipDurationSeconds < 0f)
            {
                throw new InvalidOperationException("flipDurationSeconds cannot be negative. value=" + dto.flipDurationSeconds);
            }

            if (dto.compareDelaySeconds < 0f)
            {
                throw new InvalidOperationException("compareDelaySeconds cannot be negative. value=" + dto.compareDelaySeconds);
            }

            if (dto.mismatchRevealSeconds < 0f)
            {
                throw new InvalidOperationException("mismatchRevealSeconds cannot be negative. value=" + dto.mismatchRevealSeconds);
            }

            if (dto.saveDebounceSeconds < 0f)
            {
                throw new InvalidOperationException("saveDebounceSeconds cannot be negative. value=" + dto.saveDebounceSeconds);
            }

            var scoring = new ScoringConfig(
                dto.score.matchScore,
                dto.score.mismatchPenalty,
                dto.score.comboBonusStep);
            float flipDurationSeconds = dto.flipDurationSeconds;
            float compareDelaySeconds = dto.compareDelaySeconds;
            float mismatchRevealSeconds = dto.mismatchRevealSeconds;
            float saveDebounceSeconds = dto.saveDebounceSeconds;

            return new GameConfig(
                layouts,
                defaultLayoutId,
                scoring,
                flipDurationSeconds,
                compareDelaySeconds,
                mismatchRevealSeconds,
                saveDebounceSeconds,
                dto.randomSeed);
        }
    }
}
