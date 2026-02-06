using UnityEngine;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public sealed class GameUiContext
    {
        public GameUiContext(
            GameObject rootObject,
            Font uiFont,
            RectTransform boardContainer,
            Text scoreText,
            Text turnsText,
            Text matchesText,
            Text comboText,
            Text layoutText,
            Text statusText,
            Button newGameButton,
            Button previousLayoutButton,
            Button nextLayoutButton)
        {
            RootObject = rootObject;
            UiFont = uiFont;
            BoardContainer = boardContainer;
            ScoreText = scoreText;
            TurnsText = turnsText;
            MatchesText = matchesText;
            ComboText = comboText;
            LayoutText = layoutText;
            StatusText = statusText;
            NewGameButton = newGameButton;
            PreviousLayoutButton = previousLayoutButton;
            NextLayoutButton = nextLayoutButton;
        }

        public GameObject RootObject { get; }

        public Font UiFont { get; }

        public RectTransform BoardContainer { get; }

        public Text ScoreText { get; }

        public Text TurnsText { get; }

        public Text MatchesText { get; }

        public Text ComboText { get; }

        public Text LayoutText { get; }

        public Text StatusText { get; }

        public Button NewGameButton { get; }

        public Button PreviousLayoutButton { get; }

        public Button NextLayoutButton { get; }
    }
}
