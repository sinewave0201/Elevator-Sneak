using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeInteraction : Interaction
{
    [HideInInspector]
    [SerializeField] private int sceneIndex;
    public override void Interact(PlayerController player)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
