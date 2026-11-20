using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeInteraction : Interaction
{
    [HideInInspector]
    [SerializeField] private int sceneIndex;
    private static int goBackSceneIndex = -1;
    public static int GoBackSceneIndex => goBackSceneIndex;

    public override void Interact(PlayerController player)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;

        int goBack = sceneCount;
        int nextLevel = sceneCount + 1;
        int prevLevel = sceneCount + 2;

        int destinationIdx;

        if (sceneIndex == goBack)
        {
            destinationIdx = goBackSceneIndex;
        }
        else if (sceneIndex == nextLevel)
        {
            destinationIdx = goBackSceneIndex + 1;
        }
        else if (sceneIndex == prevLevel)
        {
            destinationIdx = goBackSceneIndex - 1;
        }
        else
        {
            destinationIdx = sceneIndex;
            //load scene index
        }

        if (destinationIdx < 0 || destinationIdx >= sceneCount)
        {
            Debug.LogWarning($"Invalid destination index: {destinationIdx}");
            return;
        }

        goBackSceneIndex = currentSceneIdx;
        Debug.Log("goBackIdx: " + goBackSceneIndex);
        Debug.Log("destinationIdx: " + destinationIdx);
        SceneManager.LoadScene(destinationIdx);
    }
}
