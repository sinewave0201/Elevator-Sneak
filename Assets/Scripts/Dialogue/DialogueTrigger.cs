using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueConversation conversation;
    [Header("Character")]
    [SerializeField] private DialogueCharacterProfile characterProfile;
    [SerializeField] private string speakerName = "Stranger";
    [Header("Legacy Conversation")]
    [SerializeField, TextArea(2, 4)] private string openingLine = "What do you want?";
    [SerializeField] private string optionOne = "Who are you?";
    [SerializeField, TextArea(2, 4)] private string responseToOptionOne = "That is not important right now.";
    [SerializeField] private string optionTwo = "Goodbye.";
    [SerializeField, TextArea(2, 4)] private string responseToOptionTwo = "Stay out of trouble.";
    public DialogueCharacterProfile CharacterProfile => characterProfile;
    private DialogueConversation legacyConversation;
    public DialogueConversation Conversation => conversation != null ? conversation : legacyConversation ??= DialogueConversation.CreateLegacy(speakerName, openingLine, optionOne, responseToOptionOne, optionTwo, responseToOptionTwo);
    void Reset() => GetComponent<Collider2D>().isTrigger = true;
    public void Interact() { if (DialogueManager.Instance == null) { Debug.LogError("No DialogueManager exists in the current scene.", this); return; } DialogueManager.Instance.StartDialogue(this); }
    void OnDestroy() { if (legacyConversation != null) Destroy(legacyConversation); }
}
