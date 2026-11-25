using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StairCaseManager : MonoBehaviour
{
    [SerializeField] private GameObject block1;
    [SerializeField] private GameObject block5;

    private void Awake()
    {
        int sourceSceneIndex = SceneChangeInteraction.GoBackSceneIndex;
        string sourceSceneName = sourceSceneIndex >= 0
            ? Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sourceSceneIndex))
            : string.Empty;

        if (block1 != null)
            block1.SetActive(sourceSceneName == "Level 1");

        if (block5 != null)
            block5.SetActive(sourceSceneName == "Level 5");
    }
}
