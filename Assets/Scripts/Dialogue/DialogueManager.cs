using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Figma terminal UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text nodeLabel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text terminalCursor;
    [SerializeField] private TMP_Text profileNameText;
    [SerializeField] private TMP_Text caseText;
    [SerializeField] private TMP_Text idText;
    [SerializeField] private TMP_Text departmentText;
    [SerializeField] private TMP_Text clearanceText;
    [SerializeField] private TMP_Text recordStatusText;
    [SerializeField] private TMP_Text suspicionValueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject portraitPlaceholder;
    [SerializeField] private SuspicionGraphGraphic suspicionGraph;
    [SerializeField] private RectTransform choicesRoot;
    [SerializeField] private Button choicePrefab;
    [SerializeField, Min(0f)] private float secondsPerCharacter = .018f;

    public bool IsOpen => dialoguePanel != null && dialoguePanel.activeSelf;
    public bool IsTyping => typingRoutine != null;

    readonly List<Button> choices = new();
    DialogueConversation conversation;
    DialogueTrigger trigger;
    DialogueNode node;
    int lineIndex;
    bool choicesShowing;
    Coroutine typingRoutine;
    Coroutine cursorRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueTrigger dialogue)
    {
        if (dialogue == null || dialogue.Conversation == null) return;
        if (!dialogue.Conversation.Validate(out string error)) { Debug.LogError(error, dialogue); return; }
        trigger = dialogue;
        conversation = dialogue.Conversation;
        dialoguePanel.SetActive(true);
        ShowNode(conversation.GetStartNode());
    }

    public void AdvanceOrClose()
    {
        if (!IsOpen || node == null) return;
        if (IsTyping) { FinishTyping(); return; }
        if (choicesShowing) return;
        if (lineIndex < node.DialogueLines.Count - 1) { lineIndex++; ShowLine(); }
        else CloseDialogue();
    }

    public void CloseDialogue()
    {
        StopEffects();
        HideChoices();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        conversation = null; trigger = null; node = null; lineIndex = 0;
    }

    void ShowNode(DialogueNode next)
    {
        if (next == null) { CloseDialogue(); return; }
        node = next; lineIndex = 0;
        DialogueCharacterProfile profile = next.CharacterProfile ?? conversation.DefaultCharacter ?? trigger.CharacterProfile;
        ApplyProfile(profile, next.SpeakerName);
        ShowLine();
    }

    void ShowLine()
    {
        HideChoices();
        nodeLabel.text = $"[DIALOGUE NODE] {node.SpeakerName.ToUpperInvariant()}.";
        dialogueText.text = node.DialogueLines.Count == 0 ? string.Empty : node.DialogueLines[lineIndex].Text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();
        StopEffects();
        terminalCursor.gameObject.SetActive(true);
        typingRoutine = StartCoroutine(TypeLine());
        cursorRoutine = StartCoroutine(BlinkCursor());
    }

    IEnumerator TypeLine()
    {
        int count = dialogueText.textInfo.characterCount;
        for (int i = 1; i <= count; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            if (secondsPerCharacter > 0f) yield return new WaitForSecondsRealtime(secondsPerCharacter); else yield return null;
        }
        typingRoutine = null;
        RevealChoicesIfLastLine();
    }

    IEnumerator BlinkCursor()
    {
        while (true) { terminalCursor.enabled = !terminalCursor.enabled; yield return new WaitForSecondsRealtime(.45f); }
    }

    void FinishTyping()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = null;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        terminalCursor.enabled = true;
        RevealChoicesIfLastLine();
    }

    void RevealChoicesIfLastLine()
    {
        if (lineIndex < node.DialogueLines.Count - 1) return;
        if (node.Options.Count > 0) ShowChoices();
    }

    void ShowChoices()
    {
        EnsureChoiceCount(node.Options.Count);
        for (int i = 0; i < node.Options.Count; i++)
        {
            int captured = i;
            choices[i].onClick.RemoveAllListeners();
            choices[i].onClick.AddListener(() => SelectChoice(captured));
            TMP_Text label = choices[i].GetComponentInChildren<TMP_Text>(true);
            label.text = $"> [{i + 1:00}] {node.Options[i].Text.ToUpperInvariant()}";
            choices[i].gameObject.SetActive(true);
        }
        choicesShowing = true;
    }

    void SelectChoice(int index)
    {
        if (index < 0 || index >= node.Options.Count) return;
        DialogueOption option = node.Options[index];
        if (option.EndsConversation || string.IsNullOrWhiteSpace(option.NextNodeId)) { CloseDialogue(); return; }
        DialogueNode next = conversation.FindNode(option.NextNodeId);
        if (next == null) { Debug.LogError($"Dialogue node '{option.NextNodeId}' was not found.", conversation); CloseDialogue(); return; }
        ShowNode(next);
    }

    void EnsureChoiceCount(int required)
    {
        while (choices.Count < required) choices.Add(Instantiate(choicePrefab, choicesRoot));
        for (int i = 0; i < choices.Count; i++) choices[i].gameObject.SetActive(i < required);
    }

    void HideChoices()
    {
        foreach (Button choice in choices) if (choice != null) choice.gameObject.SetActive(false);
        choicesShowing = false;
    }

    void ApplyProfile(DialogueCharacterProfile profile, string speaker)
    {
        bool portraitSet = profile != null && profile.Portrait != null;
        portraitImage.gameObject.SetActive(portraitSet);
        portraitPlaceholder.SetActive(!portraitSet);
        if (portraitSet) portraitImage.sprite = profile.Portrait;
        profileNameText.text = profile != null ? profile.DisplayName : speaker;
        caseText.text = profile != null ? profile.CaseNumber : "CASE # UNLISTED / REVIEW PENDING";
        idText.text = profile != null ? profile.EmployeeId : "UNLISTED";
        departmentText.text = profile != null ? profile.Department : "UNKNOWN";
        clearanceText.text = profile != null ? profile.Clearance : "RESTRICTED";
        recordStatusText.text = profile != null ? profile.Status : "ACTIVE";
        int suspicion = profile != null ? profile.Suspicion : 18;
        suspicionValueText.text = $"{suspicion}%";
        suspicionGraph.SetSuspicion(suspicion);
    }

    void StopEffects()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        if (cursorRoutine != null) StopCoroutine(cursorRoutine);
        typingRoutine = null; cursorRoutine = null;
        if (terminalCursor != null) terminalCursor.gameObject.SetActive(false);
    }
}
