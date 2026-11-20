using TMPro;
using UnityEngine;

public class ElevatorFloorDisplayStyle : MonoBehaviour
{
    [SerializeField] private TMP_Text floorText;
    [SerializeField] private RectTransform directionArrow;

    private int previousFloor;
    private bool hasPreviousFloor;

    private void OnEnable()
    {
        hasPreviousFloor = TryReadFloor(out previousFloor);
        if (directionArrow != null)
            directionArrow.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!TryReadFloor(out int currentFloor))
            return;

        if (!hasPreviousFloor)
        {
            previousFloor = currentFloor;
            hasPreviousFloor = true;
            return;
        }

        if (currentFloor == previousFloor)
            return;

        if (directionArrow != null)
        {
            directionArrow.gameObject.SetActive(true);
            directionArrow.localRotation = Quaternion.Euler(0f, 0f, currentFloor > previousFloor ? 0f : 180f);
        }

        previousFloor = currentFloor;
    }

    private bool TryReadFloor(out int floor)
    {
        floor = 0;
        return floorText != null && int.TryParse(floorText.text.Trim(), out floor);
    }
}
