using UnityEngine;

namespace Kivancalp.Infrastructure.Audio
{
    internal static class ProceduralSfxFactory
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateTone(string clipName, float frequency, float durationSeconds, float volume)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(durationSeconds * SampleRate));
            float[] samples = new float[sampleCount];
            float increment = 2f * Mathf.PI * frequency / SampleRate;
            float phase = 0f;

            for (int index = 0; index < sampleCount; index += 1)
            {
                float envelope = 1f - ((float)index / sampleCount);
                samples[index] = Mathf.Sin(phase) * volume * envelope;
                phase += increment;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
