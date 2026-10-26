using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.InferenceEngine.Samples.TTS.Inference;
using Unity.InferenceEngine.Samples.TTS.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Editor
{
    public class AppEditorWindow : EditorWindow
    {
        const string k_DefaultText = "Hello! This speech was generated locally in Unity.";

        string m_InputText = k_DefaultText;
        float m_Speed = 1.15f;
        int m_VoiceIndex;
        bool m_IsGenerating;
        string m_Status;
        KokoroHandler m_Handler;
        List<KokoroHandler.Voice> m_Voices = new();
        AudioClip m_AudioClip;

        [MenuItem("Sentis/Sample/Text-To-Speech/Start Kokoro")]
        public static void OpenWindow()
        {
            var window = GetWindow<AppEditorWindow>();
            window.titleContent = new GUIContent("Kokoro Text-To-Speech");
            window.minSize = new Vector2(420, 360);
            window.Show();
        }

        [MenuItem("Sentis/Sample/Text-To-Speech/Start Kokoro", true)]
        public static bool ValidateOpenWindow() => DownloadEditorWindow.AreRequiredFilesPresent();

        void OnEnable()
        {
            titleContent = new GUIContent("Kokoro Text-To-Speech");
            ReloadEngine();
        }

        void OnDisable()
        {
            DisposeEngine();
            DestroyAudioClip();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Kokoro Text-To-Speech", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs Kokoro locally with Unity Inference Engine (Sentis). Generation can take a while on CPU.",
                MessageType.Info);

            EditorGUILayout.LabelField("Text");
            m_InputText = EditorGUILayout.TextArea(m_InputText, GUILayout.MinHeight(110));

            var voiceNames = m_Voices.Select(voice => voice.Name).ToArray();
            using (new EditorGUI.DisabledScope(voiceNames.Length == 0 || m_IsGenerating))
            {
                m_VoiceIndex = EditorGUILayout.Popup("Voice", m_VoiceIndex, voiceNames);
                m_Speed = EditorGUILayout.Slider("Speed", m_Speed, 0.5f, 2f);

                if (GUILayout.Button(m_IsGenerating ? "Generating..." : "Generate and Play", GUILayout.Height(32)))
                    GenerateAndPlay();
            }

            if (m_AudioClip != null)
            {
                EditorGUILayout.LabelField("Last clip", $"{m_AudioClip.length:0.00}s / {m_AudioClip.frequency} Hz");
                if (GUILayout.Button("Play Again"))
                    PlayPreview(m_AudioClip);
            }

            if (!string.IsNullOrWhiteSpace(m_Status))
                EditorGUILayout.HelpBox(m_Status, m_Status.StartsWith("Error:", StringComparison.Ordinal)
                    ? MessageType.Error
                    : MessageType.None);

            if (m_Voices.Count == 0 && GUILayout.Button("Reload TTS Assets"))
                ReloadEngine();
        }

        void ReloadEngine()
        {
            DisposeEngine();
            try
            {
                m_Handler = new KokoroHandler(BackendType.CPU);
                m_Voices = KokoroHandler.GetVoices();
                m_VoiceIndex = Mathf.Clamp(m_VoiceIndex, 0, Math.Max(0, m_Voices.Count - 1));
                m_Status = m_Voices.Count > 0
                    ? $"Ready ({m_Voices.Count} voice{(m_Voices.Count == 1 ? string.Empty : "s")})."
                    : "Error: No voices are listed in Resources/voicesIndex.txt.";
            }
            catch (Exception exception)
            {
                m_Status = $"Error: {exception.Message}";
                Debug.LogException(exception);
            }
        }

        async void GenerateAndPlay()
        {
            if (m_IsGenerating || m_Handler == null || m_Voices.Count == 0)
                return;

            m_IsGenerating = true;
            m_Status = "Generating speech...";
            Repaint();

            try
            {
                var tokens = MisakiSharp.TokenizeGraphemes(m_InputText);
                if (tokens.Length == 0)
                    throw new InvalidOperationException("Enter text containing supported English characters.");

                using var waveform = await m_Handler.Execute(tokens, m_Speed, m_Voices[m_VoiceIndex]);
                DestroyAudioClip();
                m_AudioClip = AudioClipUtils.ToAudioClip(waveform, name: "Kokoro Preview");
                PlayPreview(m_AudioClip);
                m_Status = $"Generated {m_AudioClip.length:0.00} seconds of audio.";
            }
            catch (Exception exception)
            {
                m_Status = $"Error: {exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                m_IsGenerating = false;
                Repaint();
            }
        }

        void DisposeEngine()
        {
            m_Handler?.Dispose();
            m_Handler = null;

            foreach (var voice in m_Voices)
                voice.Dispose();
            m_Voices.Clear();
        }

        void DestroyAudioClip()
        {
            if (m_AudioClip == null)
                return;

            DestroyImmediate(m_AudioClip);
            m_AudioClip = null;
        }

        static void PlayPreview(AudioClip clip)
        {
            var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            var method = audioUtil?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate =>
                    (candidate.Name == "PlayPreviewClip" || candidate.Name == "PlayClip") &&
                    candidate.GetParameters().Length > 0 &&
                    candidate.GetParameters()[0].ParameterType == typeof(AudioClip));

            if (method == null)
                throw new MissingMethodException("Unity's editor audio preview API was not found.");

            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            arguments[0] = clip;
            for (var i = 1; i < parameters.Length; i++)
                arguments[i] = parameters[i].HasDefaultValue
                    ? parameters[i].DefaultValue
                    : parameters[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parameters[i].ParameterType)
                        : null;
            method.Invoke(null, arguments);
        }
    }
}
