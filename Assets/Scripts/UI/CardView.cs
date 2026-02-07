using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public sealed class CardView : UIBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _frontImage;
        [SerializeField] private Image _frontInnerImage;
        [SerializeField] private Image _backImage;
        [SerializeField] private Image _backInnerImage;
        [SerializeField] private Image _matchedOverlay;
        [SerializeField] private Image _hoverOverlay;
        [SerializeField] private Text _faceLabel;

        private RectTransform _rectTransform;
        private UiThemeConfig.CardStyle _style;
        private Action<int> _clickHandler;
        private int _cardIndex;
        private Color _baseFaceColor;

        public RectTransform RectTransform => _rectTransform;

        public void Initialize(UiThemeConfig.CardStyle style)
        {
            _style = style ?? throw new ArgumentNullException(nameof(style));
            _style.EnsureInitialized();
            _rectTransform = GetComponent<RectTransform>();
            SetFaceUpImmediate(false);
            SetHorizontalScale(1f);
        }

        public void Bind(int cardIndex, int faceId, Action<int> clickHandler)
        {
            _cardIndex = cardIndex;
            _clickHandler = clickHandler;

            Color[] faceColors = _style.faceColors;

            if (faceColors == null || faceColors.Length == 0)
            {
                throw new InvalidOperationException("Card face colors must contain at least one entry.");
            }

            int safeFaceId = faceId < 0 ? 0 : faceId;
            _baseFaceColor = faceColors[safeFaceId % faceColors.Length];

            _frontImage.color = _baseFaceColor;
            _frontInnerImage.color = Tint(_baseFaceColor, _style.frontTintAmount);
            _faceLabel.text = (safeFaceId + 1).ToString();
            _faceLabel.color = _style.labelColor;
            _faceLabel.raycastTarget = false;
            _hoverOverlay.enabled = false;
            SetMatched(false);
            SetHorizontalScale(1f);
        }

        public void SetFaceUpImmediate(bool isFaceUp)
        {
            _frontImage.enabled = isFaceUp;
            _frontInnerImage.enabled = isFaceUp;
            _faceLabel.enabled = isFaceUp;
            _backImage.enabled = !isFaceUp;
            _backInnerImage.enabled = !isFaceUp;
            _hoverOverlay.enabled = false;

            if (!isFaceUp)
            {
                _matchedOverlay.enabled = false;
            }
        }

        public void SetMatched(bool isMatched)
        {
            if (isMatched)
            {
                _frontImage.color = _style.matchedColor;
                _frontInnerImage.color = Tint(_style.matchedColor, _style.matchedTintAmount);
                _faceLabel.color = _style.matchedLabelColor;
                _matchedOverlay.enabled = true;
                return;
            }

            _frontImage.color = _baseFaceColor;
            _frontInnerImage.color = Tint(_baseFaceColor, _style.frontTintAmount);
            _faceLabel.color = _style.labelColor;
            _matchedOverlay.enabled = false;
        }

        public void SetHorizontalScale(float value)
        {
            Vector3 scale = RectTransform.localScale;
            scale.x = value;
            RectTransform.localScale = scale;
        }

        public void ResetState()
        {
            _clickHandler = null;
            SetHorizontalScale(1f);
            SetFaceUpImmediate(false);
            SetMatched(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _clickHandler?.Invoke(_cardIndex);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_frontImage.enabled || _backImage.enabled)
            {
                _hoverOverlay.enabled = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverOverlay.enabled = false;
        }

        private static Color Tint(Color color, float amount)
        {
            float r = Mathf.Clamp01(color.r + amount);
            float g = Mathf.Clamp01(color.g + amount);
            float b = Mathf.Clamp01(color.b + amount);
            return new Color(r, g, b, color.a);
        }
    }
}
