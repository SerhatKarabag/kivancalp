using System;
using Kivancalp.Core.Logging;
using Kivancalp.Gameplay.Contracts;
using UnityEngine;

namespace Kivancalp.Infrastructure.Audio
{
    public sealed class GameAudioFeedbackService : IGameAudio, IDisposable
    {
        private readonly AudioSource _audioSource;
        private readonly IGameLogger _logger;
        private readonly AudioClip[] _clips;

        public GameAudioFeedbackService(AudioSource audioSource, IGameLogger logger)
        {
            _audioSource = audioSource ? audioSource : throw new ArgumentNullException(nameof(audioSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _clips = new AudioClip[4];

            _clips[(int)SoundEffectType.Flip] = ProceduralSfxFactory.CreateTone("sfx_flip", 1100f, 0.05f, 0.08f);
            _clips[(int)SoundEffectType.Match] = ProceduralSfxFactory.CreateTone("sfx_match", 760f, 0.16f, 0.11f);
            _clips[(int)SoundEffectType.Mismatch] = ProceduralSfxFactory.CreateTone("sfx_mismatch", 180f, 0.22f, 0.13f);
            _clips[(int)SoundEffectType.GameOver] = ProceduralSfxFactory.CreateTone("sfx_game_over", 420f, 0.45f, 0.16f);
        }

        public void Play(SoundEffectType effectType)
        {
            int clipIndex = (int)effectType;

            if (clipIndex < 0 || clipIndex >= _clips.Length)
            {
                _logger.Warning("Requested sound effect index is out of range: " + clipIndex);
                return;
            }

            AudioClip clip = _clips[clipIndex];

            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip);
        }

        public void Dispose()
        {
            for (int clipIndex = 0; clipIndex < _clips.Length; clipIndex += 1)
            {
                AudioClip clip = _clips[clipIndex];

                if (clip != null)
                {
                    UnityEngine.Object.Destroy(clip);
                    _clips[clipIndex] = null;
                }
            }
        }
    }
}
