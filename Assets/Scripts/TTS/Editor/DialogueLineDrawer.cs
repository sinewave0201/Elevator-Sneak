using System;
using Unity.InferenceEngine.Samples.TTS.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Editor
{
    [CustomPropertyDrawer(typeof(DialogueLine))]
    public sealed class DialogueLineDrawer : PropertyDrawer
    {
        const float k_Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var text = property.FindPropertyRelative("text");
            var voice = property.FindPropertyRelative("voice");
            var selectedVoice = property.FindPropertyRelative("selectedVoice");
            var wav = property.FindPropertyRelative("wav");

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(line, label, EditorStyles.boldLabel);
            line.y += line.height + k_Spacing;

            var textHeight = EditorGUI.GetPropertyHeight(text, true);
            line.height = textHeight;
            EditorGUI.PropertyField(line, text, new GUIContent("Text"), true);
            line.y += textHeight + k_Spacing;

            line.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(line, voice, new GUIContent("Voice"));
            line.y += line.height + k_Spacing;

            if (voice.boolValue)
            {
                DrawVoicePopup(line, selectedVoice);
                line.y += line.height + k_Spacing;

                using (new EditorGUI.DisabledScope(true))
                    EditorGUI.PropertyField(line, wav, new GUIContent("Generated WAV"));
                line.y += line.height + k_Spacing;

                line.height = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.HelpBox(line, "Save the DialogueConversation asset to generate or update this WAV automatically.", MessageType.Info);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var text = property.FindPropertyRelative("text");
            var voice = property.FindPropertyRelative("voice");
            var height = EditorGUIUtility.singleLineHeight * 2f +
                         EditorGUI.GetPropertyHeight(text, true) + k_Spacing * 3f;
            if (voice.boolValue)
                height += EditorGUIUtility.singleLineHeight * 4f + k_Spacing * 2f;
            return height;
        }

        static void DrawVoicePopup(Rect position, SerializedProperty selectedVoice)
        {
            try
            {
                var voices = VoicesUtils.GetVoicesList();
                if (voices.Length == 0)
                {
                    EditorGUI.LabelField(position, "Select Voice", "No voices available");
                    return;
                }

                var index = Array.FindIndex(voices, value =>
                    string.Equals(value, selectedVoice.stringValue, StringComparison.OrdinalIgnoreCase));
                if (index < 0) index = 0;
                index = EditorGUI.Popup(position, "Select Voice", index, voices);
                selectedVoice.stringValue = voices[index];
            }
            catch (Exception exception)
            {
                EditorGUI.LabelField(position, "Select Voice", exception.Message);
            }
        }
    }
}
