using System;
using System.Collections.Generic;
using System.IO;
using Unity.InferenceEngine.Samples.TTS.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Editor
{
    public class DownloadEditorWindow : EditorWindow
    {
        const string k_ModelId = "onnx-community/Kokoro-82M-v1.0-ONNX";
        const string k_ResourceDirectory = "Scripts/TTS/Resources";
        const string k_ModelPath = "onnx/model.onnx";

        bool m_IsDownloading;
        float m_Progress;
        string m_CurrentFile;
        string m_Error;

        static string DownloadDirectory => Path.Combine(Application.dataPath, k_ResourceDirectory);

        [MenuItem("Sentis/Sample/Text-To-Speech/Download Models")]
        public static void OpenWindow()
        {
            var window = GetWindow<DownloadEditorWindow>();
            window.titleContent = new GUIContent("Kokoro Model Files");
            window.minSize = new Vector2(480, 260);
            window.Show();
        }

        public static bool AreRequiredFilesPresent()
        {
            try
            {
                foreach (var file in GetRequiredFiles())
                {
                    if (!File.Exists(Path.Combine(DownloadDirectory, file)))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Kokoro model files", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Files are stored under Assets/{k_ResourceDirectory}. Existing files are left untouched.",
                MessageType.Info);

            foreach (var file in GetRequiredFiles())
            {
                var exists = File.Exists(Path.Combine(DownloadDirectory, file));
                EditorGUILayout.LabelField(exists ? "Ready" : "Missing", file);
            }

            if (m_IsDownloading)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Downloading {m_CurrentFile}");
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), m_Progress, $"{m_Progress:P0}");
            }

            if (!string.IsNullOrEmpty(m_Error))
                EditorGUILayout.HelpBox(m_Error, MessageType.Error);

            var allFilesReady = AreRequiredFilesPresent();
            using (new EditorGUI.DisabledScope(m_IsDownloading || allFilesReady))
            {
                if (GUILayout.Button(allFilesReady ? "All Files Are Ready" : "Download Missing Files", GUILayout.Height(32)))
                    DownloadMissingFiles();
            }
        }

        async void DownloadMissingFiles()
        {
            m_IsDownloading = true;
            m_Error = null;

            try
            {
                var downloader = new HfDownloader(DownloadDirectory, k_ModelId);
                foreach (var file in GetRequiredFiles())
                {
                    if (File.Exists(Path.Combine(DownloadDirectory, file)))
                        continue;

                    m_CurrentFile = file;
                    m_Progress = 0f;
                    var progress = new Progress<float>(value =>
                    {
                        m_Progress = value;
                        Repaint();
                    });
                    await downloader.Download(file, progress);
                }

                AssetDatabase.Refresh();
            }
            catch (Exception exception)
            {
                m_Error = exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                m_IsDownloading = false;
                Repaint();
            }
        }

        static List<string> GetRequiredFiles()
        {
            var files = new List<string> { k_ModelPath };
            foreach (var voice in VoicesUtils.GetVoicesList())
                files.Add("voices/" + voice + ".bin");
            return files;
        }
    }
}
