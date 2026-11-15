using System;
using System.Linq;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Utils
{
    public static class VoicesUtils
    {
        const string k_IndexFilePath = "voicesIndex";

        public static string[]  GetVoicesList()
        {
            var voicesIndex = Resources.Load<TextAsset>(k_IndexFilePath);
            if (voicesIndex == null)
                throw new InvalidOperationException("Voice index was not found at Resources/voicesIndex.txt.");

            return voicesIndex.text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(voice => voice.Trim())
                .Where(voice => voice.Length > 0)
                .Select(voice => voice.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)
                    ? voice[..^4]
                    : voice)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
