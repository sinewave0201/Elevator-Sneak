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
        string[] sceneNames = new string[sceneCount + 2];

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        //add two new options:
        //previous entered scene option and next scene index option
        int prevIndex = SceneChangeInteraction.PreviousSceneIndex;
        string prevPath = SceneUtility.GetScenePathByBuildIndex(prevIndex);
        string nextPath = SceneUtility.GetScenePathByBuildIndex(prevIndex + 1);
        
        sceneNames[sceneCount] = "Previous Scene";
        sceneNames[sceneCount + 1] = "Next Scene";

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