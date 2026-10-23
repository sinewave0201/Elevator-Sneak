
using UnityEngine;

public class DialogueInteraction : Interaction
{
    [SerializeField] private DialogueTrigger dialogueTrigger;

    public override bool CanInteract(PlayerController player)
    {
        return base.CanInteract(player)
            && dialogueTrigger != null
            && DialogueManager.Instance != null
            && !DialogueManager.Instance.IsOpen;
    }

    public override void Interact(PlayerController player)
    {
        if (dialogueTrigger == null)
        {
            Debug.LogError(
                "DialogueInteraction has no DialogueTrigger assigned.",
                this);
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError(
                "No DialogueManager exists in the current scene.",
                this);
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueTrigger);
    }
}
