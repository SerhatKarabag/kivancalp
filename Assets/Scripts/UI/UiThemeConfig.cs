using System;
using UnityEngine;

namespace Kivancalp.UI.Views
{
    [CreateAssetMenu(menuName = "Kivancalp/UI Theme Config", fileName = "ui_theme_config")]
    public sealed class UiThemeConfig : ScriptableObject
    {
        private const string ThemeResourcePath = "ui_theme_config";

        [Serializable]
        public sealed class Palette
        {
            public Color backgroundColor = new Color(0.84f, 0.89f, 0.94f, 1f);
            public Color boardFrameColor = new Color(1f, 1f, 1f, 0.14f);
            public Color bottomPanelColor = new Color(1f, 1f, 1f, 0.20f);
            public Color chipColor = new Color(0.10f, 0.19f, 0.34f, 0.12f);
            public Color boardContainerColor = new Color(0.09f, 0.16f, 0.33f, 0.10f);
            public Color topBandColor = new Color(1f, 1f, 1f, 0.08f);
            public Color bottomBandColor = new Color(1f, 1f, 1f, 0.06f);
            public Color boardHeaderColor = new Color(0.06f, 0.15f, 0.31f, 0.20f);
            public Color actionButtonColor = new Color(0.12f, 0.29f, 0.61f, 0.96f);
            public Color actionButtonHighlightedColor = new Color(0.16f, 0.36f, 0.70f, 1f);
            public Color actionButtonPressedColor = new Color(0.08f, 0.23f, 0.50f, 1f);
            public Color actionButtonDisabledColor = new Color(0.34f, 0.42f, 0.56f, 0.7f);
            public Color buttonLabelColor = Color.white;
            public Color labelTextColor = new Color(0.11f, 0.16f, 0.26f, 1f);
            public Color whiteOutlineColor = new Color(1f, 1f, 1f, 0.16f);
            public Color whiteOutlineSoftColor = new Color(1f, 1f, 1f, 0.12f);
        }

        [Serializable]
        public sealed class LayoutMetrics
        {
            public float safeAreaMargin = 16f;
            public float safeAreaSpacing = 12f;
            public float sectionInset = 14f;
            public float bottomSectionVerticalInset = 12f;
            public float bottomPanelHeight = 92f;
            public float boardHeaderTopOffset = 10f;
            public float boardHeaderHeight = 96f;
            public float boardHeaderVerticalSpacing = 6f;
            public float boardHeaderRowSpacing = 8f;
            public float boardContainerBottomInset = 14f;
            public float boardContainerTopPadding = 20f;
            public float headerChipHorizontalPadding = 12f;
            public float headerChipVerticalPadding = 3f;
            public float bottomRowSpacing = 12f;
            public float topBandStartY = 0.72f;
            public float bottomBandEndY = 0.24f;
            public float chipMinWidthRatio = 0.6f;
            public float buttonMinWidth = 90f;
            public float buttonPreferredWidth = 220f;
            public float buttonLabelHorizontalPadding = 10f;
            public float buttonLabelVerticalPadding = 4f;
            public float scoreChipPreferredWidth = 230f;
            public float layoutChipPreferredWidth = 170f;
            public float statusChipPreferredWidth = 180f;
            public float turnsChipPreferredWidth = 180f;
            public float matchesChipPreferredWidth = 210f;
            public float comboChipPreferredWidth = 180f;
        }

        [Serializable]
        public sealed class Typography
        {
            public int headerTextSize = 28;
            public int secondaryTextSize = 23;
            public int textBestFitMinSize = 10;
            public int buttonTextBestFitMinSize = 12;
            public int buttonTextBestFitMaxSize = 24;
            public FontStyle labelFontStyle = FontStyle.Bold;
        }

        [Serializable]
        public sealed class Labels
        {
            public string previousLayoutButtonLabel = "Layout -";
            public string nextLayoutButtonLabel = "Layout +";
            public string newGameButtonLabel = "New Game";
        }

        [Serializable]
        public sealed class HudLabels
        {
            public string scorePrefix = "Score: ";
            public string turnsPrefix = "Turns: ";
            public string matchesPrefix = "Matches: ";
            public string comboPrefix = "Combo: ";
            public string layoutPrefix = "Layout: ";
            public string completedStatus = "Completed";
        }

        [Serializable]
        public sealed class CardStyle
        {
            public Vector2 baseSize = new Vector2(120f, 160f);
            public Color backColor = new Color(0.11f, 0.25f, 0.70f, 1f);
            public Color backInnerColor = new Color(0.16f, 0.33f, 0.84f, 1f);
            public Color matchedColor = new Color(0.30f, 0.63f, 0.27f, 1f);
            public Color matchedOverlayColor = new Color(0.93f, 1f, 0.93f, 0.25f);
            public Color hoverOverlayColor = new Color(1f, 1f, 1f, 0.12f);
            public Color shadowColor = new Color(0f, 0f, 0f, 0.22f);
            public Vector2 shadowOffset = new Vector2(4f, -4f);
            public Color outlineColor = new Color(1f, 1f, 1f, 0.18f);
            public Vector2 outlineOffset = new Vector2(1f, -1f);
            public float innerPadding = 6f;
            public Color backStripeColor = new Color(1f, 0.90f, 0.20f, 0.58f);
            public int backStripeCount = 4;
            public float backStripeStartY = 0.18f;
            public float backStripeStep = 0.20f;
            public float backStripeHeight = 0.08f;
            public Color topShineColor = new Color(1f, 1f, 1f, 0.20f);
            public float topShineStartY = 0.56f;
            public float frontTintAmount = 0.14f;
            public float matchedTintAmount = 0.18f;
            public Color labelColor = new Color(0.08f, 0.10f, 0.14f, 1f);
            public Color matchedLabelColor = new Color(0.06f, 0.17f, 0.05f, 1f);
            public int labelFontSize = 42;
            public int labelBestFitMinSize = 14;
            public int labelBestFitMaxSize = 48;
            public Color[] faceColors = CreateDefaultFaceColors();

            private static Color[] CreateDefaultFaceColors()
            {
                return new[]
                {
                    new Color(0.93f, 0.33f, 0.30f, 1f),
                    new Color(0.95f, 0.49f, 0.18f, 1f),
                    new Color(0.96f, 0.74f, 0.20f, 1f),
                    new Color(0.84f, 0.28f, 0.52f, 1f),
                    new Color(0.66f, 0.31f, 0.82f, 1f),
                    new Color(0.30f, 0.44f, 0.88f, 1f),
                    new Color(0.21f, 0.58f, 0.88f, 1f),
                    new Color(0.51f, 0.36f, 0.82f, 1f),
                    new Color(0.72f, 0.42f, 0.27f, 1f),
                    new Color(0.56f, 0.49f, 0.44f, 1f),
                    new Color(0.80f, 0.35f, 0.37f, 1f),
                    new Color(0.40f, 0.34f, 0.64f, 1f),
                    new Color(0.83f, 0.57f, 0.23f, 1f),
                    new Color(0.22f, 0.46f, 0.77f, 1f),
                    new Color(0.62f, 0.32f, 0.68f, 1f),
                };
            }

            public void EnsureInitialized()
            {
                if (faceColors == null || faceColors.Length == 0)
                {
                    faceColors = CreateDefaultFaceColors();
                }

                if (backStripeCount < 1)
                {
                    backStripeCount = 1;
                }

                if (labelBestFitMaxSize < labelBestFitMinSize)
                {
                    labelBestFitMaxSize = labelBestFitMinSize;
                }
            }
        }

        public Palette palette = new Palette();
        public LayoutMetrics layout = new LayoutMetrics();
        public Typography typography = new Typography();
        public Labels labels = new Labels();
        public HudLabels hudLabels = new HudLabels();
        public CardStyle card = new CardStyle();

        public static UiThemeConfig LoadOrCreateRuntimeDefault()
        {
            UiThemeConfig themeConfig = Resources.Load<UiThemeConfig>(ThemeResourcePath);

            if (themeConfig == null)
            {
                themeConfig = CreateInstance<UiThemeConfig>();
            }

            themeConfig.EnsureInitialized();
            return themeConfig;
        }

        public void EnsureInitialized()
        {
            if (palette == null)
            {
                palette = new Palette();
            }

            if (layout == null)
            {
                layout = new LayoutMetrics();
            }

            if (typography == null)
            {
                typography = new Typography();
            }

            if (labels == null)
            {
                labels = new Labels();
            }

            if (hudLabels == null)
            {
                hudLabels = new HudLabels();
            }

            if (card == null)
            {
                card = new CardStyle();
            }

            card.EnsureInitialized();
        }
    }
}
