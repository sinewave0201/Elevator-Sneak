using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeInteraction : Interaction
{
    [HideInInspector]
    [SerializeField] private int sceneIndex;
    private static int previousSceneIndex = -1;
    public static int PreviousSceneIndex => previousSceneIndex;
    public override void Interact(PlayerController player)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;

        int destinationIdx;

        if (sceneIndex == sceneCount)
        {
            destinationIdx = previousSceneIndex;
        }
        else if (sceneIndex == sceneCount + 1)
        {
            destinationIdx = previousSceneIndex + 1;
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

        previousSceneIndex = currentSceneIdx;//update prev scene idx
        Debug.Log("prevIdx: " + previousSceneIndex);
        Debug.Log("destinationIdx: " + destinationIdx);
        SceneManager.LoadScene(destinationIdx);
    }
}
