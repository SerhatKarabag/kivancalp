using UnityEngine;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public sealed class GameUiRef : MonoBehaviour
    {
        [SerializeField] private RectTransform boardContainer;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text turnsText;
        [SerializeField] private Text matchesText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text layoutText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button previousLayoutButton;
        [SerializeField] private Button nextLayoutButton;

        public RectTransform BoardContainer => boardContainer;

        public Text ScoreText => scoreText;

        public Text TurnsText => turnsText;

        public Text MatchesText => matchesText;

        public Text ComboText => comboText;

        public Text LayoutText => layoutText;

        public Text StatusText => statusText;

        public Button NewGameButton => newGameButton;

        public Button PreviousLayoutButton => previousLayoutButton;

        public Button NextLayoutButton => nextLayoutButton;
    }
}
