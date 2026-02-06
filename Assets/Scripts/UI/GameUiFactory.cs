using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public static class GameUiFactory
    {
        private const string InputSystemUiModuleTypeName = "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem";
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        public static GameUiContext Create(Transform parent, UiThemeConfig theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            theme.EnsureInitialized();

            Font uiFont = Resources.GetBuiltinResource<Font>(BuiltinFontName);
            GameObject rootObject = CreateUiRoot(parent);
            CreateEventSystemIfNeeded();

            var canvas = rootObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = rootObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            rootObject.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            BuildBackground(rootRect, theme);
            RectTransform safeArea = CreateSafeArea(rootRect, theme);

            RectTransform boardPanel = CreateSectionPanel(
                "BoardPanel",
                safeArea,
                theme.palette.boardFrameColor,
                0f,
                1f,
                theme.palette.whiteOutlineColor);

            RectTransform bottomPanel = CreateSectionPanel(
                "BottomPanel",
                safeArea,
                theme.palette.bottomPanelColor,
                theme.layout.bottomPanelHeight,
                0f,
                theme.palette.whiteOutlineColor);

            RectTransform boardHeader = CreateBoardHeader(boardPanel, theme);
            RectTransform primaryHeaderRow = CreateHeaderRow("PrimaryHeaderRow", boardHeader, theme.layout.boardHeaderRowSpacing);
            RectTransform secondaryHeaderRow = CreateHeaderRow("SecondaryHeaderRow", boardHeader, theme.layout.boardHeaderRowSpacing);

            Text scoreText = CreateHudChip(
                "ScoreChip",
                primaryHeaderRow,
                uiFont,
                theme.hudLabels.scorePrefix + "0",
                TextAnchor.MiddleLeft,
                theme.layout.scoreChipPreferredWidth,
                0f,
                theme.typography.headerTextSize,
                true,
                theme);

            Text layoutText = CreateHudChip(
                "LayoutChip",
                primaryHeaderRow,
                uiFont,
                theme.hudLabels.layoutPrefix + "0",
                TextAnchor.MiddleLeft,
                theme.layout.layoutChipPreferredWidth,
                0f,
                theme.typography.headerTextSize,
                true,
                theme);

            CreateFlexibleSpacer(primaryHeaderRow);

            Text statusText = CreateHudChip(
                "StatusChip",
                primaryHeaderRow,
                uiFont,
                string.Empty,
                TextAnchor.MiddleRight,
                theme.layout.statusChipPreferredWidth,
                0f,
                theme.typography.headerTextSize,
                false,
                theme);

            Text turnsText = CreateHudChip(
                "TurnsChip",
                secondaryHeaderRow,
                uiFont,
                theme.hudLabels.turnsPrefix + "0",
                TextAnchor.MiddleLeft,
                theme.layout.turnsChipPreferredWidth,
                0f,
                theme.typography.secondaryTextSize,
                true,
                theme);

            Text matchesText = CreateHudChip(
                "MatchesChip",
                secondaryHeaderRow,
                uiFont,
                theme.hudLabels.matchesPrefix + "0",
                TextAnchor.MiddleLeft,
                theme.layout.matchesChipPreferredWidth,
                0f,
                theme.typography.secondaryTextSize,
                true,
                theme);

            Text comboText = CreateHudChip(
                "ComboChip",
                secondaryHeaderRow,
                uiFont,
                theme.hudLabels.comboPrefix + "0",
                TextAnchor.MiddleLeft,
                theme.layout.comboChipPreferredWidth,
                0f,
                theme.typography.secondaryTextSize,
                true,
                theme);

            float boardTopInset = theme.layout.boardHeaderHeight + theme.layout.boardContainerTopPadding;
            RectTransform boardContainer = CreateBoardContainer(boardPanel, boardTopInset, theme);
            RectTransform bottomRow = CreateBottomRow(bottomPanel, theme);

            Button previousLayoutButton = CreateActionButton("PrevLayoutButton", bottomRow, uiFont, theme.labels.previousLayoutButtonLabel, theme);
            Button nextLayoutButton = CreateActionButton("NextLayoutButton", bottomRow, uiFont, theme.labels.nextLayoutButtonLabel, theme);
            Button newGameButton = CreateActionButton("NewGameButton", bottomRow, uiFont, theme.labels.newGameButtonLabel, theme);

            return new GameUiContext(
                rootObject,
                uiFont,
                boardContainer,
                scoreText,
                turnsText,
                matchesText,
                comboText,
                layoutText,
                statusText,
                newGameButton,
                previousLayoutButton,
                nextLayoutButton);
        }

        private static void BuildBackground(RectTransform rootRect, UiThemeConfig theme)
        {
            CreateFullscreenImage("Background", rootRect, theme.palette.backgroundColor);

            Image topBand = CreateImage("TopBand", rootRect, theme.palette.topBandColor);
            RectTransform topBandRect = topBand.rectTransform;
            topBandRect.anchorMin = new Vector2(0f, theme.layout.topBandStartY);
            topBandRect.anchorMax = new Vector2(1f, 1f);
            topBandRect.offsetMin = Vector2.zero;
            topBandRect.offsetMax = Vector2.zero;

            Image bottomBand = CreateImage("BottomBand", rootRect, theme.palette.bottomBandColor);
            RectTransform bottomBandRect = bottomBand.rectTransform;
            bottomBandRect.anchorMin = new Vector2(0f, 0f);
            bottomBandRect.anchorMax = new Vector2(1f, theme.layout.bottomBandEndY);
            bottomBandRect.offsetMin = Vector2.zero;
            bottomBandRect.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateSafeArea(RectTransform parent, UiThemeConfig theme)
        {
            var safeAreaObject = new GameObject("SafeArea", typeof(RectTransform), typeof(VerticalLayoutGroup));
            RectTransform safeAreaRect = safeAreaObject.GetComponent<RectTransform>();
            safeAreaRect.SetParent(parent, false);
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = new Vector2(theme.layout.safeAreaMargin, theme.layout.safeAreaMargin);
            safeAreaRect.offsetMax = new Vector2(-theme.layout.safeAreaMargin, -theme.layout.safeAreaMargin);

            VerticalLayoutGroup layoutGroup = safeAreaObject.GetComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = theme.layout.safeAreaSpacing;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            return safeAreaRect;
        }

        private static RectTransform CreateSectionPanel(string name, RectTransform parent, Color color, float fixedHeight, float flexibleHeight, Color outlineColor)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Outline));
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = color;

            Outline panelOutline = panelObject.GetComponent<Outline>();
            panelOutline.effectColor = outlineColor;
            panelOutline.effectDistance = new Vector2(1f, -1f);

            LayoutElement layoutElement = panelObject.GetComponent<LayoutElement>();
            if (fixedHeight > 0f)
            {
                layoutElement.minHeight = fixedHeight;
                layoutElement.preferredHeight = fixedHeight;
            }

            layoutElement.flexibleHeight = flexibleHeight;
            return panelRect;
        }

        private static RectTransform CreateBoardHeader(RectTransform boardPanel, UiThemeConfig theme)
        {
            var headerObject = new GameObject("BoardHeader", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            RectTransform headerRect = headerObject.GetComponent<RectTransform>();
            headerRect.SetParent(boardPanel, false);
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, theme.layout.boardHeaderHeight);
            headerRect.anchoredPosition = new Vector2(0f, -theme.layout.boardHeaderTopOffset);

            Image headerImage = headerObject.GetComponent<Image>();
            headerImage.color = theme.palette.boardHeaderColor;

            VerticalLayoutGroup layoutGroup = headerObject.GetComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(12, 12, 10, 10);
            layoutGroup.spacing = theme.layout.boardHeaderVerticalSpacing;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            return headerRect;
        }

        private static RectTransform CreateHeaderRow(string name, RectTransform parent, float spacing)
        {
            var rowObject = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);

            HorizontalLayoutGroup layoutGroup = rowObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = spacing;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;

            LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            return rowRect;
        }

        private static Text CreateHudChip(string name, RectTransform parent, Font font, string initialText, TextAnchor alignment, float preferredWidth, float flexibleWidth, int maxSize, bool useBackground, UiThemeConfig theme)
        {
            var chipObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            RectTransform chipRect = chipObject.GetComponent<RectTransform>();
            chipRect.SetParent(parent, false);

            Image chipImage = chipObject.GetComponent<Image>();
            chipImage.color = useBackground ? theme.palette.chipColor : new Color(0f, 0f, 0f, 0f);

            LayoutElement chipLayout = chipObject.GetComponent<LayoutElement>();
            chipLayout.minWidth = preferredWidth * theme.layout.chipMinWidthRatio;
            chipLayout.preferredWidth = preferredWidth;
            chipLayout.flexibleWidth = flexibleWidth;

            Text text = CreateText(
                "Text",
                chipRect,
                font,
                maxSize,
                alignment,
                theme.palette.labelTextColor,
                Vector2.zero,
                Vector2.one,
                new Vector2(theme.layout.headerChipHorizontalPadding, theme.layout.headerChipVerticalPadding),
                new Vector2(-theme.layout.headerChipHorizontalPadding, -theme.layout.headerChipVerticalPadding),
                initialText);

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = theme.typography.textBestFitMinSize;
            text.resizeTextMaxSize = maxSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.fontStyle = theme.typography.labelFontStyle;
            return text;
        }

        private static void CreateFlexibleSpacer(RectTransform parent)
        {
            var spacerObject = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            RectTransform spacerRect = spacerObject.GetComponent<RectTransform>();
            spacerRect.SetParent(parent, false);

            LayoutElement layoutElement = spacerObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = 0f;
            layoutElement.preferredWidth = 0f;
            layoutElement.flexibleWidth = 1f;
        }

        private static RectTransform CreateBoardContainer(RectTransform boardPanel, float topInset, UiThemeConfig theme)
        {
            var containerObject = new GameObject("BoardContainer", typeof(RectTransform), typeof(Image), typeof(Outline));
            RectTransform containerRect = containerObject.GetComponent<RectTransform>();
            containerRect.SetParent(boardPanel, false);
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = new Vector2(theme.layout.sectionInset, theme.layout.boardContainerBottomInset);
            containerRect.offsetMax = new Vector2(-theme.layout.sectionInset, -topInset);

            Image containerImage = containerObject.GetComponent<Image>();
            containerImage.color = theme.palette.boardContainerColor;

            Outline containerOutline = containerObject.GetComponent<Outline>();
            containerOutline.effectColor = theme.palette.whiteOutlineSoftColor;
            containerOutline.effectDistance = new Vector2(1f, -1f);
            return containerRect;
        }

        private static RectTransform CreateBottomRow(RectTransform parent, UiThemeConfig theme)
        {
            var rowObject = new GameObject("BottomRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);
            rowRect.anchorMin = Vector2.zero;
            rowRect.anchorMax = Vector2.one;
            rowRect.offsetMin = new Vector2(theme.layout.sectionInset, theme.layout.bottomSectionVerticalInset);
            rowRect.offsetMax = new Vector2(-theme.layout.sectionInset, -theme.layout.bottomSectionVerticalInset);

            HorizontalLayoutGroup layoutGroup = rowObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = theme.layout.bottomRowSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            return rowRect;
        }

        private static GameObject CreateUiRoot(Transform parent)
        {
            var rootObject = new GameObject("CardMatchUiRoot", typeof(RectTransform));
            RectTransform rect = rootObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rootObject;
        }

        private static void CreateEventSystemIfNeeded()
        {
            EventSystem current = EventSystem.current;
            if (current != null)
            {
                EnsureInputModule(current.gameObject);
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            EnsureInputModule(eventSystemObject);
            UnityEngine.Object.DontDestroyOnLoad(eventSystemObject);
        }

        private static void EnsureInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule standaloneInputModule = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (standaloneInputModule != null)
            {
                standaloneInputModule.enabled = false;
                UnityEngine.Object.Destroy(standaloneInputModule);
            }

            Type inputSystemUiModuleType = Type.GetType(InputSystemUiModuleTypeName);
            if (inputSystemUiModuleType != null)
            {
                if (eventSystemObject.GetComponent(inputSystemUiModuleType) == null)
                {
                    eventSystemObject.AddComponent(inputSystemUiModuleType);
                }
            }
#else
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private static Image CreateImage(string name, RectTransform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Image CreateFullscreenImage(string name, RectTransform parent, Color color)
        {
            Image image = CreateImage(name, parent, color);
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private static Text CreateText(string name, RectTransform parent, Font font, int fontSize, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string initialText = "")
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(parent, false);
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.offsetMin = offsetMin;
            textRect.offsetMax = offsetMax;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = initialText;
            return text;
        }

        private static Button CreateActionButton(string name, RectTransform parent, Font font, string caption, UiThemeConfig theme)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(Outline));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = theme.layout.buttonMinWidth;
            layoutElement.preferredWidth = theme.layout.buttonPreferredWidth;
            layoutElement.flexibleWidth = 1f;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = theme.palette.actionButtonColor;

            Outline outline = buttonObject.GetComponent<Outline>();
            outline.effectColor = theme.palette.whiteOutlineColor;
            outline.effectDistance = new Vector2(1f, -1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = buttonImage.color;
            colors.highlightedColor = theme.palette.actionButtonHighlightedColor;
            colors.pressedColor = theme.palette.actionButtonPressedColor;
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = theme.palette.actionButtonDisabledColor;
            button.colors = colors;

            Text label = CreateText(
                "Label",
                buttonRect,
                font,
                theme.typography.buttonTextBestFitMaxSize,
                TextAnchor.MiddleCenter,
                theme.palette.buttonLabelColor,
                Vector2.zero,
                Vector2.one,
                new Vector2(theme.layout.buttonLabelHorizontalPadding, theme.layout.buttonLabelVerticalPadding),
                new Vector2(-theme.layout.buttonLabelHorizontalPadding, -theme.layout.buttonLabelVerticalPadding),
                caption);

            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = theme.typography.buttonTextBestFitMinSize;
            label.resizeTextMaxSize = theme.typography.buttonTextBestFitMaxSize;
            label.raycastTarget = false;

            return button;
        }
    }
}
