using Kivancalp.Gameplay.Configuration;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public static class BoardLayoutCalculator
    {
        public static void Apply(RectTransform container, BoardLayoutConfig layout, CardView[] activeViews, int cardCount)
        {
            Rect containerRect = container.rect;
            float width = containerRect.width;
            float height = containerRect.height;

            if (width <= 1f || height <= 1f)
            {
                return;
            }

            float horizontalPadding = layout.Padding;
            float verticalPadding = layout.Padding;
            float spacing = layout.Spacing;

            float availableWidth = width - (2f * horizontalPadding) - ((layout.Columns - 1) * spacing);
            float availableHeight = height - (2f * verticalPadding) - ((layout.Rows - 1) * spacing);

            if (availableWidth <= 0f || availableHeight <= 0f)
            {
                return;
            }

            float cardWidth = availableWidth / layout.Columns;
            float cardHeight = availableHeight / layout.Rows;
            float cardSize = Mathf.Min(cardWidth, cardHeight);

            float boardWidth = (layout.Columns * cardSize) + ((layout.Columns - 1) * spacing);
            float boardHeight = (layout.Rows * cardSize) + ((layout.Rows - 1) * spacing);

            float startX = -boardWidth * 0.5f + (cardSize * 0.5f);
            float startY = boardHeight * 0.5f - (cardSize * 0.5f);

            for (int index = 0; index < cardCount; index += 1)
            {
                CardView view = activeViews[index];

                if (view == null)
                {
                    continue;
                }

                int row = index / layout.Columns;
                int column = index % layout.Columns;

                float x = startX + (column * (cardSize + spacing));
                float y = startY - (row * (cardSize + spacing));

                RectTransform rect = view.RectTransform;
                rect.anchoredPosition = new Vector2(x, y);
                rect.sizeDelta = new Vector2(cardSize, cardSize);
            }
        }
    }
}
