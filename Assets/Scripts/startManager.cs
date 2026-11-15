using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class startManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text debugLog;

    [Header("Intro Dialogue")]
    [SerializeField] private GameObject introCanvas;
    [SerializeField] private DialogueTrigger dialogueTrigger;

    
    void Update()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueManager == null || !dialogueManager.IsOpen)
            return;

        bool pressedF =
            Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame;

        bool clicked =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (pressedF || clicked)
            dialogueManager.AdvanceOrClose();
    }

    // Update is called once per frame
    public void continueToIntro()
    {
        if (inputField.text.Length == 0)
        {
            debugLog.text = "We don't hire nameless!";
            return;
        }
        else
        {
            PlayerDataManager dataManager = PlayerDataManager.Instance;
            dataManager.Stats.SetPlayerName(inputField.text);

            canvas.SetActive(false);
            introCanvas.SetActive(true);
            //start intro dialogue
            DialogueManager dM = DialogueManager.Instance;
            DialogueManager.Instance.StartDialogue(dialogueTrigger);

            return;
        }
    }
}
