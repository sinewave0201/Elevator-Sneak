using UnityEngine;
using UnityEngine.SceneManagement;

public class StairManager : MonoBehaviour
{
    [SerializeField] int stairIDX;
    public void interact()
    {
        SceneManager.LoadScene(stairIDX);
    }
}
