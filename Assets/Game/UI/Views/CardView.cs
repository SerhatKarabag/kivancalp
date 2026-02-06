using System;
using Kivancalp.Gameplay.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public sealed class CardView : UIBehaviour, IPointerClickHandler
    {
        private static readonly Color BackColor = new Color(0.16f, 0.29f, 0.76f, 1f);
        private static readonly Color MatchedColor = new Color(0.30f, 0.63f, 0.27f, 1f);

        private static readonly Color[] FaceColors =
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

        private RectTransform _rectTransform;
        private Image _hitImage;
        private Image _frontImage;
        private Image _backImage;
        private Text _faceLabel;
        private Action<int> _clickHandler;
        private int _cardIndex;
        private bool _isBuilt;

        public RectTransform RectTransform => _rectTransform;

        public void Build(Font font)
        {
            if (_isBuilt)
            {
                return;
            }

            _rectTransform = gameObject.GetComponent<RectTransform>();

            if (_rectTransform == null)
            {
                _rectTransform = gameObject.AddComponent<RectTransform>();
            }

            _hitImage = gameObject.GetComponent<Image>();

            if (_hitImage == null)
            {
                _hitImage = gameObject.AddComponent<Image>();
            }

            _hitImage.color = Color.clear;

            _backImage = CreateLayer("Back", BackColor, font, out _);
            _frontImage = CreateLayer("Front", Color.white, font, out _faceLabel);
            _faceLabel.alignment = TextAnchor.MiddleCenter;
            _faceLabel.color = Color.black;
            _faceLabel.fontSize = 36;

            SetFaceUpImmediate(false);
            SetHorizontalScale(1f);
            _isBuilt = true;
        }

        public void Bind(int cardIndex, int faceId, Action<int> clickHandler)
        {
            _cardIndex = cardIndex;
            _clickHandler = clickHandler;
            _frontImage.color = FaceColors[faceId % FaceColors.Length];
            _faceLabel.text = (faceId + 1).ToString();
            _faceLabel.raycastTarget = false;
            SetMatched(false);
            SetHorizontalScale(1f);
        }

        public void SetFaceUpImmediate(bool isFaceUp)
        {
            _frontImage.enabled = isFaceUp;
            _faceLabel.enabled = isFaceUp;
            _backImage.enabled = !isFaceUp;
        }

        public void SetMatched(bool isMatched)
        {
            if (isMatched)
            {
                _frontImage.color = MatchedColor;
            }
        }

        public void SetHorizontalScale(float value)
        {
            Vector3 scale = _rectTransform.localScale;
            scale.x = value;
            _rectTransform.localScale = scale;
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

        private Image CreateLayer(string name, Color color, Font font, out Text text)
        {
            var layerObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = layerObject.GetComponent<RectTransform>();
            rect.SetParent(_rectTransform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = layerObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            text = labelObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 28;
            text.raycastTarget = false;
            text.text = string.Empty;
            return image;
        }
    }
}
