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

            var layouts = new BoardLayoutConfig[dto.layouts.Length];

            for (int index = 0; index < dto.layouts.Length; index += 1)
            {
                GameConfigDto.LayoutDto sourceLayout = dto.layouts[index];
                var layout = new BoardLayoutConfig(
                    new LayoutId(sourceLayout.id),
                    sourceLayout.name,
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

            return new GameConfig(
                layouts,
                new LayoutId(dto.defaultLayoutId),
                scoring,
                dto.flipDurationSeconds,
                dto.compareDelaySeconds,
                dto.mismatchRevealSeconds,
                dto.saveDebounceSeconds,
                dto.randomSeed);
        }
    }
}
