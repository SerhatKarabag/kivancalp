using Kivancalp.Core;
using Kivancalp.UI.Views;
using UnityEngine;

namespace Kivancalp.UI.Presentation
{
    public sealed class CardFlipAnimationSystem : ITickable
    {
        private struct ActiveAnimation
        {
            public CardView View;
            public bool TargetFaceUp;
            public bool MidpointApplied;
            public float Duration;
            public float Elapsed;
        }

        private readonly ActiveAnimation[] _activeAnimations;
        private int _activeCount;

        public CardFlipAnimationSystem(int maxAnimationCount)
        {
            _activeAnimations = new ActiveAnimation[maxAnimationCount];
        }

        public void Play(CardView view, bool targetFaceUp, float duration)
        {
            if (view == null)
            {
                return;
            }

            if (duration <= 0f)
            {
                view.SetFaceUpImmediate(targetFaceUp);
                view.SetHorizontalScale(1f);
                return;
            }

            int animationIndex = IndexOf(view);

            if (animationIndex < 0)
            {
                if (_activeCount >= _activeAnimations.Length)
                {
                    return;
                }

                animationIndex = _activeCount;
                _activeCount += 1;
            }

            _activeAnimations[animationIndex] = new ActiveAnimation
            {
                View = view,
                TargetFaceUp = targetFaceUp,
                MidpointApplied = false,
                Duration = duration,
                Elapsed = 0f,
            };
        }

        public void Tick(float deltaTime)
        {
            for (int index = 0; index < _activeCount; index += 1)
            {
                ActiveAnimation animation = _activeAnimations[index];
                animation.Elapsed += deltaTime;
                float progress = animation.Elapsed / animation.Duration;

                if (progress < 0.5f)
                {
                    float phase = progress * 2f;
                    animation.View.SetHorizontalScale(1f - Ease(phase));
                }
                else
                {
                    if (!animation.MidpointApplied)
                    {
                        animation.View.SetFaceUpImmediate(animation.TargetFaceUp);
                        animation.MidpointApplied = true;
                    }

                    float phase = (progress - 0.5f) * 2f;
                    animation.View.SetHorizontalScale(Ease(phase));
                }

                if (progress >= 1f)
                {
                    animation.View.SetFaceUpImmediate(animation.TargetFaceUp);
                    animation.View.SetHorizontalScale(1f);
                    RemoveAt(index);
                    index -= 1;
                    continue;
                }

                _activeAnimations[index] = animation;
            }
        }

        public void Cancel(CardView view)
        {
            if (view == null)
            {
                return;
            }

            int animationIndex = IndexOf(view);

            if (animationIndex < 0)
            {
                return;
            }

            view.SetHorizontalScale(1f);
            RemoveAt(animationIndex);
        }

        private int IndexOf(CardView view)
        {
            for (int index = 0; index < _activeCount; index += 1)
            {
                if (_activeAnimations[index].View == view)
                {
                    return index;
                }
            }

            return -1;
        }

        private void RemoveAt(int index)
        {
            int lastIndex = _activeCount - 1;
            _activeAnimations[index] = _activeAnimations[lastIndex];
            _activeAnimations[lastIndex] = default;
            _activeCount -= 1;
        }

        private static float Ease(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * (3f - (2f * clamped));
        }
    }
}
