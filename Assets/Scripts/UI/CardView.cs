using System;
using Kivancalp.Gameplay.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kivancalp.UI.Views
{
    public sealed class CardView : UIBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private RectTransform _rectTransform;
        private Image _hitImage;
        private Image _frontImage;
        private Image _frontInnerImage;
        private Image _backImage;
        private Image _backInnerImage;
        private Image _matchedOverlay;
        private Image _hoverOverlay;
        private Text _faceLabel;
        private UiThemeConfig.CardStyle _style;
        private Action<int> _clickHandler;
        private int _cardIndex;
        private bool _isBuilt;
        private Color _baseFaceColor;

        public RectTransform RectTransform => _rectTransform;

        public void Build(Font font, UiThemeConfig.CardStyle style)
        {
            if (_isBuilt)
            {
                return;
            }

            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            _style = style ?? throw new ArgumentNullException(nameof(style));
            _style.EnsureInitialized();

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
            _hitImage.raycastTarget = true;

            CreateDropShadow();

            _backImage = CreateLayerContainer("Back", _style.backColor);
            _backInnerImage = CreateInnerPanel(_backImage.rectTransform, "BackInner", _style.backInnerColor);
            CreateBackPattern(_backInnerImage.rectTransform);

            _frontImage = CreateLayerContainer("Front", Color.white);
            _frontInnerImage = CreateInnerPanel(_frontImage.rectTransform, "FrontInner", Color.white);
            CreateTopShine(_frontInnerImage.rectTransform, "FrontShine");

            _faceLabel = CreateFaceLabel(_frontInnerImage.rectTransform, font);
            _matchedOverlay = CreateOverlay("MatchedOverlay", _frontInnerImage.rectTransform, _style.matchedOverlayColor);
            _hoverOverlay = CreateOverlay("HoverOverlay", _rectTransform, _style.hoverOverlayColor);

            _matchedOverlay.enabled = false;
            _hoverOverlay.enabled = false;

            SetFaceUpImmediate(false);
            SetHorizontalScale(1f);
            _isBuilt = true;
        }

        public void Bind(int cardIndex, int faceId, Action<int> clickHandler)
        {
            _cardIndex = cardIndex;
            _clickHandler = clickHandler;

            Color[] faceColors = _style.faceColors;
            _baseFaceColor = faceColors[faceId % faceColors.Length];

            _frontImage.color = _baseFaceColor;
            _frontInnerImage.color = Tint(_baseFaceColor, _style.frontTintAmount);
            _faceLabel.text = (faceId + 1).ToString();
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

        private void CreateDropShadow()
        {
            var shadowObject = new GameObject("Shadow", typeof(RectTransform), typeof(Image));
            var shadowRect = shadowObject.GetComponent<RectTransform>();
            shadowRect.SetParent(_rectTransform, false);
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = _style.shadowOffset;
            shadowRect.offsetMax = _style.shadowOffset;

            Image shadowImage = shadowObject.GetComponent<Image>();
            shadowImage.color = _style.shadowColor;
            shadowImage.raycastTarget = false;
        }

        private Image CreateLayerContainer(string name, Color color)
        {
            var layerObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            var layerRect = layerObject.GetComponent<RectTransform>();
            layerRect.SetParent(_rectTransform, false);
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.offsetMin = Vector2.zero;
            layerRect.offsetMax = Vector2.zero;

            Image image = layerObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            Outline outline = layerObject.GetComponent<Outline>();
            outline.effectColor = _style.outlineColor;
            outline.effectDistance = _style.outlineOffset;

            return image;
        }

        private Image CreateInnerPanel(RectTransform parent, string name, Color color)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(_style.innerPadding, _style.innerPadding);
            panelRect.offsetMax = new Vector2(-_style.innerPadding, -_style.innerPadding);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = color;
            panelImage.raycastTarget = false;
            return panelImage;
        }

        private void CreateBackPattern(RectTransform parent)
        {
            int stripeCount = _style.backStripeCount;

            for (int index = 0; index < stripeCount; index += 1)
            {
                float verticalAnchor = _style.backStripeStartY + (index * _style.backStripeStep);
                var stripeObject = new GameObject("Stripe_" + index, typeof(RectTransform), typeof(Image));
                var stripeRect = stripeObject.GetComponent<RectTransform>();
                stripeRect.SetParent(parent, false);
                stripeRect.anchorMin = new Vector2(0.08f, verticalAnchor);
                stripeRect.anchorMax = new Vector2(0.92f, verticalAnchor + _style.backStripeHeight);
                stripeRect.offsetMin = Vector2.zero;
                stripeRect.offsetMax = Vector2.zero;

                Image stripeImage = stripeObject.GetComponent<Image>();
                stripeImage.color = _style.backStripeColor;
                stripeImage.raycastTarget = false;
            }
        }

        private void CreateTopShine(RectTransform parent, string name)
        {
            var shineObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var shineRect = shineObject.GetComponent<RectTransform>();
            shineRect.SetParent(parent, false);
            shineRect.anchorMin = new Vector2(0f, _style.topShineStartY);
            shineRect.anchorMax = Vector2.one;
            shineRect.offsetMin = Vector2.zero;
            shineRect.offsetMax = Vector2.zero;

            Image shineImage = shineObject.GetComponent<Image>();
            shineImage.color = _style.topShineColor;
            shineImage.raycastTarget = false;
        }

        private Text CreateFaceLabel(RectTransform parent, Font font)
        {
            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Outline));
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(parent, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text text = labelObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = _style.labelFontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = _style.labelBestFitMinSize;
            text.resizeTextMaxSize = _style.labelBestFitMaxSize;

            Outline outline = labelObject.GetComponent<Outline>();
            outline.effectColor = _style.outlineColor;
            outline.effectDistance = _style.outlineOffset;

            return text;
        }

        private static Image CreateOverlay(string name, RectTransform parent, Color color)
        {
            var overlayObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            var overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.SetParent(parent, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = color;
            overlayImage.raycastTarget = false;
            return overlayImage;
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
