using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button optionOneButton;
    [SerializeField] private Button optionTwoButton;
    [SerializeField] private TMP_Text optionOneLabel;
    [SerializeField] private TMP_Text optionTwoLabel;

    public bool IsOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private DialogueTrigger currentDialogue;
    private bool responseIsShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        dialoguePanel.SetActive(false);

        optionOneButton.onClick.AddListener(() => HandleOptionButton(0));
        optionTwoButton.onClick.AddListener(() => HandleOptionButton(1));
    }

    public void StartDialogue(DialogueTrigger dialogue)
    {
        currentDialogue = dialogue;
        responseIsShowing = false;

        speakerText.text = dialogue.SpeakerName;
        dialogueText.text = dialogue.OpeningLine;
        optionOneLabel.text = dialogue.OptionOne;
        optionTwoLabel.text = dialogue.OptionTwo;

        optionOneButton.gameObject.SetActive(true);
        optionTwoButton.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
    }

    public void AdvanceOrClose()
    {
        if (!IsOpen)
            return;

        // Do not close before the player has selected an answer.
        if (responseIsShowing)
            CloseDialogue();
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        currentDialogue = null;
        responseIsShowing = false;
    }

    private void HandleOptionButton(int optionIndex)
    {
        if (responseIsShowing)
        {
            CloseDialogue();
            return;
        }

        SelectOption(optionIndex);
    }

    private void SelectOption(int optionIndex)
    {
        if (currentDialogue == null)
            return;

        dialogueText.text = optionIndex == 0
            ? currentDialogue.ResponseToOptionOne
            : currentDialogue.ResponseToOptionTwo;

        // After the NPC replies, reuse the right-hand option as the exit button.
        optionOneButton.gameObject.SetActive(false);
        optionTwoLabel.text = "OK";
        optionTwoButton.gameObject.SetActive(true);
        responseIsShowing = true;
    }
}
