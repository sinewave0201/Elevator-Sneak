using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string speakerName = "Stranger";

    [Header("Conversation")]
    [SerializeField, TextArea(2, 4)] private string openingLine = "What do you want?";
    [SerializeField] private string optionOne = "Who are you?";
    [SerializeField, TextArea(2, 4)] private string responseToOptionOne = "That is not important right now.";
    [SerializeField] private string optionTwo = "Goodbye.";
    [SerializeField, TextArea(2, 4)] private string responseToOptionTwo = "Stay out of trouble.";

    public string SpeakerName => speakerName;
    public string OpeningLine => openingLine;
    public string OptionOne => optionOne;
    public string ResponseToOptionOne => responseToOptionOne;
    public string OptionTwo => optionTwo;
    public string ResponseToOptionTwo => responseToOptionTwo;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("No DialogueManager exists in the current scene.", this);
            return;
        }

        DialogueManager.Instance.StartDialogue(this);
    }
}
