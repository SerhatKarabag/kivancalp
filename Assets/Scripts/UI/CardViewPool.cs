using System;
using System.Collections.Generic;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public sealed class CardViewPool : IDisposable
    {
        private readonly RectTransform _parent;
        private readonly CardView _prefab;
        private readonly UiThemeConfig.CardStyle _cardStyle;
        private readonly Stack<CardView> _available;
        private readonly CardView[] _allViews;

        public CardViewPool(RectTransform parent, CardView prefab, UiThemeConfig.CardStyle cardStyle, int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _parent = parent ? parent : throw new ArgumentNullException(nameof(parent));
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _cardStyle = cardStyle ?? throw new ArgumentNullException(nameof(cardStyle));
            _cardStyle.EnsureInitialized();
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
            CardView view = UnityEngine.Object.Instantiate(_prefab, _parent);
            view.gameObject.name = "CardView_" + index;
            view.Initialize(_cardStyle);
            view.gameObject.SetActive(false);
            return view;
        }
    }
}
