using System;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Utils
{
    public static class AudioClipUtils
    {
        public static AudioClip ToAudioClip(Tensor<float> waveform, int sampleRate = 24000, int channels = 1, string name = "AudioClip")
        {
            if (waveform == null)
                throw new ArgumentNullException(nameof(waveform));
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));
            if (channels <= 0 || waveform.count % channels != 0)
                throw new ArgumentOutOfRangeException(nameof(channels), "Channel count must evenly divide the waveform length.");

            var audioClip = AudioClip.Create(name, waveform.count / channels, channels, sampleRate, false);
            if (!audioClip.SetData(waveform.DownloadToArray(), 0))
            {
                UnityEngine.Object.DestroyImmediate(audioClip);
                throw new InvalidOperationException("Unity could not copy the generated waveform into an AudioClip.");
            }
            return audioClip;
        }
    }
}
