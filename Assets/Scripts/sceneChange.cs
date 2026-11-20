using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [SerializeField] private int mainScreenIdx;

    public void ChangeScene()
    {
        SceneManager.LoadScene(mainScreenIdx);
    }
}