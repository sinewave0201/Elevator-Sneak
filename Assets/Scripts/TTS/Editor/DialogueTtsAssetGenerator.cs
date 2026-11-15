using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.InferenceEngine.Samples.TTS.Inference;
using Unity.InferenceEngine.Samples.TTS.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Editor
{
    [InitializeOnLoad]
    public static class DialogueTtsAssetGenerator
    {
        const string k_OutputRoot = "Assets/GeneratedAudio/Dialogues";
        const string k_GeneratorVersion = "2";
        const float k_GenerationSpeed = 1.15f;

        static readonly HashSet<string> s_QueuedAssets = new(StringComparer.OrdinalIgnoreCase);
        static bool s_IsScheduled;
        static bool s_IsProcessing;
        static bool s_IsWritingAssets;

        static DialogueTtsAssetGenerator()
        {
            EditorApplication.delayCall += QueueAllConversations;
        }

        internal static bool IsWritingAssets => s_IsWritingAssets;

        internal static void Queue(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || s_IsWritingAssets) return;
            s_QueuedAssets.Add(assetPath.Replace('\\', '/'));
            if (s_IsScheduled || s_IsProcessing) return;

            s_IsScheduled = true;
            EditorApplication.delayCall += ProcessQueue;
        }

        static void QueueAllConversations()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(DialogueConversation)}"))
                Queue(AssetDatabase.GUIDToAssetPath(guid));
        }

        [MenuItem("Sentis/Sample/Text-To-Speech/Regenerate Selected Dialogue WAVs")]
        static void RegenerateSelected()
        {
            var conversation = Selection.activeObject as DialogueConversation;
            if (conversation == null)
            {
                EditorUtility.DisplayDialog("Dialogue TTS", "Select a DialogueConversation asset first.", "OK");
                return;
            }

            foreach (var node in conversation.Nodes)
            foreach (var line in node.DialogueLines)
                if (line.VoiceEnabled) line.SetGeneratedWav(null, string.Empty, line.GeneratedWavPath);

            EditorUtility.SetDirty(conversation);
            Queue(AssetDatabase.GetAssetPath(conversation));
        }

        static async void ProcessQueue()
        {
            s_IsScheduled = false;
            if (s_IsProcessing || s_QueuedAssets.Count == 0) return;

            s_IsProcessing = true;
            var paths = s_QueuedAssets.ToArray();
            s_QueuedAssets.Clear();

            try
            {
                using var session = new GenerationSession();
                for (var i = 0; i < paths.Length; i++)
                {
                    var conversation = AssetDatabase.LoadAssetAtPath<DialogueConversation>(paths[i]);
                    if (conversation == null) continue;

                    EditorUtility.DisplayProgressBar(
                        "Generating dialogue speech",
                        conversation.name,
                        paths.Length == 0 ? 1f : (float)i / paths.Length);
                    await ProcessConversation(conversation, paths[i], session);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                s_IsProcessing = false;
                if (s_QueuedAssets.Count > 0 && !s_IsScheduled)
                {
                    s_IsScheduled = true;
                    EditorApplication.delayCall += ProcessQueue;
                }
            }
        }

        static async Task ProcessConversation(
            DialogueConversation conversation,
            string conversationPath,
            GenerationSession session)
        {
            var changed = false;
            var assetGuid = AssetDatabase.AssetPathToGUID(conversationPath);
            var conversationFolder = $"{Sanitize(conversation.name)}_{assetGuid[..Math.Min(8, assetGuid.Length)]}";

            for (var nodeIndex = 0; nodeIndex < conversation.Nodes.Count; nodeIndex++)
            {
                var node = conversation.Nodes[nodeIndex];
                if (node == null) continue;

                for (var lineIndex = 0; lineIndex < node.DialogueLines.Count; lineIndex++)
                {
                    var line = node.DialogueLines[lineIndex];
                    if (line == null) continue;

                    if (!line.VoiceEnabled)
                    {
                        changed |= DeleteGeneratedWav(line.GeneratedWavPath);
                        if (line.Wav != null || !string.IsNullOrEmpty(line.GeneratedFingerprint) ||
                            !string.IsNullOrEmpty(line.GeneratedWavPath))
                        {
                            line.ClearGeneratedWav();
                            changed = true;
                        }
                        continue;
                    }

                    session.EnsureVoicesLoaded();
                    var voiceName = string.IsNullOrWhiteSpace(line.SelectedVoice)
                        ? session.Voices[0].Name
                        : line.SelectedVoice;
                    line.SetDefaultVoice(voiceName);

                    var voice = session.Voices.FirstOrDefault(candidate =>
                        string.Equals(candidate.Name, voiceName, StringComparison.OrdinalIgnoreCase));
                    if (voice == null)
                    {
                        DeleteGeneratedWav(line.GeneratedWavPath);
                        line.ClearGeneratedWav();
                        changed = true;
                        Debug.LogError($"Dialogue line selects missing TTS voice '{voiceName}'.", conversation);
                        continue;
                    }

                    var nodeName = string.IsNullOrWhiteSpace(node.Id) ? $"node_{nodeIndex}" : node.Id;
                    var targetPath = $"{k_OutputRoot}/{conversationFolder}/" +
                        $"{nodeIndex:00}_{Sanitize(nodeName)}_{lineIndex:00}_{Sanitize(voice.Name)}.wav";
                    var fingerprint = Hash128.Compute(
                        $"{k_GeneratorVersion}\n{k_GenerationSpeed:R}\n{line.Text}\n{voice.Name}").ToString();

                    if (line.Wav != null && line.GeneratedFingerprint == fingerprint &&
                        string.Equals(line.GeneratedWavPath, targetPath, StringComparison.OrdinalIgnoreCase) &&
                        AssetDatabase.LoadAssetAtPath<AudioClip>(targetPath) != null)
                        continue;

                    var oldPath = line.GeneratedWavPath;
                    try
                    {
                        await GenerateWav(line.Text, voice, targetPath, session.Handler);
                        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(targetPath);
                        if (clip == null)
                            throw new InvalidOperationException($"Unity could not import generated WAV '{targetPath}'.");

                        line.SetGeneratedWav(clip, fingerprint, targetPath);
                        changed = true;
                        if (!string.Equals(oldPath, targetPath, StringComparison.OrdinalIgnoreCase))
                            DeleteGeneratedWav(oldPath);
                    }
                    catch (Exception exception)
                    {
                        DeleteGeneratedWav(targetPath);
                        DeleteGeneratedWav(oldPath);
                        line.ClearGeneratedWav();
                        changed = true;
                        Debug.LogError(
                            $"Failed to generate dialogue WAV for '{conversation.name}', node '{nodeName}', " +
                            $"line {lineIndex}: {exception.Message}",
                            conversation);
                        Debug.LogException(exception);
                    }
                }
            }

            if (!changed) return;
            s_IsWritingAssets = true;
            try
            {
                EditorUtility.SetDirty(conversation);
                AssetDatabase.SaveAssetIfDirty(conversation);
            }
            finally
            {
                s_IsWritingAssets = false;
            }
        }

        static async Task GenerateWav(
            string text,
            KokoroHandler.Voice voice,
            string targetAssetPath,
            KokoroHandler handler)
        {
            var tokens = MisakiSharp.TokenizeGraphemes(text);
            if (tokens.Length == 0)
                throw new InvalidOperationException("The dialogue text does not contain supported speech tokens.");

            using var waveform = await handler.Execute(tokens, k_GenerationSpeed, voice);
            var absoluteTargetPath = ToAbsolutePath(targetAssetPath);
            var directory = Path.GetDirectoryName(absoluteTargetPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            var temporaryPath = Path.GetTempFileName();
            try
            {
                WavUtils.WriteFloatWav(temporaryPath, waveform.DownloadToArray());
                File.Copy(temporaryPath, absoluteTargetPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }

            AssetDatabase.ImportAsset(
                targetAssetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(targetAssetPath) is AudioImporter importer)
            {
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        static bool DeleteGeneratedWav(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) ||
                !assetPath.StartsWith(k_OutputRoot + "/", StringComparison.OrdinalIgnoreCase))
                return false;
            return AssetDatabase.DeleteAsset(assetPath);
        }

        static string ToAbsolutePath(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Unity project root could not be resolved.");
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unnamed";
            var invalid = Path.GetInvalidFileNameChars();
            var result = new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
            return result.Trim().Replace(' ', '_');
        }

        sealed class GenerationSession : IDisposable
        {
            public KokoroHandler Handler { get; private set; }
            public List<KokoroHandler.Voice> Voices { get; private set; }

            public void EnsureVoicesLoaded()
            {
                if (Voices != null) return;
                Handler = new KokoroHandler(BackendType.CPU);
                Voices = KokoroHandler.GetVoices();
                if (Voices.Count == 0)
                    throw new InvalidOperationException("No TTS voices are available.");
            }

            public void Dispose()
            {
                Handler?.Dispose();
                if (Voices == null) return;
                foreach (var voice in Voices) voice.Dispose();
            }
        }
    }

    public sealed class DialogueTtsSaveProcessor : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            if (DialogueTtsAssetGenerator.IsWritingAssets) return paths;
            foreach (var path in paths)
            {
                if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) continue;
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(DialogueConversation))
                    DialogueTtsAssetGenerator.Queue(path);
            }
            return paths;
        }
    }
}
