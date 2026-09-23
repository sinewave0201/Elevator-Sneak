using System.Collections.Generic;
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

    private readonly List<Button> optionButtons = new List<Button>();
    private DialogueConversation currentConversation;
    private DialogueNode currentNode;
    private int currentLineIndex;
    private bool optionsAreShowing;
    private bool closeOptionIsShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        optionButtons.Add(optionOneButton);

        if (optionTwoButton != optionOneButton)
            optionButtons.Add(optionTwoButton);

        HideOptions();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueTrigger dialogue)
    {
        if (dialogue == null)
            return;

        DialogueConversation conversation = dialogue.Conversation;

        if (conversation == null)
        {
            Debug.LogError("Cannot start dialogue: no conversation is assigned.", dialogue);
            return;
        }

        if (!conversation.Validate(out string error))
        {
            Debug.LogError($"Cannot start dialogue: {error}", dialogue);
            return;
        }

        currentConversation = conversation;
        dialoguePanel.SetActive(true);
        ShowNode(conversation.GetStartNode());
    }

    public void AdvanceOrClose()
    {
        if (!IsOpen || currentNode == null)
            return;

        if (closeOptionIsShowing)
        {
            CloseDialogue();
            return;
        }

        // An explicit option must be selected before the conversation can continue.
        if (optionsAreShowing)
            return;

        if (currentLineIndex < currentNode.DialogueLines.Count - 1)
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        HideOptions();
        currentConversation = null;
        currentNode = null;
        currentLineIndex = 0;
    }

    private void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            CloseDialogue();
            return;
        }

        currentNode = node;
        currentLineIndex = 0;
        speakerText.text = node.SpeakerName;
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        HideOptions();

        dialogueText.text = currentNode.DialogueLines.Count > 0
            ? currentNode.DialogueLines[currentLineIndex].Text
            : string.Empty;

        bool isLastLine = currentNode.DialogueLines.Count == 0 ||
                          currentLineIndex >= currentNode.DialogueLines.Count - 1;

        if (!isLastLine)
            return;

        if (currentNode.Options.Count > 0)
            ShowDialogueOptions();
        else
            ShowCloseOption();
    }

    private void ShowDialogueOptions()
    {
        EnsureButtonCount(currentNode.Options.Count);

        for (int i = 0; i < currentNode.Options.Count; i++)
        {
            int optionIndex = i;
            DialogueOption option = currentNode.Options[i];
            Button button = optionButtons[i];

            ConfigureButton(button, option.Text, () => SelectOption(optionIndex));
            SetButtonLayout(button, i, currentNode.Options.Count);
        }

        optionsAreShowing = true;
        closeOptionIsShowing = false;
    }

    private void ShowCloseOption()
    {
        EnsureButtonCount(1);
        ConfigureButton(optionButtons[0], "OK", CloseDialogue);
        SetButtonLayout(optionButtons[0], 0, 1);
        optionsAreShowing = true;
        closeOptionIsShowing = true;
    }

    private void SelectOption(int optionIndex)
    {
        if (currentNode == null || optionIndex < 0 || optionIndex >= currentNode.Options.Count)
            return;

        DialogueOption option = currentNode.Options[optionIndex];

        if (option.EndsConversation || string.IsNullOrWhiteSpace(option.NextNodeId))
        {
            CloseDialogue();
            return;
        }

        DialogueNode nextNode = currentConversation.FindNode(option.NextNodeId);

        if (nextNode == null)
        {
            Debug.LogError($"Dialogue node '{option.NextNodeId}' could not be found.", currentConversation);
            CloseDialogue();
            return;
        }

        ShowNode(nextNode);
    }

    private void EnsureButtonCount(int requiredCount)
    {
        while (optionButtons.Count < requiredCount)
        {
            Button button = Instantiate(optionOneButton, optionOneButton.transform.parent);
            button.name = $"DialogueOption{optionButtons.Count + 1}";
            optionButtons.Add(button);
        }

        for (int i = 0; i < optionButtons.Count; i++)
            optionButtons[i].gameObject.SetActive(i < requiredCount);
    }

    private void ConfigureButton(Button button, string labelText, UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        TMP_Text label = GetButtonLabel(button);
        if (label != null)
            label.text = labelText;

        button.gameObject.SetActive(true);
    }

    private TMP_Text GetButtonLabel(Button button)
    {
        if (button == optionOneButton)
            return optionOneLabel;

        if (button == optionTwoButton)
            return optionTwoLabel;

        return button.GetComponentInChildren<TMP_Text>(true);
    }

    private static void SetButtonLayout(Button button, int index, int totalCount)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        int columns = totalCount == 1 ? 1 : 2;
        int rows = Mathf.CeilToInt(totalCount / (float)columns);
        int column = index % columns;
        int row = index / columns;
        const float left = 0.04f;
        const float right = 0.96f;
        const float bottom = 0.02f;
        const float top = 0.33f;
        const float gap = 0.005f;

        float columnWidth = (right - left) / columns;
        float rowHeight = (top - bottom) / rows;
        float rowTop = top - row * rowHeight;

        rect.anchorMin = new Vector2(left + column * columnWidth + gap, rowTop - rowHeight + gap);
        rect.anchorMax = new Vector2(left + (column + 1) * columnWidth - gap, rowTop - gap);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void HideOptions()
    {
        foreach (Button button in optionButtons)
        {
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }

        optionsAreShowing = false;
        closeOptionIsShowing = false;
    }
}
