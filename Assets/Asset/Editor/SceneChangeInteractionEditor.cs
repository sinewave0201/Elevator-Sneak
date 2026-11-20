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

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        string[] sceneNames = new string[sceneCount + 3];

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        sceneNames[sceneCount] = "Go Back";
        sceneNames[sceneCount + 1] = "Next Level";
        sceneNames[sceneCount + 2] = "Previous Level";

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
