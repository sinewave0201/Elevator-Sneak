using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorManager : MonoBehaviour
{
    [SerializeField] int elevatorIDX;
    public void interact()
    {
        SceneManager.LoadScene(elevatorIDX);
    }
}
