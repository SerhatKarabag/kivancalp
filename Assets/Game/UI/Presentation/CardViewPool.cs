using System;
using System.Collections.Generic;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public sealed class CardViewPool : IDisposable
    {
        private readonly RectTransform _parent;
        private readonly Font _font;
        private readonly Stack<CardView> _available;
        private readonly CardView[] _allViews;

        public CardViewPool(RectTransform parent, Font font, int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _parent = parent ? parent : throw new ArgumentNullException(nameof(parent));
            _font = font ? font : throw new ArgumentNullException(nameof(font));
            _available = new Stack<CardView>(capacity);
            _allViews = new CardView[capacity];

            for (int index = 0; index < capacity; index += 1)
            {
                CardView view = CreateCardView(index);
                _allViews[index] = view;
                _available.Push(view);
            }
        }

        public CardView Rent()
        {
            if (_available.Count == 0)
            {
                throw new InvalidOperationException("Card view pool exhausted.");
            }

            CardView view = _available.Pop();
            view.transform.SetParent(_parent, false);
            view.gameObject.SetActive(true);
            return view;
        }

        public void Return(CardView view)
        {
            if (view == null)
            {
                return;
            }

            view.ResetState();
            view.transform.SetParent(_parent, false);
            view.gameObject.SetActive(false);
            _available.Push(view);
        }

        public void Dispose()
        {
            for (int index = 0; index < _allViews.Length; index += 1)
            {
                CardView view = _allViews[index];

                if (view != null)
                {
                    UnityEngine.Object.Destroy(view.gameObject);
                }
            }

            _available.Clear();
        }

        private CardView CreateCardView(int index)
        {
            var cardObject = new GameObject("CardView_" + index, typeof(RectTransform), typeof(CardView));
            var cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.SetParent(_parent, false);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(120f, 160f);

            CardView view = cardObject.GetComponent<CardView>();
            view.Build(_font);
            cardObject.SetActive(false);
            return view;
        }
    }
}
