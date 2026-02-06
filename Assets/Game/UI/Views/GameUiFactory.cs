using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public static class GameUiFactory
    {
        private static readonly Color BackgroundColor = new Color(0.96f, 0.80f, 0.44f, 1f);
        private static readonly Color PanelColor = new Color(1f, 1f, 1f, 0.12f);
        private static readonly Color BoardColor = new Color(1f, 1f, 1f, 0.08f);
        private const string InputSystemUiModuleTypeName = "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem";
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        public static GameUiContext Create(Transform parent)
        {
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
            CreateFullscreenImage("Background", rootRect, BackgroundColor);

            RectTransform topPanel = CreatePanel("TopPanel", rootRect, PanelColor, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -140f), new Vector2(-12f, -12f));
            RectTransform boardPanel = CreatePanel("BoardPanel", rootRect, BoardColor, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(16f, 110f), new Vector2(-16f, -156f));
            RectTransform bottomPanel = CreatePanel("BottomPanel", rootRect, PanelColor, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 12f), new Vector2(-12f, 100f));

            RectTransform topRow = CreateTopRow(topPanel);
            Text scoreText = CreateTopRowText("ScoreText", topRow, uiFont, "Score: 0", TextAnchor.MiddleLeft, 140f, 1f);
            Text turnsText = CreateTopRowText("TurnsText", topRow, uiFont, "Turns: 0", TextAnchor.MiddleLeft, 140f, 1f);
            Text matchesText = CreateTopRowText("MatchesText", topRow, uiFont, "Matches: 0", TextAnchor.MiddleLeft, 180f, 1.2f);
            Text comboText = CreateTopRowText("ComboText", topRow, uiFont, "Combo: 0", TextAnchor.MiddleLeft, 140f, 1f);
            Text layoutText = CreateTopRowText("LayoutText", topRow, uiFont, "Layout: 0", TextAnchor.MiddleLeft, 150f, 1f);
            Text statusText = CreateTopRowText("StatusText", topRow, uiFont, string.Empty, TextAnchor.MiddleRight, 140f, 1.2f);

            Button previousLayoutButton = CreateButton("PrevLayoutButton", bottomPanel, uiFont, "Layout -", new Vector2(20f, 20f), new Vector2(200f, 70f));
            Button nextLayoutButton = CreateButton("NextLayoutButton", bottomPanel, uiFont, "Layout +", new Vector2(220f, 20f), new Vector2(400f, 70f));
            Button newGameButton = CreateButton("NewGameButton", bottomPanel, uiFont, "New Game", new Vector2(420f, 20f), new Vector2(640f, 70f));

            return new GameUiContext(
                rootObject,
                uiFont,
                boardPanel,
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

        private static RectTransform CreatePanel(string name, RectTransform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.offsetMin = offsetMin;
            panelRect.offsetMax = offsetMax;

            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panelRect;
        }

        private static RectTransform CreateTopRow(RectTransform parent)
        {
            GameObject rowObject = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.SetParent(parent, false);
            rowRect.anchorMin = Vector2.zero;
            rowRect.anchorMax = Vector2.one;
            rowRect.offsetMin = new Vector2(20f, 20f);
            rowRect.offsetMax = new Vector2(-20f, -20f);

            HorizontalLayoutGroup layoutGroup = rowObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.spacing = 16f;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;
            return rowRect;
        }

        private static Image CreateFullscreenImage(string name, RectTransform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateTopRowText(string name, RectTransform parent, Font font, string initialText, TextAnchor alignment, float minWidth, float flexibleWidth)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(Text));
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.SetParent(parent, false);

            LayoutElement layoutElement = textObject.GetComponent<LayoutElement>();
            layoutElement.minWidth = minWidth;
            layoutElement.flexibleWidth = flexibleWidth;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 28;
            text.alignment = alignment;
            text.color = Color.black;
            text.text = initialText;
            return text;
        }

        private static Text CreateTextValue(string label, RectTransform parent, Font font, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            string initialText = label + ": 0";
            return CreateText(label + "Text", parent, font, 28, TextAnchor.MiddleLeft, Color.black, anchorMin, anchorMax, offsetMin, offsetMax, initialText);
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

        private static Button CreateButton(string name, RectTransform parent, Font font, string caption, Vector2 min, Vector2 max)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.SetParent(parent, false);
            buttonRect.anchorMin = new Vector2(0f, 0f);
            buttonRect.anchorMax = new Vector2(0f, 0f);
            buttonRect.offsetMin = min;
            buttonRect.offsetMax = max;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.16f, 0.32f, 0.56f, 0.94f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.20f, 0.40f, 0.68f, 1f);
            colors.pressedColor = new Color(0.10f, 0.24f, 0.44f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            Text label = CreateText("Label", buttonRect, font, 24, TextAnchor.MiddleCenter, Color.white, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, caption);
            label.raycastTarget = false;

            return button;
        }
    }
}
