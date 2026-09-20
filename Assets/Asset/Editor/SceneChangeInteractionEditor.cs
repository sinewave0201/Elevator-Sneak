using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(SceneChangeInteraction))]
public class SceneChangeInteractionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneChangeInteraction interaction =
            (SceneChangeInteraction)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();

        string[] sceneNames = new string[SceneManager.sceneCountInBuildSettings];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        int currentIndex = serializedObject
            .FindProperty("sceneIndex").intValue;

        int newIndex = EditorGUILayout.Popup(
            "Destination",
            currentIndex,
            sceneNames
        );

        if (newIndex != currentIndex)
        {
            serializedObject.FindProperty("sceneIndex").intValue = newIndex;
            serializedObject.ApplyModifiedProperties();
        }
    }
}