using System;

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

                var layout = new BoardLayoutConfig(
                    new LayoutId(sourceLayout.id),
                    displayName,
                    sourceLayout.rows,
                    sourceLayout.columns,
                    sourceLayout.spacing,
                    sourceLayout.padding);

                if (!layout.IsValid)
                {
                    throw new InvalidOperationException("Invalid layout in config. id=" + sourceLayout.id);
                }

                layouts[index] = layout;
            }

            var scoring = new ScoringConfig(dto.score.matchScore, dto.score.mismatchPenalty, dto.score.comboBonusStep);
            float flipDurationSeconds = Math.Max(0f, dto.flipDurationSeconds);
            float compareDelaySeconds = Math.Max(0f, dto.compareDelaySeconds);
            float mismatchRevealSeconds = Math.Max(0f, dto.mismatchRevealSeconds);
            float saveDebounceSeconds = Math.Max(0f, dto.saveDebounceSeconds);

            return new GameConfig(
                layouts,
                new LayoutId(dto.defaultLayoutId),
                scoring,
                flipDurationSeconds,
                compareDelaySeconds,
                mismatchRevealSeconds,
                saveDebounceSeconds,
                dto.randomSeed);
        }
    }
}
